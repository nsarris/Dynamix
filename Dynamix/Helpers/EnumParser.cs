using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Helpers
{
    public class EnumParser
    {
        #region Default Instance Configuration

        public static StringComparer DefaultStringComparer { get; set; } = StringComparer.InvariantCulture;
        public static Func<string, bool> DefaultNullComparer { get; set; } = IsNull;

        #endregion

        #region Singletons

        public static readonly SystemEnumParser System = new SystemEnumParser();
        public static readonly EnumParser Default = new EnumParser();
        public static readonly EnumParser DefaultIgnoreCase = new EnumParser(StringComparer.InvariantCultureIgnoreCase);

        #endregion

        #region Fields

        private readonly StringComparer stringComparer = DefaultStringComparer;
        private readonly Func<string, bool> nullComparer = IsNull;

        #endregion

        #region Ctor

        public EnumParser(StringComparer stringComparer = null, Func<string, bool> nullComparer = null)
        {
            this.stringComparer = stringComparer ?? DefaultStringComparer;
            this.nullComparer = nullComparer ?? DefaultNullComparer;
        }

        public EnumParser(StringComparer stringComparer)
        {
            this.stringComparer = stringComparer ?? DefaultStringComparer;
        }

        public EnumParser(Func<string, bool> nullComparer)
        {
            this.nullComparer = nullComparer ?? DefaultNullComparer;
        }

        #endregion  

        #region Public API

        public T Parse<T>(object s)
        {
            return (T)Parse(typeof(T), s);
        }

        public bool TryParse<T>(object s, out T result)
        {
            var r = TryParse(typeof(T), s, out var tmp);
            result = (T)tmp;
            return r;
        }

        public object Parse(Type type, object s)
        {
            var r = TryParse(type, s, stringComparer, nullComparer, out var tmp);
            if (!r) throw GetOverflowException(s, type.Name);
            return tmp;
        }

        public bool TryParse(Type type, object s, out object result)
        {
            return TryParse(type, s, stringComparer, nullComparer, out result);
        }


        #endregion

        #region Private API

        private static bool TryParse(Type type, object value, StringComparer stringComparer, Func<string, bool> nullComparer, out object result)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!type.IsEnumOrNullableEnum(out var underlyingType))
                throw new ArgumentException("Given type is not an enum", nameof(type));

            result = type.DefaultOf();

            if (value == null)
                return type.IsNullable();
            else if (value is string s)
                return ParseString(type, s, stringComparer, nullComparer, out result);
            else if (value.GetType().IsNumericOrNullableNumeric()
                || value.GetType().IsEnumOrNullableEnum())
                return TryParseNumericOrEnum(type, value, out result);
            else
                return ParseString(type, value.ToString(), stringComparer, nullComparer, out result);
        }

        private static bool ParseString(Type type, string value, StringComparer stringComparer, Func<string, bool> nullComparer, out object result)
        {
            result = type.DefaultOf();
            var enumType = Nullable.GetUnderlyingType(type) ?? type;
            var underlyingType = Enum.GetUnderlyingType(enumType);

            if (nullComparer(value))
                return type.IsNullable();

            foreach (var v in Enum.GetValues(enumType))
                if (stringComparer.Compare(value, v.ToString()) == 0
                    || stringComparer.Compare(value, Convert.ChangeType(v, underlyingType).ToString()) == 0)
                {
                    result = v;
                    return true;
                }

            return false;
        }

        private static bool TryParseNumericOrEnum(Type type, object value, out object result)
        {
            result = type.DefaultOf();
            var enumType = Nullable.GetUnderlyingType(type) ?? type;

            foreach (var v in Enum.GetValues(enumType))
                if (Decimal.Compare(Convert.ToDecimal(value), Convert.ToDecimal(v)) == 0)
                {
                    result = v;
                    return true;
                }

            return false;
        }

        private static bool IsNull(string s)
        {
            return s.IsNullOrWhiteSpace()
                || StringComparer.InvariantCultureIgnoreCase.Compare(s, "null") == 0;
        }

        private static Exception GetOverflowException(object value, string enumTypeName)
        {
            return new OverflowException($"Value '{value}' is outside the range of the underlying type of enumType {enumTypeName}");
        }

        #endregion
    }
}
