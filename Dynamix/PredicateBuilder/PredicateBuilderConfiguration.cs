using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Dynamix.PredicateBuilder
{
    public class PredicateBuilderConfiguration
    {
        private static readonly string[] trueValueStrings = new[] { "true", "1" };
        private static readonly string[] falseValueStrings = new[] { "false", "0" };
        private static readonly string[] nullValueStrings = new[] { "null" };

        private readonly Dictionary<PredicateDataType, IEnumerable<object>> emptyValues
            = new Dictionary<PredicateDataType, IEnumerable<object>>();

        public IEnumerable<string> TrueTextValues { get; private set; } = trueValueStrings;
        public IEnumerable<string> FalseTextValues { get; private set; } = falseValueStrings;
        public IEnumerable<string> NullTextValues { get; private set; } = nullValueStrings;

        public bool TrueValuesCaseSenstive { get; private set; }
        public bool FalseValuesCaseSenstive { get; private set; }
        public bool NullValuesCaseSenstive { get; private set; }

        public NumberStyles NumberStyles { get; set; } = NumberStyles.Number;
        public IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;
        public DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.None;
        public string[] DateTimeFormats { get; private set; }
        public string[] TimeSpanFormats { get; private set; }

        public PredicateBuilderConfiguration WithEmptyValues(PredicateDataType dataType, IEnumerable<object> values)
        {
            if (dataType == PredicateDataType.Collection
                || dataType == PredicateDataType.Unsupported)
                throw new InvalidOperationException($"Cannot set empty values for {dataType} types");

            if (emptyValues.ContainsKey(dataType))
                emptyValues[dataType] = values;
            else
                emptyValues.Add(dataType, values);

            return this;
        }

        public IEnumerable<object> GetEmptyValues(PredicateDataType dataType)
        {
            return emptyValues.TryGetValue(dataType, out var values) ?
                    values : Enumerable.Empty<object>();
        }

        public PredicateBuilderConfiguration WithTrueTextValues(PredicateDataType dataType, IEnumerable<string> values, bool caseSensitive = false)
        {
            TrueValuesCaseSenstive = caseSensitive;
            TrueTextValues =
                values == null ?
                    Enumerable.Empty<string>() :
                    values.Select(x => caseSensitive ? x : x.ToLower()).ToList();

            return this;
        }

        public PredicateBuilderConfiguration WithFalseTextValues(PredicateDataType dataType, IEnumerable<string> values, bool caseSensitive = false)
        {
            FalseValuesCaseSenstive = caseSensitive;
            FalseTextValues =
                values == null ?
                    Enumerable.Empty<string>() :
                    values.Select(x => caseSensitive ? x : x.ToLower()).ToList();

            return this;
        }

        public PredicateBuilderConfiguration WithNullTextValues(PredicateDataType dataType, IEnumerable<string> values, bool caseSensitive = false)
        {
            NullValuesCaseSenstive = caseSensitive;
            NullTextValues =
                values == null ?
                    Enumerable.Empty<string>() :
                    values.Select(x => caseSensitive ? x : x.ToLower()).ToList();

            return this;
        }

        public PredicateBuilderConfiguration WithDataTimeFormats(IEnumerable<string> formats)
        {
            DateTimeFormats = formats == null ? new string[] { } : formats.ToArray();
            return this;
        }

        public PredicateBuilderConfiguration WithTimeSpanFormats(IEnumerable<string> formats)
        {
            TimeSpanFormats = formats == null ? new string[] { } : formats.ToArray();
            return this;
        }

        public PredicateBuilderConfiguration WithNumberStyles(NumberStyles numberStyles)
        {
            NumberStyles = numberStyles;
            return this;
        }

        public PredicateBuilderConfiguration WithDateTimeStyles(DateTimeStyles dateTimeStyles)
        {
            DateTimeStyles = dateTimeStyles;
            return this;
        }

        public PredicateBuilderConfiguration WithFormatProvider(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider;
            return this;
        }

        public bool IsTrueString(object value)
        {
            var s = (value ?? "").ToString();
            if (!TrueValuesCaseSenstive)
                s = s.ToLower();
            return TrueTextValues.Contains(s);
        }

        public bool IsFalseString(object value)
        {
            var s = (value ?? "").ToString();
            if (!FalseValuesCaseSenstive)
                s = s.ToLower();
            return FalseTextValues.Contains(s);
        }

        public bool IsNullString(object value)
        {
            var s = (value ?? "").ToString();
            if (!NullValuesCaseSenstive)
                s = s.ToLower();
            return NullTextValues.Contains(s);
        }

        
    }
}
