﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Reflection;

namespace Dynamix
{
    public class DynamicTypeBuilder
    {
        ModuleBuilder moduleBuilder;
        readonly Dictionary<string, DynamicTypeCachedDescriptor> cache = new Dictionary<string, DynamicTypeCachedDescriptor>(StringComparer.OrdinalIgnoreCase);

        int id;
        readonly object o = new object();
        readonly object or = new object();
        readonly string assembly;
        readonly string module;

        static DynamicTypeBuilder instance;
        public static DynamicTypeBuilder Instance { get { if (instance == null) instance = new DynamicTypeBuilder("DynamicTypeBuilderStaticInsance", "DynamicTypeBuilderStaticInsance"); return instance; } }

        public DynamicTypeBuilder()
        {
            assembly = Assembly.GetExecutingAssembly().FullName;
            module = Assembly.GetExecutingAssembly().GetLoadedModules()[0].Name;
        }

        public DynamicTypeBuilder(string assemblyName, string moduleName)
        {
            assembly = assemblyName;
            module = moduleName;
        }

        public Type CreateAndRegisterType(DynamicTypeDescriptor descr, bool overwrite = true)
        {
            return CreateAndRegisterType(descr.Name, descr.Properties, descr.Fields, descr.Interfaces, overwrite, descr.BaseType, descr.AttributeBuilders);
        }

        public Type CreateAndRegisterType(DynamicTypeDescriptorBuilder descriptorBuilder, bool overwrite = true)
        {
            return CreateAndRegisterType(descriptorBuilder.Build(), overwrite);
        }

        public Type CreateAndRegisterType(string typeName, IEnumerable<DynamicTypeProperty> properties, IEnumerable<DynamicTypeField> fields, IEnumerable<Type> interfaces = null, bool overwrite = true, Type baseType = null, IEnumerable<CustomAttributeBuilder> customAttributeBuilders = null)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            Type t = null;
            lock (or)
            {
                if (overwrite)
                {
                    if (cache.TryGetValue(typeName, out DynamicTypeCachedDescriptor td))
                    {
                        if (td.Fields.Count == properties.Count()
                        && td.Fields.Count(x => properties.Any(
                            y => y.Name == x.Name && y.Type == x.Type)) == properties.Count())
                            return td.Type;
                        else
                        {
                            t = CreateType(properties, fields, interfaces, typeName, baseType, customAttributeBuilders);
                            cache[typeName] = new DynamicTypeCachedDescriptor(t, properties);
                        }
                    }
                    else
                    {
                        t = CreateType(properties, fields, interfaces, typeName, baseType, customAttributeBuilders);
                        cache[typeName] = new DynamicTypeCachedDescriptor(t, properties);
                    }
                }
                else
                {
                    t = CreateType(properties, fields, interfaces, typeName, baseType, customAttributeBuilders);
                    cache.Add(typeName, new DynamicTypeCachedDescriptor(t, properties));
                }
            }
            return t;
        }

        public Type TryGetRegisteredTypeOrNull(string TypeName)
        {
            if (string.IsNullOrWhiteSpace(TypeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            if (cache.TryGetValue(TypeName, out DynamicTypeCachedDescriptor t))
                return t.Type;
            else
                return null;
        }

        public Type GetRegisteredType(string TypeName)
        {
            if (string.IsNullOrWhiteSpace(TypeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            return cache[TypeName].Type;
        }



        public object CreateNewObject(IEnumerable<DynamicTypeProperty> properties, IEnumerable<DynamicTypeField> fields)
        {
            var myType = CreateType(properties, fields);
            var myObject = Activator.CreateInstance(myType);
            return myObject;
        }

        public Type CreateType(DynamicTypeDescriptor descriptor, string typeName = null)
        {
            return CreateType(descriptor.Properties, descriptor.Fields, descriptor.Interfaces, typeName ?? descriptor.Name, descriptor.BaseType, descriptor.AttributeBuilders);
        }

        public Type CreateType(DynamicTypeDescriptorBuilder descriptorBuilder, string typeName = null)
        {
            return CreateType(descriptorBuilder.Build(), typeName);
        }

        public Type CreateType(IEnumerable<DynamicTypeProperty> properties, IEnumerable<DynamicTypeField> fields, IEnumerable<Type> interfaces = null, string TypeName = null, Type BaseType = null, IEnumerable<CustomAttributeBuilder> customAttributeBuilders = null)
        {
            lock (o)
            {
                var tb =
                 !TypeName.IsNullOrWhiteSpace() ?
                    GetTypeBuilder(assembly, module, TypeName, BaseType) :
                    GetTypeBuilder(assembly, module, "DynamicType_" + (++id).ToString(), BaseType);

                foreach (var iface in interfaces)
                    tb.AddInterfaceImplementation(iface);

                var propertyBuilders = properties.Select(x => (x , CreateProperty(tb, x, interfaces))).ToList();
                var fieldBuilders = fields.Select(x => (x, CreateField(tb, x))).ToList();

                var defaultConstructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

                CreateConstructor(tb, 
                    propertyBuilders,
                    fieldBuilders,
                    defaultConstructor);

                if (customAttributeBuilders != null)
                    foreach (var a in customAttributeBuilders)
                        tb.SetCustomAttribute(a);

                return tb.CreateType();
            }

        }

        private void CreateConstructor(TypeBuilder tb, 
            IEnumerable<(DynamicTypeProperty Property , PropertyBuilder PropertyBuilder)> propertyBuilders, 
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
            foreach (var f in fields)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg, i);
                generator.Emit(OpCodes.Stfld, f.FieldBuilder);

                if (f.Field.HasConstructorDefaultValue)
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional | ParameterAttributes.HasDefault, f.Field.CtorParameterName)
                        .SetConstant(f.Field.ConstructorDefaultValue);
                else
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional, f.Field.CtorParameterName);
                i++;
            }

            foreach (var p in properties)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg, i);
                generator.Emit(OpCodes.Call, p.PropertyBuilder.SetMethod);
                
                if (p.Property.HasConstructorDefaultValue)
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional | ParameterAttributes.HasDefault, p.Property.CtorParameterName)
                        .SetConstant(p.Property.ConstructorDefaultValue);
                else
                    constructor
                        .DefineParameter(i, ParameterAttributes.Optional, p.Property.CtorParameterName);

                i++;
            }

            generator.Emit(OpCodes.Ret);
        }

        private TypeBuilder GetTypeBuilder(string AssemblyName, string ModuleName, string TypeName, Type BaseType = null)
        {
            if (moduleBuilder == null)
            {
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName);
            }

            var tb = moduleBuilder.DefineType(TypeName
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , BaseType);

            return tb;
        }

        private static PropertyBuilder CreateProperty(TypeBuilder tb, DynamicTypeProperty property, IEnumerable<Type> interfaces)
        {
            var fieldBuilder = tb.DefineField("_" + property.Name, property.Type, FieldAttributes.Private);

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
                var modifyProperty = setterIlGenerator.DefineLabel();
                var exitSet = setterIlGenerator.DefineLabel();

                setterIlGenerator.MarkLabel(modifyProperty);
                setterIlGenerator.Emit(OpCodes.Ldarg_0);
                setterIlGenerator.Emit(OpCodes.Ldarg_1);
                setterIlGenerator.Emit(OpCodes.Stfld, fieldBuilder);

                setterIlGenerator.Emit(OpCodes.Nop);
                setterIlGenerator.MarkLabel(exitSet);
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
    }
}
