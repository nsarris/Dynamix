using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection.Emit
{
    public static class FieldAccessorMethodEmitter
    {
        public static TDelegate GetFieldSetter<TDelegate>(FieldInfo field)
            where TDelegate : class
        {
            if (!(GetFieldSetter(field, typeof(TDelegate)) is TDelegate ret))
                throw new InvalidCastException("Constructed field setter cannot be casted to requested delegate type");
            return ret;
        }

        public static Delegate GetFieldSetter(FieldInfo field, Type delegateType = null)
        {
            var method = GetFieldSetterMethod(field, delegateType);
            if (delegateType == null)
                delegateType = GenericTypeExtensions.GetFuncGenericType((field.IsStatic ? new[] { field.DeclaringType } : Type.EmptyTypes).Concat(new[] { field.FieldType }));
            return method.CreateDelegate(delegateType);
        }

        public static DynamicMethod GetFieldSetterMethod(FieldInfo field, Type delegateType = null)
        {
            //TODO check delegate type compatibility, must be Action<castedT,castedTProp>

            var paramTypes = delegateType == null ?
                (!field.IsStatic ? new Type[] { field.DeclaringType } : Type.EmptyTypes).Concat(new[] { field.FieldType }) :
                delegateType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray();

            var instanceType = !field.IsStatic ? paramTypes.First() : null;
            var valueType = paramTypes.Skip(field.IsStatic ? 0 : 1).First();

            var dyn = new DynamicMethod("set_" + field.Name, typeof(void), paramTypes.ToArray(), field.DeclaringType, true);
            var il = dyn.GetILGenerator();

            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_0);
                EmitCast(il, valueType, field.FieldType);
                il.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                EmitThis(il, instanceType, field.DeclaringType);
                il.Emit(OpCodes.Ldarg_1);
                EmitCast(il, valueType, field.FieldType);
                il.Emit(OpCodes.Stfld, field);
            }

            il.Emit(OpCodes.Ret);
            return dyn;
        }

        public static TDelegate GetFieldGetter<TDelegate>(FieldInfo field)
            where TDelegate : class
        {
            if (!(GetFieldGetter(field, typeof(TDelegate)) is TDelegate ret))
                throw new InvalidCastException("Constructed field getter cannot be casted to requested delegate type");
            return ret;
        }

        public static Delegate GetFieldGetter(FieldInfo field, Type delegateType = null)
        {
            var method = GetFieldSetterMethod(field, delegateType);
            if (delegateType == null)
                delegateType = GenericTypeExtensions.GetFuncGenericType((field.IsStatic ? new[] { field.DeclaringType } : new Type[] { }).Concat(new[] { field.FieldType }));
            return (method.CreateDelegate(delegateType));
        }

        public static DynamicMethod GetFieldGetterMethod(FieldInfo field, Type delegateType = null)
        {
            //TODO check delegate type compatibility, must be Func<castedT,castedTProp>
            var delMethod = delegateType?.GetMethod("Invoke");

            var paramTypes = delegateType == null ?
                !field.IsStatic ? new Type[] { field.DeclaringType } : Type.EmptyTypes :
                delMethod.GetParameters().Select(x => x.ParameterType).ToArray();

            var instanceType = !field.IsStatic ? paramTypes.First() : null;
            var valueType = delegateType == null ? field.FieldType : delMethod.ReturnType;

            var dyn = new DynamicMethod("get_" + field.Name, valueType, paramTypes.ToArray(), field.DeclaringType, true);
            var il = dyn.GetILGenerator();
            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, field);
            }
            else
            {
                EmitThis(il, instanceType, field.DeclaringType);
                il.Emit(OpCodes.Ldfld, field);
            }
            EmitCast(il, field.FieldType, valueType);
            il.Emit(OpCodes.Ret);
            return dyn;
        }

        private static void EmitThis(ILGenerator il, Type argType, Type privateType)
        {
            il.Emit(OpCodes.Ldarg_0);
            if (argType.IsByRef || argType.IsPointer)
            {
                argType = argType.GetElementType();
                if (argType == privateType)
                {
                    if (argType.IsValueType)
                    {
                        return;
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldobj, argType);
                    }
                }
                else if (argType.IsValueType)
                {
                    il.Emit(OpCodes.Ldobj, argType);
                }
            }
            EmitCast(il, argType, privateType);
        }

        private static void EmitCast(ILGenerator il, Type from, Type to)
        {
            if (from.IsValueType && !to.IsValueType)
            {
                il.Emit(OpCodes.Box, from);
            }
            else if (!from.IsValueType && to.IsValueType)
            {
                il.Emit(OpCodes.Unbox, to);
                return;
            }
            if (from.IsEnum && to == Enum.GetUnderlyingType(from) ||
               to.IsEnum && from == Enum.GetUnderlyingType(to))
            {
                return;
            }
            if (TryEmitCast(from, to) || TryEmitCast(to, from))
            {
                il.Emit(OpCodes.Castclass, to);
            }
        }

        private static bool TryEmitCast(Type a, Type b)
        {
            if (a == b)
            {
                return false;
            }
            else if (a.IsAssignableFrom(b) || b.IsAssignableFrom(a))
            {
                return true;
            }
            else if (a.IsEnum && Enum.GetUnderlyingType(a) == b)
            {
                return false;
            }
            else if (a.IsEnum && b.IsEnum && Enum.GetUnderlyingType(a) == Enum.GetUnderlyingType(b))
            {
                return false;
            }
            else if (a.IsPointer && b.IsPointer)
            {
                return false;
            }
            else if (a.IsPointer && (b == typeof(IntPtr) || b == typeof(UIntPtr)))
            {
                return false;
            }
            return true;
        }
    }
}
