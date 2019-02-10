using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{

    public class EnumerableTypeDescriptor
    {
        #region Cache

        private static readonly ConcurrentDictionary<Type, EnumerableTypeDescriptor> descriptorCache = new ConcurrentDictionary<Type, EnumerableTypeDescriptor>();

        public static EnumerableTypeDescriptor Get(Type type)
        {
            if (!descriptorCache.TryGetValue(type, out var enumerableTypeDescriptor))
            {
                enumerableTypeDescriptor = new EnumerableTypeDescriptor(type);
                descriptorCache.TryAdd(type, enumerableTypeDescriptor);
            }

            return enumerableTypeDescriptor;
        }

        #endregion

        private readonly ConstructorInfoEx parameterlessConstructor;
        private readonly IReadOnlyDictionary<Type, ConstructorInfoEx> constructorsWithEnumerableParameter;
        private readonly IEnumerable<ListMethodDescriptor> listMethods;
        private readonly IEnumerable<DictionaryMethodDescriptor> dictionaryMethods;

        public Type Type { get; private set; }
        public IReadOnlyList<EnumerableInterfaceDescriptor> EnumerableInterfaces { get; }
        public bool IsEnumerable { get; }
        public EnumerableInterfaceDescriptor ExplicitImplementation { get; }

        internal ConstructorInfoEx ParameterlessConstructor { get { AssertOpenGenericOrAbstractType("Constructors"); return parameterlessConstructor; } }
        internal IReadOnlyDictionary<Type, ConstructorInfoEx> ConstructorsWithEnumerableParameter { get { AssertOpenGenericOrAbstractType("Constructors"); return constructorsWithEnumerableParameter; } }
        internal IEnumerable<ListMethodDescriptor> ListMethods { get { AssertOpenGenericType("Methods"); return listMethods; } }
        internal IEnumerable<DictionaryMethodDescriptor> DictionaryMethods { get { AssertOpenGenericType("Methods"); return dictionaryMethods; } }

        private EnumerableTypeDescriptor(Type type)
        {
            Type = type;

            if (type == typeof(string))
                EnumerableInterfaces = new List<EnumerableInterfaceDescriptor>();
            else if (type.IsArray)
                EnumerableInterfaces = new List<EnumerableInterfaceDescriptor>() { EnumerableInterfaceDescriptor.GetArray(type) };
            else
                EnumerableInterfaces = new ReadOnlyCollection<EnumerableInterfaceDescriptor>(
                    Type.GetInterfaces()
                    .Join(SupportedEnumerableTypes.Types, x => x.GetTypeOrGenericTypeDefinition(), y => y.Key.GetTypeOrGenericTypeDefinition(),
                        (x, y) => new EnumerableInterfaceDescriptor(x, y.Value))
                    .GroupBy(x => x.GenericArgumentHashCode)
                    .Select(x => x.Aggregate((accumulate, current)
                            => current.Type.IsSuperclassOf(accumulate.Type) ? current : accumulate))
                    .ToList());

            IsEnumerable = EnumerableInterfaces.Any();

            if (!IsEnumerable)
                return;

            if (Type.IsArray)
                ExplicitImplementation = EnumerableInterfaces.Single();
            else
            {
                var defaultEnumeratorElementType = GetExplicitEnumerableElementType(Type);

                if (defaultEnumeratorElementType != null)
                    ExplicitImplementation = EnumerableInterfaces.FirstOrDefault(x => x.ElementType == defaultEnumeratorElementType);
            }

            if (!Type.ContainsGenericParameters && !Type.IsAbstract)
            {
                parameterlessConstructor = DiscoverParameterlessConstructor();
                constructorsWithEnumerableParameter = DiscoverConstructors();
            }

            if (!Type.ContainsGenericParameters)
            {
                listMethods = DiscoverListMethods();
                dictionaryMethods = DiscoverDictionaryMethods();
            }
        }

        private void AssertOpenGenericOrAbstractType(string assertion)
        {
            AssertOpenGenericType(assertion);
            AssertAbstractType(assertion);
        }

        private void AssertOpenGenericType(string assertion)
        {
            if (Type.ContainsGenericParameters)
                throw new NotSupportedException($"{assertion} not supported on open generic types.");
        }

        private void AssertAbstractType(string assertion)
        {
            if (Type.IsAbstract)
                throw new NotSupportedException($"{assertion} not supported on abstract types.");
        }

        private IReadOnlyDictionary<Type, ConstructorInfoEx> DiscoverConstructors()
        {
            return Type.IsAbstract ? new Dictionary<Type, ConstructorInfoEx>() :
               Type.GetConstructors(new Type[] { typeof(IEnumerable) }, (BindingFlags)BindingFlagsEx.AllInstance)
               .ToDictionary(x => x.GetParameters().Single().ParameterType, x => new ConstructorInfoEx(x));
        }

        private ConstructorInfoEx DiscoverParameterlessConstructor()
        {
            return Type.IsAbstract ? null :
                Type.GetConstructorEx(Type.EmptyTypes, BindingFlagsEx.AllInstance);
        }

        private IEnumerable<ListMethodDescriptor> DiscoverListMethods()
        {
            return

            (GetMethods(nameof(ListMethod.Add), 1)
                .Select(x => new ListMethodDescriptor(ListMethod.Add, x, 0)))
            .Concat(
                GetMethods(nameof(ListMethod.Remove), 1)
                .Select(x => new ListMethodDescriptor(ListMethod.Remove, x, 0)))
            .Concat(
                GetMethods(nameof(ListMethod.Insert), 2)
                .Where(x => x.MethodInfo.GetParameters().First().ParameterType == typeof(int))
                .Select(x => new ListMethodDescriptor(ListMethod.Insert, x, 1)))
            .Concat(
                GetMethods(nameof(ListMethod.RemoveAt), 1)
                .Where(x => x.MethodInfo.GetParameters().First().ParameterType == typeof(int))
                .Select(x => new ListMethodDescriptor(ListMethod.RemoveAt, x, null)))
            .Concat(
                GetMethods(nameof(ListMethod.Contains), 1)
                .Select(x => new ListMethodDescriptor(ListMethod.Contains, x, 0)))
            .OrderBy(x => x.ElementType?.GetHierarchyDepth() ?? 0)
            .ToList();
        }

        private IEnumerable<DictionaryMethodDescriptor> DiscoverDictionaryMethods()
        {
            return

            (GetMethods(nameof(DictionaryMethod.Add), 2)
                .Select(x => new DictionaryMethodDescriptor(DictionaryMethod.Add, x, 0, 1)))
            .Concat(
                GetMethods(nameof(DictionaryMethod.ContainsKey), 1)
                .Select(x => new DictionaryMethodDescriptor(DictionaryMethod.ContainsKey, x, 0, -1)))
            .OrderBy(x => x.KeyType?.GetHierarchyDepth() ?? 0)
            .ToList();
        }

        private IEnumerable<MethodInfoEx> GetMethods(string name, int parameterCount)
        {
            return Type.GetMethodsEx(BindingFlagsEx.AllInstance).Where(x => x.Name == name && x.MethodInfo.GetParameters().Length == parameterCount);
        }

        internal ListMethodDescriptor GetApplicableListMethod(ListMethod method, object elementInstance)
        {
            return ListMethods
                .FirstOrDefault(x => x.Method == method
                && (x.ElementType is null
                || elementInstance is null
                || x.ElementType.IsInstanceOfType(elementInstance)));
        }

        internal DictionaryMethodDescriptor GetApplicableDictionaryMethod(DictionaryMethod method, (object key, object value) entry)
        {
            return DictionaryMethods
                .FirstOrDefault(x => x.Method == method
                && (x.ElementType is null
                    || entry.value is null
                    || x.ElementType.IsInstanceOfType(entry.value))
                && (x.KeyType is null
                    || entry.key is null
                    || x.KeyType.IsInstanceOfType(entry.key)
                ));
        }

        private Type GetExplicitEnumerableElementType(Type type)
        {
            var defaultEnumeratorMethod =
                type.GetMethods()
                .FirstOrDefault(x =>
                    x.Name == "GetEnumerator"
                    && !x.IsGenericMethod
                    && !x.GetParameters().Any());

            if (defaultEnumeratorMethod != null)
                return
                    defaultEnumeratorMethod.ReturnType.GetInterfaces().FirstOrDefault(x => x.HasGenericTypeDefinition(typeof(IEnumerator<>)))?.GenericTypeArguments.Single() ??
                    (defaultEnumeratorMethod.ReturnType.GetInterfaces().Contains(typeof(IDictionaryEnumerator)) ? typeof(DictionaryEntry) : typeof(object));

            return null;
        }


        #region Instance builders


        public IEnumerable CreateBestMatchingInstance(IEnumerable<Type> genericTypeArguments = null)
        {
            if (Type.IsArrayOrArrayClass())
                return Array.CreateInstance(ExplicitImplementation.ElementType, 0);
            else if (!Type.IsInterface)
            {
                var ctor =
                    GetConcreteType(Type, genericTypeArguments)
                    .GetConstructors(Type.EmptyTypes, (BindingFlags)BindingFlagsEx.AllInstance, true)
                    .FirstOrDefault();

                if (ctor is null)
                    throw new InvalidOperationException($"Could not find compatible type to construct for {Type}");

                return (IEnumerable)new ConstructorInfoEx(ctor).Invoke();
            }
            else
            {
                var buildInType = SupportedEnumerableTypes.Types
                    .Where(x => x.Key.GetTypeOrGenericTypeDefinition() == Type.GetTypeOrGenericTypeDefinition())
                    .Select(x => x.Value)
                    .FirstOrDefault();

                if (buildInType != null)
                {
                    return (IEnumerable)ActivatorEx.CreateInstance(
                         GetConcreteType(buildInType.TypeToBuild, genericTypeArguments));
                }
                else
                {
                    var ctor =
                        GetConcreteType(Type, genericTypeArguments)
                        .GetAllImplementations()
                        .SelectMany(x => x.GetConstructors(Type.EmptyTypes, (BindingFlags)BindingFlagsEx.AllInstance))
                        .FirstOrDefault();

                    if (ctor is null)
                        throw new InvalidOperationException($"Could not find compatible type to construct for {Type}");

                    return (IEnumerable)new ConstructorInfoEx(ctor).Invoke();
                }
            }
        }

        public IEnumerable<ConstructorInfo> GetBestMatchingConstructors(Type type, IEnumerable<Type> parameterTypes, BindingFlags bindingFlags, bool includeDescendants = false)
        {
            return
            (includeDescendants ? new[] { type } : type.GetSelfAndDescendants())
                    .Where(x => !x.IsAbstract)
                    .SelectMany(x =>
                        x.GetConstructors(bindingFlags)
                        .Where(c => c.GetParameters().CanBeInvokedWith(parameterTypes)));
        }

        private Type GetConcreteType(Type type, IEnumerable<Type> genericTypeArguments)
        {
            return genericTypeArguments != null && genericTypeArguments.Any() ?
                    type.MakeGenericTypeCached(genericTypeArguments) :
                    type;
        }

        #endregion
    }
}
