using System.Collections.Generic;
using System.Linq;

namespace Dynamix.DynamicProjection
{
    public enum UnmappedValueType
    {
        TypeDefault,
        Constant,
        OriginalValue
    }

    internal class ValueMap
    {
        public Dictionary<object, object> Values { get; }
        public UnmappedValueType UnmappedValueType { get; }
        public object UnmappedValue { get; }
        internal ValueMap(Dictionary<object, object> values, UnmappedValueType unmappedValueType, object unmappedValue)
        {
            Values = values;
            UnmappedValueType = unmappedValueType;
            UnmappedValue = unmappedValue;
        }
    }
}
