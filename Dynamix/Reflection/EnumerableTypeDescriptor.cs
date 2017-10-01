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
    public enum EnumerableTypeEnum
    {
        Enumerable,
        Collection,
        Array,
        List,
        Dictionary
    }

    public class EnumerableTypeDescriptor
    {
        private MethodInfo enumeratorMethod;
        private Type enumerableOfElementType;
        //private EnumerableTypeEnum EnumerableType;

        private class SupportedTypeDescription
        {
            public SupportedTypeDescription(Type Type, bool IsReadOnly, bool IsIndexed, EnumerableTypeEnum EnumerableType)
            {
                this.Type = Type;
                this.IsReadOnly = IsReadOnly;
                this.EnumerableType = EnumerableType;
                this.IsIndexed = IsIndexed;
            }
            public Type Type { get; private set; }
            public bool IsReadOnly { get; private set; }
            public bool IsIndexed { get; private set; }
            public EnumerableTypeEnum EnumerableType { get; private set; }
        }

        private static readonly Dictionary<Type, SupportedTypeDescription> Types = new Dictionary<Type, SupportedTypeDescription>();
        public bool IsGeneric { get; private set; }
        public Type Type { get; private set; }
        public Type GenericType { get; private set; }
        public bool IsIndexed { get; private set; }
        public bool IsDictionary { get; private set; }
        public bool IsArray { get; private set; }
        public bool IsCollection { get; private set; }
        public bool IsList { get; private set; }
        public Type DictionaryKeyType { get; private set; }
        public Type DictionaryElementType { get; private set; }
        public Type ElementType { get; private set; }
        public bool IsReadOnly { get; private set; }
        public bool IsInterFace { get; private set; }

        static void AddType(Type Type, bool IsReadOnly = false, bool IsIndexed = false, EnumerableTypeEnum EnumerableType = EnumerableTypeEnum.Enumerable)
        {
            Types.Add(Type, new SupportedTypeDescription(Type, IsReadOnly, IsIndexed, EnumerableType));
        }

        static EnumerableTypeDescriptor()
        {
            AddType(typeof(IEnumerable), IsReadOnly: true, IsIndexed: false, EnumerableType: EnumerableTypeEnum.Enumerable);
            AddType(typeof(IEnumerable<>), IsReadOnly: true, IsIndexed: false, EnumerableType: EnumerableTypeEnum.Enumerable);
            AddType(typeof(IList), IsReadOnly: false, IsIndexed: true, EnumerableType: EnumerableTypeEnum.List);
            AddType(typeof(IList<>), IsReadOnly: false, IsIndexed: true, EnumerableType: EnumerableTypeEnum.List);
            AddType(typeof(IReadOnlyList<>), IsReadOnly: true, IsIndexed: true, EnumerableType: EnumerableTypeEnum.List);
            AddType(typeof(ICollection), IsReadOnly: true, IsIndexed: false, EnumerableType: EnumerableTypeEnum.Collection);
            AddType(typeof(ICollection<>), IsReadOnly: true, IsIndexed: false, EnumerableType: EnumerableTypeEnum.Collection);
            AddType(typeof(IReadOnlyCollection<>), IsReadOnly: true, IsIndexed: false, EnumerableType: EnumerableTypeEnum.Collection);
            AddType(typeof(IDictionary), IsReadOnly: false, IsIndexed: true, EnumerableType: EnumerableTypeEnum.Dictionary);
            AddType(typeof(IDictionary<,>), IsReadOnly: false, IsIndexed: true,  EnumerableType: EnumerableTypeEnum.Dictionary);
        }

        private void ConstructArrayType()
        {
            this.IsArray = true;
            this.IsGeneric = false;
            this.IsIndexed = true;
            this.ElementType = this.Type.GetElementType();
            return;
        }

        private void ConstructInterfaceType()
        {
            IsInterFace = true;
            IsGeneric = Type.IsGenericType;
            if (Type.IsGenericType)
                GenericType = Type.GetGenericTypeDefinition();

            if (!Types.TryGetValue(IsGeneric ? GenericType : Type, out var typeDescriptor))
                throw new Exception("Type is not an enumerable type");
            
            IsReadOnly = typeDescriptor.IsReadOnly;
            IsDictionary = typeDescriptor.EnumerableType == EnumerableTypeEnum.Dictionary;
            IsCollection = typeDescriptor.EnumerableType == EnumerableTypeEnum.Collection;
            IsArray = typeDescriptor.EnumerableType == EnumerableTypeEnum.Array;
            IsList = typeDescriptor.EnumerableType == EnumerableTypeEnum.List;
            
            IsIndexed = typeDescriptor.IsIndexed;
            
            if (!IsGeneric)
                enumeratorMethod = typeof(IEnumerable).GetMethod("GetEnumerator");
            else
            {
                var elementtypes = Type.GetGenericArguments();
                if (IsDictionary)
                {
                    ElementType = typeof(KeyValuePair<,>).MakeGenericType(elementtypes);
                    DictionaryKeyType = elementtypes[0];
                    DictionaryElementType = elementtypes[1];
                }
                else
                    ElementType = elementtypes[0];

                enumerableOfElementType = typeof(IEnumerable<>).MakeGenericType(new[] { ElementType });
                enumeratorMethod = enumerableOfElementType.GetMethod("GetEnumerator");

            }
        }

        

        private void ConstructClassType()
        {
            var interfaces = Type.GetInterfaces()
                .Where(x => x.IsGenericType ? Types.ContainsKey(x.GetGenericTypeDefinition()) : Types.ContainsKey(x))
                .Select(x => new { Type = x, TypeDescriptor = x.IsGenericType ? Types[x.GetGenericTypeDefinition()] : Types[x] })
                .ToList();

            var enumerable = interfaces.Any(x => x.Type == typeof(IEnumerable));
            if (!enumerable)
                throw new Exception("Type is not an enumerable type");
            
            var enumeratorMethods = Type.GetMethods().Where(x => x.Name == "GetEnumerator" && x.GetParameters().Count() == 0).ToList();
            //TODO: Check if only explicit implementation!!
            enumeratorMethod = enumeratorMethods.FirstOrDefault();

            var enumeratorType = enumeratorMethod.ReturnType;
            var genericEnumerator = enumeratorType.GetInterfaces().Where(x => x.HasGenericDefinition(typeof(IEnumerator<>))).FirstOrDefault();

            if (genericEnumerator != null)
            {
                this.IsGeneric = true;
                this.ElementType = genericEnumerator.GetGenericArguments()[0];
                this.GenericType = Type.GetGenericTypeDefinition();
                interfaces = interfaces.Where(x => x.Type.HasGenericArguments(ElementType)).ToList();
            }
            else
                interfaces = interfaces.Where(x => !x.Type.IsGenericType).ToList();

            var actualInterfaces = interfaces
                .GroupBy(x => x.TypeDescriptor.EnumerableType)
                .OrderByDescending(x => x.Key)
                .FirstOrDefault();

            IsReadOnly = !actualInterfaces.Any(x => !x.TypeDescriptor.IsReadOnly);
            IsIndexed = actualInterfaces.Any(x => x.TypeDescriptor.IsIndexed);
            
            if (actualInterfaces.Any(x => x.TypeDescriptor.EnumerableType == EnumerableTypeEnum.Dictionary))
            {
                IsDictionary = true;
            }
            if (actualInterfaces.Any(x => x.TypeDescriptor.EnumerableType == EnumerableTypeEnum.List))
            {
                IsList = true;
            }
            if (actualInterfaces.Any(x => x.TypeDescriptor.EnumerableType == EnumerableTypeEnum.Array))
            {
                IsCollection = true;
            }
            if (actualInterfaces.Any(x => x.TypeDescriptor.EnumerableType == EnumerableTypeEnum.Collection))
            {
                IsArray = true;
            }
            if (actualInterfaces.Any(x => x.TypeDescriptor.EnumerableType == EnumerableTypeEnum.Enumerable))
            {

            }
        }

        public EnumerableTypeDescriptor(Type t)
        {
            this.Type = t;

            if (t.IsArray)
                ConstructArrayType();
            else if (t.IsInterface)
                ConstructInterfaceType();
            else
                ConstructClassType();
        }

        public static EnumerableTypeDescriptor Get(Type t)
        {
            if (!t.GetInterfaces().Any(x => x == typeof(IEnumerable)))
                return null;
            else
                return new EnumerableTypeDescriptor(t);
        }


        private class EnumerableMethods
        {
            public MethodInfo ToList { get; set; }
            public MethodInfo ToArray { get; set; }
            public MethodInfo AsEnumerable { get; set; }
        }

        private static ConcurrentDictionary<Type, EnumerableMethods> toListMethods = new ConcurrentDictionary<Type, EnumerableMethods>();

        private static EnumerableMethods GetEnumerableMethods(Type elementType)
        {
            if (!toListMethods.TryGetValue(elementType, out var methods))
            {
                methods = new EnumerableMethods
                {
                    ToList = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(new[] { elementType }),
                    ToArray = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(new[] { elementType }),
                    AsEnumerable = typeof(Enumerable).GetMethod("AsEnumerable").MakeGenericMethod(new[] { elementType }),
                };
                toListMethods.TryAdd(elementType, methods);
            }
            return methods;
        }

        private EnumerableMethods enumerableMethods;

        private EnumerableMethods GetEnumerableMethods()
        {
            if (enumerableMethods == null)
                enumerableMethods = GetEnumerableMethods(ElementType);
            return enumerableMethods;
        }

        public IList ToList(object enumerable)
        {
            return (IList)GetEnumerableMethods(ElementType).ToList.Invoke(null, new[] { enumerable });
        }

        public List<T> ToList<T>(object enumerable)
        {
            var list = ToList(enumerable);
            if (typeof(T) == ElementType)
                return (List<T>)list;
            else
                return list.Cast<T>().ToList();
        }

        public IReadOnlyCollection<T> ToReadOnlyCollection<T>(object enumerable)
        {
            var list = ToList<T>(enumerable);
            return (IReadOnlyCollection<T>)Activator.CreateInstance(typeof(ReadOnlyCollection<T>), new object[] { list });
        }

        public Array ToArray(object enumerable)
        {
            return (Array)GetEnumerableMethods().ToArray.Invoke(null, new[] { enumerable });
        }

        public T[] ToArray<T>(object enumerable)
        {
            var array = ToArray(enumerable);
            if (typeof(T) == ElementType)
                return (T[])array;
            else
                return array.Cast<T>().ToArray();
        }

        public IEnumerator GetEnumerator(object enumerable)
        {
            return (IEnumerator)enumeratorMethod.Invoke(enumerable, new object[] { });
        }
        public IEnumerator<T> GetEnumerator<T>(object enumerable)
        {
            var enumerator = (IEnumerator)enumeratorMethod.Invoke(enumerable, new object[] { });
            if (typeof(T) == ElementType)
                return (IEnumerator<T>)enumerator;
            else
                throw new Exception("ElementType does not match requested enumerator type");
        }

        public IEnumerable AsEnumerable(object enumerable)
        {
            return (IEnumerable)enumerable;
        }

        public IEnumerable AsEnumerableOfElementType(object enumerable)
        {
            return (IEnumerable)GetEnumerableMethods().AsEnumerable.Invoke(null, new[] { enumerable });
        }

        public IEnumerable<T> AsEnumerable<T>(object enumerable)
        {
            return (IEnumerable<T>)GetEnumerableMethods().AsEnumerable.Invoke(null, new[] { enumerable });
        }


        public enum MatchEnumerableTypeEnum
        {
            None,
            ElementType,
            InputType,
        }

        public static Array BuildArrayFromEnumerable(IEnumerable enumerable, bool readOnly, MatchEnumerableTypeEnum matchType, Type specifiedType = null)
        {
            throw new NotImplementedException();
        }

        
        public static IList BuildListFromEnumerable(IEnumerable enumerable, Type elementType = null, bool readOnly = false)
        {
            if (elementType != null)
                return (IList)typeof(EnumerableTypeDescriptor).GetMethods()
                    .Where(x => x.Name == nameof(BuildListFromEnumerable) && x.IsGenericMethod).FirstOrDefault()
                    .MakeGenericMethodCached(elementType).Invoke(null, new object[] { enumerable, readOnly });
            
            if (readOnly)
                return new ReadOnlyCollection<object>(enumerable.Cast<object>().ToList());
            else
            {
                return enumerable.Cast<object>().ToArray();
            }
        }

        public static IList<T> BuildListFromEnumerable<T>(IEnumerable enumerable, bool readOnly = false)
        {
            if (readOnly)
                return new ReadOnlyCollection<T>(enumerable.Cast<T>().ToList());
            else
                return new List<T>(enumerable.Cast<T>());
        }

        public static ICollection BuildCollectionFromEnumerable(IEnumerable enumerable, bool readOnly)
        {
            throw new NotImplementedException();
        }

        public static IDictionary BuildDicrionaryFromEnumerable(IEnumerable enumerable, bool readOnly)
        {
            throw new NotImplementedException();
        }




        public IEnumerable CreateFromEnumerable(IEnumerable enumerable)
        {
            if (enumerable == null)
                return null;

            if (IsInterFace)
            {
                if (IsReadOnly)
                {
                    if (IsList || IsCollection)
                    {
                        if (IsGeneric)
                        {
                            if (!typeof(IList<>).MakeGenericType(ElementType).IsAssignableFrom(enumerable.GetType()))
                                throw new Exception("Input enumerable is a not a proper IList<>");

                            var returnType = typeof(ReadOnlyCollection<>).MakeGenericType(ElementType);
                            return (IEnumerable)Activator.CreateInstance(returnType, enumerable);
                        }
                        else
                        {
                            return new ReadOnlyCollection<object>(enumerable.Cast<object>().ToList());
                        }
                    }
                    else if (IsDictionary)
                    {
                        if (IsGeneric)
                        {
                            //if (!enumerable.GetType().GetInterfaces().Any(x => x.HasGenericSignature(typeof(IDictionary<,>), DictionaryKeyType,DictionaryElementType)))
                            var dictionaryInterface = typeof(IDictionary<,>).MakeGenericType(DictionaryKeyType, DictionaryElementType);

                            if (!dictionaryInterface.IsAssignableFrom(enumerable.GetType()))
                                throw new Exception("Input enumerable is a not a proper dictionary");

                            return (IEnumerable)Activator.CreateInstance(typeof(ReadOnlyDictionary<,>).MakeGenericType(DictionaryKeyType, DictionaryElementType), enumerable);
                        }
                        else
                        {
                            if (!typeof(IDictionary).IsAssignableFrom(enumerable.GetType()))
                                throw new Exception("Input enumerable is a not a proper dictionary");

                            return (IEnumerable)Activator.CreateInstance(typeof(ReadOnlyDictionary<object, object>), enumerable);
                        }
                    }
                }
                else
                {
                    if (IsList)
                        return new List<object>(enumerable.Cast<object>());
                    else if (IsCollection)
                    {
                        var r = new Collection<object>();
                        foreach (var o in enumerable)
                            r.Add(o);
                        return r;
                    }
                    else if (IsDictionary)
                    {
                        return null;
                    }
                }
            }
            else
            {
                return CreateConcreteEnumerable(enumerable);
            }

            throw new Exception("Unexpected case");
        }

        private IEnumerable CreateConcreteEnumerable(IEnumerable enumerable)
        {
            //if (enumerable == null)
            //    return null;

            if (this.IsGeneric)
            {
                var genericEnumerableType = typeof(IEnumerable<>).MakeGenericType(new[] { ElementType });

                if (!genericEnumerableType.IsAssignableFrom(enumerable.GetType()))
                    throw new Exception("Source enumerable is not of same element type.");

                enumerable = AsEnumerableOfElementType(enumerable);
            }

            var ctor = this.Type.GetConstructors().Select(x => new { Ctor = x, Parameters = x.GetParameters() })
                    .Where(x => x.Parameters.Length == 1 && x.Parameters[0].ParameterType.IsAssignableFrom(enumerable.GetType()))
                    .Select(x => x.Ctor)
                    .FirstOrDefault();

            if (ctor != null)
                return (IEnumerable)ctor.Invoke(new object[] { enumerable });
            else
            {
                ctor = this.Type.GetConstructors().Select(x => new { Ctor = x, Parameters = x.GetParameters() })
                    .Where(x => x.Parameters.Length == 1
                        && (x.Parameters[0].ParameterType == typeof(IList) || x.Parameters[0].ParameterType == typeof(IList<>).MakeGenericType(new Type[] { ElementType })))
                    .Select(x => x.Ctor)
                    .FirstOrDefault();

                if (ctor != null)
                    return (IEnumerable)ctor.Invoke(new object[] { ToList(enumerable) });
                else
                {
                    var addMethod = this.Type.GetMethods().Where(x => x.Name == "Add" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.IsAssignableFrom(ElementType)).FirstOrDefault();
                    ctor = this.Type.GetConstructor(new Type[] { });
                    if (ctor != null && addMethod != null)
                    {
                        var r = ctor.Invoke(new object[] { });
                        foreach (var item in enumerable)
                            addMethod.Invoke(r, new object[] { item });
                        return (IEnumerable)r;
                    }
                    else
                        throw new Exception("Cannot construct Enumerable Type. No valid constructor or no Add method found.");
                }
            }
        }
    }
}
