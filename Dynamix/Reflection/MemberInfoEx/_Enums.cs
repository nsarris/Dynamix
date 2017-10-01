using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Reflection
{
    public enum BindingFlagsEx
    {
        Instance = BindingFlags.Instance,
        Static = BindingFlags.Static,
        Public = BindingFlags.Public,
        NonPublic = BindingFlags.NonPublic,
        FlattenHierarchy = BindingFlags.FlattenHierarchy,
        All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
        AllPublic = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
        AllPrivate = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
        AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
        AllStatic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
        AllNonInherited = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
        AllPublicNonInherited = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public,
        AllPrivateNonInherited = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic,
        AllInstanceNonInherit = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
        AllStaticNonInherit = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.NonPublic,
    }

    public enum MemberInfoExKind
    {
        Field,
        Property,
        Method,
        Constructor
    }
}
