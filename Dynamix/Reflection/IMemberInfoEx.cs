using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public interface IMemberInfoEx
    {
        Type Type { get; }
        MemberInfo MemberInfo { get; }
        bool IsField { get; }
        bool IsEnumerable { get;  }
        EnumerableTypeDescriptor EnumerableDescriptor { get; }
        Func<object, object> Getter { get; }
        Action<object, object> Setter { get; }
        bool PublicGet { get; }
        bool PublicSet { get; }

        object Get(object instance, bool allowPrivate = false);
        void Set(object instance, object value, bool allowPrivate = false);
    }

}
