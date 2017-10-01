using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public static class PropertyInfoExtensions
    {
        public static T GetAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            if (Attribute.IsDefined(property, typeof(T)))
                return ((T)(Attribute.GetCustomAttribute(property, typeof(T))));
            else
                return null;
        }

        public static bool HasAttribute<T>(this PropertyInfo property) where T : Attribute
        {
            return (Attribute.IsDefined(property, typeof(T)));
        }

        public static bool HasAttribute(this PropertyInfo property, Attribute attr)
        {
            return (Attribute.IsDefined(property, attr.GetType()));
        }
    }
}
