using Dynamix.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Helpers
{
    public class SystemEnumParser
    {
        readonly static ConcurrentDictionary<Type, IEnumParser> parsers
            = new ConcurrentDictionary<Type, IEnumParser>();

        private static IEnumParser GetParser(Type type)
        {
            var effectiveType = Nullable.GetUnderlyingType(type) ?? type;

            if (!parsers.TryGetValue(effectiveType, out var parser))
            {
                parser = (IEnumParser)Activator.CreateInstance(typeof(EnumParser<>).MakeGenericTypeCached(effectiveType));
                parsers.TryAdd(effectiveType, parser);
            }

            return parser;
        }

        private static bool IsNullable<T>()
        {
            return Nullable.GetUnderlyingType(typeof(T)) != null;
        }

        private static bool IsNullable(Type t)
        {
            return Nullable.GetUnderlyingType(t) != null;
        }


        public T Parse<T>(string s)
        {
            var parser = GetParser(typeof(T));
            return (T)(IsNullable<T>() ?
                parser.ParseNullable(s) :
                parser.Parse(s));
        }
        public T Parse<T>(string s, bool ignoreCase)
        {
            var parser = GetParser(typeof(T));
            return (T)(IsNullable<T>() ?
                parser.ParseNullable(s, ignoreCase) :
                parser.Parse(s, ignoreCase));
        }

        public bool TryParse<T>(string s, out T result)
        {
            var parser = GetParser(typeof(T));
            var r = (IsNullable<T>() ?
                parser.TryParseNullable(s, out object tmp) :
                parser.TryParse(s, out tmp));
            result = (T)tmp;
            return r;
        }
        public bool TryParse<T>(string s, bool ignoreCase, out T result)
        {
            var parser = GetParser(typeof(T));
            var r = (IsNullable<T>() ?
                parser.TryParseNullable(s, ignoreCase, out object tmp) :
                parser.TryParse(s, ignoreCase, out tmp));
            result = (T)tmp;
            return r;
        }

        public object Parse(Type type, string s)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.ParseNullable(s) :
                parser.Parse(s);
        }
        public object Parse(Type type, string s, bool ignoreCase)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.ParseNullable(s, ignoreCase) :
                parser.Parse(s, ignoreCase);
        }


        public bool TryParse(Type type, string s, out object result)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.TryParseNullable(s, out result) :
                parser.TryParse(s, out result);
        }
        public bool TryParse(Type type, string s, bool ignoreCase, out object result)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.TryParseNullable(s, ignoreCase, out result) :
                parser.TryParse(s, ignoreCase, out result);
        }
    }

    internal interface IEnumParser
    {
        object Parse(string s);
        object Parse(string s, bool ignoreCase);

        bool TryParse(string s, out object result);
        bool TryParse(string s, bool ignoreCase, out object result);

        object ParseNullable(string s);
        object ParseNullable(string s, bool ignoreCase);

        bool TryParseNullable(string s, out object result);
        bool TryParseNullable(string s, bool ignoreCase, out object result);
    }

    internal class EnumParser<T> : IEnumParser
        where T : struct, Enum
    {
        public T Parse(string s) => (T)Enum.Parse(typeof(T), s);
        public T Parse(string s, bool ignoreCase) => (T)Enum.Parse(typeof(T), s, ignoreCase);

        public bool TryParse(string s, out T result) => Enum.TryParse<T>(s, out result);
        public bool TryParse(string s, bool ignoreCase, out T result) => Enum.TryParse<T>(s, ignoreCase, out result);


        public bool TryParse(string s, out object result)
        {
            var check = TryParse(s, out T t);
            result = t;
            return check;
        }

        public bool TryParse(string s, bool ignoreCase, out object result)
        {
            var check = TryParse(s, ignoreCase, out T t);
            result = t;
            return check;
        }

        object IEnumParser.Parse(string s) => Parse(s);
        object IEnumParser.Parse(string s, bool ignoreCase) => Parse(s, ignoreCase);


        private bool CheckNull(string s)
        {
            return string.IsNullOrEmpty(s)
                || StringComparer.OrdinalIgnoreCase.Compare(s, "null") == 0;
        }

        public T? ParseNullable(string s)
        {
            if (CheckNull(s)) return null;
            return (T?)Parse(s);
        }
        public T? ParseNullable(string s, bool ignoreCase)
        {
            if (CheckNull(s)) return null;
            return (T?)Parse(s, ignoreCase);
        }

        public bool TryParseNullable(string s, out T? result)
        {
            if (CheckNull(s)) { result = null; return true; }
            var r = TryParse(s, out T tmp);
            result = (T?)tmp;
            return r;
        }
        public bool TryParseNullable(string s, bool ignoreCase, out T? result)
        {
            if (CheckNull(s)) { result = null; return true; }
            var r = TryParse(s, ignoreCase, out T tmp);
            result = (T?)tmp;
            return r;
        }


        object IEnumParser.ParseNullable(string s) => ParseNullable(s);
        object IEnumParser.ParseNullable(string s, bool ignoreCase) => ParseNullable(s, ignoreCase);

        public bool TryParseNullable(string s, out object result)
        {
            var r = TryParseNullable(s, out T? tmp);
            result = tmp;
            return r;
        }

        public bool TryParseNullable(string s, bool ignoreCase, out object result)
        {
            var r = TryParseNullable(s, ignoreCase, out T? tmp);
            result = tmp;
            return r;
        }
    }
}

