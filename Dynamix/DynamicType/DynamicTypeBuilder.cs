using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dynamix.Reflection;

namespace Dynamix
{
    public class DynamicTypeBuilder
    {
        #region Const and Static

        private const string defaultInstanceAssemblyName = "DynamicTypeBuilderAssembly";
        private const string defaultInstanceModuleName = "DynamicTypeBuilderModule";

        static Lazy<DynamicTypeBuilder> instance = new Lazy<DynamicTypeBuilder>(() => new DynamicTypeBuilder(defaultInstanceAssemblyName, defaultInstanceModuleName));
        static long autoIncrement;
        public static DynamicTypeBuilder Instance => instance.Value;

        private static long NextIncrement()
        {
            return ++autoIncrement;
        }

        #endregion

        #region Fields and Properties

        readonly Lazy<ModuleBuilder> moduleBuilder;
        readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();

        readonly object olock = new object();

        #endregion

        #region Ctor

        public DynamicTypeBuilder(string assemblyName, string moduleName)
        {
            moduleBuilder = new Lazy<ModuleBuilder>(() =>
            {
                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
                
                return assemblyBuilder.DefineDynamicModule(moduleName);
            });
        }

        #endregion

        #region Type Creation and Registy Public API

        public Type CreateType(DynamicTypeDescriptorBuilder descriptorBuilder)
        {
            return CreateType(descriptorBuilder.Build());
        }

        public Type CreateType(DynamicTypeDescriptor descriptor)
        {
            lock (olock)
            {
                if (descriptor.Name.IsNullOrEmpty())
                    descriptor.Name = "DynamicType_" + NextIncrement();

                if (!typeCache.TryGetValue(descriptor.Name, out Type type))
                {
                    type = CreateTypeInternal(descriptor);
                    typeCache.Add(descriptor.Name, type);
                }

                return type;
            }
        }

        public Type GetRegisteredTypeOrNull(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            return (typeCache.TryGetValue(typeName, out Type type)) ? type : null;
        }

        public bool TryGetRegisteredType(string typeName, out Type type)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            return typeCache.TryGetValue(typeName, out type);
        }

