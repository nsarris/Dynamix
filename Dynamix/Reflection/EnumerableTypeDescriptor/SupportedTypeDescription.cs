using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dynamix.Reflection
{
    internal class SupportedEnumerableTypes
    {
        public static readonly Dictionary<Type, SupportedEnumerableTypes> Types = new Dictionary<Type, SupportedEnumerableTypes>();

        static SupportedEnumerableTypes()
        {
            AddType(typeof(IEnumerable), EnumerableInterface.IEnumerable, EnumerableInterfaceKind.Enumerable, true, typeof(List<object>));
            AddType(typeof(IEnumerable<>), EnumerableInterface.IEnumerableT, EnumerableInterfaceKind.Enumerable, true, typeof(List<>));
            AddType(typeof(ICollection), EnumerableInterface.ICollection, EnumerableInterfaceKind.Collection, true, typeof(List<object>));
            AddType(typeof(ICollection<>), EnumerableInterface.ICollectionT, EnumerableInterfaceKind.Collection, false, typeof(List<>));
            AddType(typeof(IReadOnlyCollection<>), EnumerableInterface.IReadOnlyCollectionT, EnumerableInterfaceKind.Collection, true, typeof(ReadOnlyCollection<>));
            AddType(typeof(IList), EnumerableInterface.IList, EnumerableInterfaceKind.List, false, typeof(List<object>));
            AddType(typeof(IList<>), EnumerableInterface.IListT, EnumerableInterfaceKind.List, false, typeof(List<>));
            AddType(typeof(IReadOnlyList<>), EnumerableInterface.IReadOnlyListT, EnumerableInterfaceKind.List, true, typeof(ReadOnlyCollection<>));
            AddType(typeof(IDictionary), EnumerableInterface.IDictionary, EnumerableInterfaceKind.Dictionary, false, typeof(Dictionary<object,object>));
            AddType(typeof(IDictionary<,>), EnumerableInterface.IDictionaryTKeyTValue, EnumerableInterfaceKind.Dictionary, false, typeof(Dictionary<,>));
            AddType(typeof(IReadOnlyDictionary<,>), EnumerableInterface.IReadOnlyDictionaryTKeyTValue, EnumerableInterfaceKind.Dictionary, true, typeof(ReadOnlyDictionary<,>));
        }

        static void AddType(Type type, EnumerableInterface enumerableInterface, EnumerableInterfaceKind enumerableInterfaceKind, bool isReadOnly, Type typeToBuild)
        {
            Types.Add(type, new SupportedEnumerableTypes(type, enumerableInterface, enumerableInterfaceKind, isReadOnly, typeToBuild));
        }

        public Type Type { get; }
        public bool IsReadOnly { get; }
        public EnumerableInterfaceKind EnumerableInterfaceKind { get; }
        public EnumerableInterface EnumerableInterface { get; }
        internal Type TypeToBuild { get; }

        internal SupportedEnumerableTypes(Type type, EnumerableInterface enumerableInterface, EnumerableInterfaceKind enumerableInterfaceKind, bool isReadOnly, Type typeToBuild)
        {
            this.Type = type;
            this.IsReadOnly = isReadOnly;
            this.EnumerableInterfaceKind = enumerableInterfaceKind;
            this.EnumerableInterface = enumerableInterface;
            this.TypeToBuild = typeToBuild;
        }
    }
}
