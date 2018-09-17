using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public static class ConstructorInfoExTypeExtensions
    {
        const BindingFlags PUBLIC_ISTANCE_STATIC = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public static ConstructorInfoEx GetConstructorEx(this ConstructorInfo constructorInfo, bool enableCaching = true)
        {
            if (enableCaching)
                return ConstructorInfoExCache.GetConstructorEx(constructorInfo);
            else
                return new ConstructorInfoEx(constructorInfo, false);
        }


        public static ConstructorInfoEx GetConstructorEx(this Type type, IEnumerable<Type> signature, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return ConstructorInfoExCache.GetConstructorEx(type, signature, bindingFlags);
            else
            {
                var f = type.GetConstructor(bindingFlags, null, signature?.ToArray() ?? Type.EmptyTypes, null);
                if (f == null) return null;
                return new ConstructorInfoEx(f, false);
            }
        }

        public static ConstructorInfoEx GetConstructorEx(this Type type, IEnumerable<Type> signature, BindingFlagsEx bindingFlags, bool enableCaching = true)
        {
            return GetConstructorEx(type, signature, (BindingFlags)bindingFlags, enableCaching);
        }

        public static IReadOnlyDictionary<string, ConstructorInfoEx> GetConstructorsExDic(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {

            if (enableCaching)
                return ConstructorInfoExCache.GetConstructorsExDic(type, bindingFlags);
            else
                return new ReadOnlyDictionary<string, ConstructorInfoEx>(
                    type.GetConstructors(bindingFlags).ToDictionary(x => x.Name, x => new ConstructorInfoEx(x, false)));
        }

        public static IEnumerable<ConstructorInfoEx> GetConstructorsEx(this Type type, BindingFlags bindingFlags = PUBLIC_ISTANCE_STATIC, bool enableCaching = true)
        {
            if (enableCaching)
                return ConstructorInfoExCache.GetConstructorsEx(type, bindingFlags);
            else
                return type.GetConstructors(bindingFlags).Select(x => new ConstructorInfoEx(x, false));
        }
    }
}
