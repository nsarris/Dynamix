using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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

        public DynamicTypeBuilder(string AssemblyName, string ModuleName)
        {
            assembly = AssemblyName;
            module = ModuleName;
        }

        public Type CreateAndRegisterType(DynamicTypeDescriptor descr, bool Overwrite = true)
        {
            return CreateAndRegisterType(descr.Name, descr.Properties, Overwrite, descr.BaseType, descr.AttributeBuilders);
        }

        public Type CreateAndRegisterType(string TypeName, IEnumerable<DynamicTypeProperty> fields, bool Overwrite = true, Type BaseType = null, IEnumerable<CustomAttributeBuilder> customAttributeBuilders = null)
        {
            if (string.IsNullOrWhiteSpace(TypeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            Type t = null;
            lock (or)
            {
                if (Overwrite)
                {
                    if (cache.TryGetValue(TypeName, out DynamicTypeCachedDescriptor td))
                    {
                        if (td.Fields.Count == fields.Count()
                        && td.Fields.Count(x => fields.Any(
                            y => y.Name == x.Name && y.Type == x.Type)) == fields.Count())
                            return td.Type;
                        else
                        {
                            t = CreateType(fields, TypeName, BaseType, customAttributeBuilders);
                            cache[TypeName] = new DynamicTypeCachedDescriptor(t, fields);
                        }
                    }
                    else
                    {
                        t = CreateType(fields, TypeName, BaseType, customAttributeBuilders);
                        cache[TypeName] = new DynamicTypeCachedDescriptor(t, fields);
                    }
                }
                else
                {
                    t = CreateType(fields, TypeName, BaseType, customAttributeBuilders);
                    cache.Add(TypeName, new DynamicTypeCachedDescriptor(t, fields));
                }
            }
            return t;
        }

        public Type TryGetRegisteredTypeOrNull(string TypeName)
        {
            if (string.IsNullOrWhiteSpace(TypeName))
                throw new ArgumentException("TypeName cannot be null or whitespace");

            DynamicTypeCachedDescriptor t = null;
            if (cache.TryGetValue(TypeName, out t))
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



        public object CreateNewObject(IEnumerable<DynamicTypeProperty> fields)
        {
            var myType = CreateType(fields);
            var myObject = Activator.CreateInstance(myType);
            return myObject;
        }

        public Type CreateType(DynamicTypeDescriptor descriptor, string TypeName = null)
        {
            return CreateType(descriptor.Properties, TypeName ?? descriptor.Name, descriptor.BaseType, descriptor.AttributeBuilders);
        }

        public Type CreateType(IEnumerable<DynamicTypeProperty> fields, string TypeName = null, Type BaseType = null, IEnumerable<CustomAttributeBuilder> customAttributeBuilders = null)
        {
            lock (o)
            {
                var tb =
                 !TypeName.IsNullOrWhiteSpace() ?
                    GetTypeBuilder(assembly, module, TypeName, BaseType) :
                    GetTypeBuilder(assembly, module, "DynamicType_" + (++id).ToString(), BaseType);

                var propertyBuilders = fields.Select(x => CreateProperty(tb, x.Name, x.Type, x.AttributeBuilders)).ToList();
                var defaultConstructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

                CreateConstructor(tb, propertyBuilders, defaultConstructor);

                if (customAttributeBuilders != null)
                    foreach (var a in customAttributeBuilders)
                        tb.SetCustomAttribute(a);

                return tb.CreateType();
            }

        }

        private ConstructorBuilder CreateConstructor(TypeBuilder tb, IEnumerable<PropertyBuilder> propertyBuilders, ConstructorBuilder defaultConstructor)
        {
            var constructor = tb.DefineConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, propertyBuilders.Select(x => x.PropertyType).ToArray());
            
            var generator = constructor.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0); //load this for base type constructor
            generator.Emit(OpCodes.Call, defaultConstructor);

            var i = 1;
            foreach (var p in propertyBuilders)
            {
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg, i);
                generator.Emit(OpCodes.Call, p.SetMethod);
                constructor.DefineParameter(i, new ParameterAttributes(), p.Name.ToCamelCase());
                i++;
            }
            generator.Emit(OpCodes.Ret);
            return constructor;
        }

        private TypeBuilder GetTypeBuilder(string AssemblyName, string ModuleName, string TypeName, Type BaseType = null)
        {
            if (moduleBuilder == null)
            {
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
                moduleBuilder = assemblyBuilder.DefineDynamicModule(ModuleName);
            }

            TypeBuilder tb = moduleBuilder.DefineType(TypeName
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , BaseType);

            return tb;
        }

        private static PropertyBuilder CreateProperty(TypeBuilder tb, string propertyName, Type propertyType, IEnumerable<CustomAttributeBuilder> attributeBuilders = null)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);

            if (attributeBuilders != null)
                foreach (var attributeBuilder in attributeBuilders)
                    propertyBuilder.SetCustomAttribute(attributeBuilder);

            return propertyBuilder;
        }
    }
}
