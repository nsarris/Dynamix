using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class InterfaceInstanceBuilder<T>
        where T : class
    {
        private readonly TypeConstructor<T> typeConstructor;

        public InterfaceInstanceBuilder()
        {
            var dynamicTypeName = typeof(T).FullName + "_DynamicImplementation";

            if (!DynamicTypeBuilder.Instance.TryGetRegisteredType(dynamicTypeName, out var typeToBuild))
            {
                typeToBuild = DynamicTypeBuilder.Instance.CreateType(builder =>
                {
                    builder
                        .HasName(dynamicTypeName)
                        .ImplementsInterface(typeof(T))
                        .AddPropertiesFromType(typeof(T), 
                            config => config
                                .AreInitializedInConstructorOptional()
                                .HaveGetter(GetSetAccessModifier.Public)
                                .HaveSetter(GetSetAccessModifier.Public));
                });
            }

            typeConstructor = new TypeConstructor<T>(typeToBuild);
        }

        public T Build(Action<MemberValueCollector<T>> valueCollectorAction)
        {
            var valueCollector = new MemberValueCollector<T>();
            valueCollectorAction(valueCollector);
            return typeConstructor.Construct(valueCollector.GetDictionary().Select(x => (x.Key.ToCamelCase(), x.Value)));
        }
    }
}
