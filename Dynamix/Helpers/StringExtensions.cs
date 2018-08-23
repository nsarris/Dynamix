using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dynamix
{
    internal static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static bool EqualsIgnoreCase(this string str, string value)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(str, value) == 0;
        }

        public static string Left(this string str, int length)
        {
            if (str.Length > length)
                return str.Substring(0, length);
            else
                return str;
        }

        public static string Truncate(this string str, int maxLength, bool addEllipsis = true)
        {
            var r = str.Left(maxLength);
            if (addEllipsis && str.Length > r.Length) r += "...";
            return r;
        }

        public static string Replace(this string str, string oldValue, string newValue, int occuranceCount)
        {
            var regex = new Regex(Regex.Escape(oldValue));
            return regex.Replace(str, newValue, occuranceCount);
        }

        public static List<T> SplitAndConvert<T>(this string str, char separator, bool ignoreIvalidValues = true)
        {
            var r = new List<T>();
            if (str == null) return r;

            foreach (var item in str.Split(separator))
                try
                {
                    var v = (T)Convert.ChangeType(item, typeof(T));
                    r.Add(v);
                }
                catch
                {
                    if (!ignoreIvalidValues)
                        throw;
                }
            return r;
        }

        public static string JoinString(this IEnumerable<string> strings, string separator = "")
        {
            return string.Join(separator, strings);
        }

        public static bool AnyIsNullOrEmpty(this IEnumerable<string> strings)
        {
            return strings.Any(x => string.IsNullOrEmpty(x));
        }

        public static bool AllAreNullOrEmpty(this IEnumerable<string> strings)
        {
            return strings.All(x => string.IsNullOrEmpty(x));
        }

        public static bool StartsWithAny(this string str, params string[] strings)
        {
            return StartsWithAny(str, (IEnumerable<string>)strings);
        }

        public static bool StartsWithAny(this string str, IEnumerable<string> strings)
        {
            foreach (var s in strings)
                if (str.StartsWith(s))
                    return true;
            return false;
        }

        public static string ToCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToLower();
            var sb = new StringBuilder(s);

            sb[0] = s[0].ToString().ToLower()[0];

            return sb.ToString();
        }

        public static string ToPascalCase(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            if (s.Length == 1)
                return s.ToUpper();
            var sb = new StringBuilder(s);

            sb[0] = s[0].ToString().ToUpper()[0];

            return sb.ToString();
        }
        
        public static string ReplaceCharacter(this string value, char find, char replace)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var sb = new StringBuilder();
            for (var i = 0; i < value.Length; i++)
                sb.Append(value[i] == find ? replace : value[i]);

            return sb.ToString();
        }

        public static string FirstNotEmptyOrNull(this IEnumerable<string> items)
        {
            foreach (var item in items)
                if (!string.IsNullOrEmpty(item))
                    return item;

            return null;
        }
    }

}
