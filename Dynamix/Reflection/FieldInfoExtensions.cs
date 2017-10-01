using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public static class FieldInfoExtensions
    {
        public static T GetAttribute<T>(this FieldInfo field) where T : Attribute
        {
            if (Attribute.IsDefined(field, typeof(T)))
                return ((T)(Attribute.GetCustomAttribute(field, typeof(T))));
            else
                return null;
        }

        public static bool HasAttribute<T>(this FieldInfo field) where T : Attribute
        {
            return (Attribute.IsDefined(field, typeof(T)));
        }

        public static bool HasAttribute(this FieldInfo field, Attribute attr)
        {
            return (Attribute.IsDefined(field, attr.GetType()));
        }
    }
}