        public Type GetRegisteredType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            return typeCache[typeName];
        }

        #endregion

        #region Builder Implementation

        private Type CreateTypeInternal(DynamicTypeDescriptor typeDescriptor)
        {
            var tb = GetTypeBuilder(typeDescriptor.Name, typeDescriptor.BaseType);

            foreach (var iface in typeDescriptor.Interfaces)
                tb.AddInterfaceImplementation(iface);

            var propertyBuilders = typeDescriptor.Properties.Select(x => (x, CreateProperty(tb, x, typeDescriptor.Interfaces, out var fieldBuilder), fieldBuilder)).ToList();
            var fieldBuilders = typeDescriptor.Fields.Select(x => (x, CreateField(tb, x))).ToList();

            var defaultConstructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            CreateConstructor(tb,
                propertyBuilders,
                fieldBuilders,
                defaultConstructor);

            if (typeDescriptor.AttributeBuilders != null)
                foreach (var a in typeDescriptor.AttributeBuilders)
                    tb.SetCustomAttribute(a);

            return tb.CreateType();
        }

        private void CreateConstructor(TypeBuilder tb,
            IEnumerable<(DynamicTypeProperty Property, PropertyBuilder PropertyBuilder, FieldBuilder FieldBuilder)> propertyBuilders,
            IEnumerable<(DynamicTypeField Field, FieldBuilder FieldBuilder)> fieldBuilders,
            ConstructorBuilder defaultConstructor)
        {
            var fields = fieldBuilders.Where(x => x.Field.InitializeInConstructor).ToList();
            var properties = propertyBuilders.Where(x => x.Property.InitializeInConstructor).ToList();

            if (!fields.Any() && !properties.Any())
                return;

            var constructor =
                tb.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                    CallingConventions.Standard,
                        fields.Select(x => x.FieldBuilder.FieldType).Concat(
                        properties.Select(x => x.PropertyBuilder.PropertyType))
                        .ToArray());

            var generator = constructor.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, defaultConstructor);

            var i = 1;
            foreach (var (field,fieldBuilder) in fields)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg, i);
                generator.Emit(OpCodes.Stfld, fieldBuilder);

                if (field.HasConstructorDefaultValue)
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional | ParameterAttributes.HasDefault, field.CtorParameterName)
                        .SetConstant(field.ConstructorDefaultValue);
                else
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional, field.CtorParameterName);
                i++;
            }

            foreach (var (property, propertyBuilder, fieldBuilder) in properties)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg, i);
                //generator.Emit(OpCodes.Call, p.PropertyBuilder.SetMethod);
                generator.Emit(OpCodes.Stfld, fieldBuilder);

                if (property.HasConstructorDefaultValue)
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional | ParameterAttributes.HasDefault, property.CtorParameterName)
                        .SetConstant(property.ConstructorDefaultValue);
                else
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional, property.CtorParameterName);

                i++;
            }

            generator.Emit(OpCodes.Ret);
        }

        private TypeBuilder GetTypeBuilder(string typeName, Type baseType = null)
        {
            var tb = moduleBuilder.Value.DefineType(typeName
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , baseType);

            return tb;
        }

        private static PropertyBuilder CreateProperty(TypeBuilder tb, DynamicTypeProperty property, IEnumerable<Type> interfaces, out FieldBuilder fieldBuilder)
        {
            var fieldAttributes = FieldAttributes.Private;
            if (property.SetAccessModifier == GetSetAccessModifier.None)
                fieldAttributes = fieldAttributes | FieldAttributes.InitOnly;

            fieldBuilder = tb.DefineField("_" + property.Name, property.Type, fieldAttributes);

            var propertyBuilder = tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.Type, null);

            if (property.GetAccessModifier != GetSetAccessModifier.None)
            {
                var getPropMthdBldr =
                    tb.DefineMethod("get_" + property.Name,
                    (property.GetAccessModifier == GetSetAccessModifier.Private ?
                        MethodAttributes.Private :
                    property.GetAccessModifier == GetSetAccessModifier.Protected ?
                        MethodAttributes.Family :
                        MethodAttributes.Public) |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig |
                    MethodAttributes.Virtual,
                    property.Type, Type.EmptyTypes);

                var getterIlGenerator = getPropMthdBldr.GetILGenerator();

                getterIlGenerator.Emit(OpCodes.Ldarg_0);
                getterIlGenerator.Emit(OpCodes.Ldfld, fieldBuilder);
                getterIlGenerator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getPropMthdBldr);

                var ifaceMethods = interfaces
                    .SelectMany(x => x.GetMethods())
                    .Where(x => x.Name == getPropMthdBldr.Name
                        && x.ReturnType == getPropMthdBldr.ReturnType)
                    .ToList();

                foreach (var ifaceMethod in ifaceMethods)
                {
                    tb.DefineMethodOverride(getPropMthdBldr, ifaceMethod);
                }
            }


            if (property.SetAccessModifier != GetSetAccessModifier.None)
            {
                var setPropMthdBldr =
                    tb.DefineMethod("set_" + property.Name,
                      (property.SetAccessModifier == GetSetAccessModifier.Private ?
                        MethodAttributes.Private :
                      property.SetAccessModifier == GetSetAccessModifier.Protected ?
                        MethodAttributes.Family :
                        MethodAttributes.Public) |
                      MethodAttributes.SpecialName |
                      MethodAttributes.HideBySig,
                      null, new[] { property.Type });

                var setterIlGenerator = setPropMthdBldr.GetILGenerator();

                setterIlGenerator.Emit(OpCodes.Ldarg_0);
                setterIlGenerator.Emit(OpCodes.Ldarg_1);
                setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);
                setterIlGenerator.Emit(OpCodes.Ret);

                propertyBuilder.SetSetMethod(setPropMthdBldr);
            }

            if (property.AttributeBuilders != null)
                foreach (var attributeBuilder in property.AttributeBuilders)
                    propertyBuilder.SetCustomAttribute(attributeBuilder);

            return propertyBuilder;
        }

        private static FieldBuilder CreateField(TypeBuilder tb, DynamicTypeField field)
        {
            var fieldBuilder = tb.DefineField(field.Name, field.Type,
                (field.AccessModifier == MemberAccessModifier.Private ?
                        FieldAttributes.Private :
                    field.AccessModifier == MemberAccessModifier.Protected ?
                        FieldAttributes.Family :
                        FieldAttributes.Public));

            if (field.AttributeBuilders != null)
                foreach (var attributeBuilder in field.AttributeBuilders)
                    fieldBuilder.SetCustomAttribute(attributeBuilder);

            return fieldBuilder;
        }

        #endregion
    }
}
