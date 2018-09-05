using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Helpers
{
    public static class ConvertEx
    {
        private static MethodInfo castMethod
                = typeof(ConvertEx).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(x => x.Name == nameof(Cast) && x.IsGenericMethod);
        private static bool IsConvertSupported(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    return true;
                default:
                    return false;
            }
        }

        public static MethodInfo GetImplicitCovertionMethod(Type typeFrom, Type typeTo)
        {
            return typeTo.GetMethod(
                "op_Implicit",
                (BindingFlags.Public | BindingFlags.Static),
                null,
                new Type[] { typeFrom },
                new ParameterModifier[0]
            );
        }

        public static object ConvertTo(object value, Type targetType)
        {
            if (value == null)
                return null;

            if (value.GetType().IsAssignableTo(targetType))
                return CastTo(value, targetType);

            //var implicitCoverter = GetImplicitCovertionMethod(value.GetType(), targetType);
            //if (implicitCoverter != null)
            //    return implicitCoverter.Invoke(value, new[] { targetType });

            if (value is IConvertible && IsConvertSupported(targetType))
                return System.Convert.ChangeType(value, targetType);

            var converter = TypeDescriptor.GetConverter(value);
            if (converter != null && converter.CanConvertTo(targetType))
                return converter.ConvertTo(value, targetType);

            return CastTo(value, targetType);
        }

        public static T Convert<T>(object value)
        {
            var targetType = typeof(T);

            if (value == null)
            {
                if (targetType.IsInterface || targetType.IsClass || Nullable.GetUnderlyingType(targetType) != null)
                    return default;
                else
                    throw new InvalidCastException("Null cannot be casted to non nullable value type");
            }

            if (value is IConvertible && IsConvertSupported(targetType))
                return (T)System.Convert.ChangeType(value, targetType);

            var converter = TypeDescriptor.GetConverter(value);
            if (converter != null && converter.CanConvertTo(targetType))
                return (T)converter.ConvertTo(value, targetType);

            return Cast<T>(value);
        }

        public static T Cast<T>(object value)
        {
            return (T)value;
        }

        public static object CastTo(object value, Type targetType)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (value == null || value.GetType() == targetType)
                return value;

            return castMethod
                .MakeGenericMethodCached(new[] { targetType })
                .Invoke(null, new[] { value });
        }
    }
}
