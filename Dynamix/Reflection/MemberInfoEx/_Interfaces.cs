using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{

    public interface IMemberInfoEx
    {
        string Name { get; }
        MemberInfo MemberInfo { get; }
        MemberInfoExKind Kind { get; }
    }

    public interface IValueMemberInfoEx : IMemberInfoEx
    {
        Type Type { get; }
        bool IsField { get; }
        bool IsEnumerable { get; }
        EnumerableTypeDescriptor EnumerableDescriptor { get; }

        bool PublicGet { get; }
        bool PublicSet { get; }

        object Get(object instance);
        void Set(object instance, object value);
    }

}
