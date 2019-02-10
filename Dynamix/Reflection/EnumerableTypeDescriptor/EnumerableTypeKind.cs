namespace Dynamix.Reflection
{
    public enum EnumerableInterfaceKind
    {
        Enumerable,
        Collection,
        List,
        Dictionary
    }

    public enum EnumerableInterface
    {
        IEnumerable,
        IEnumerableT,
        ICollection,
        ICollectionT,
        IReadOnlyCollectionT,
        IList,
        IListT,
        IReadOnlyListT,
        IDictionary,
        IDictionaryTKeyTValue,
        IReadOnlyDictionaryTKeyTValue
    }
}
