using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Reflection;

namespace Dynamix
{
    public static class NumericValueParser
    {
        readonly static Dictionary<Type, INumericValueParser> parsers
            = NumericTypeDefinition.SupportedTypes
            .ToDictionary(
                x => x,
                x => (INumericValueParser)Activator.CreateInstance(
                        typeof(NumericValueParser<>).MakeGenericType(x)));


        private static NumericValueParser<T> GetParser<T>()
            where T : struct
        {
            return (NumericValueParser<T>)parsers[typeof(T)];
        }

        private static INumericValueParser GetParser(Type type)
        {
            var effectiveType = Nullable.GetUnderlyingType(type) ?? type;
            return parsers[effectiveType];
        }

        private static bool IsNullable<T>()
        {
            return Nullable.GetUnderlyingType(typeof(T)) != null;
        }

        private static bool IsNullable(Type t)
        {
            return Nullable.GetUnderlyingType(t) != null;
        }


        public static T Parse<T>(string s)
        {
            var parser = GetParser(typeof(T));
            return (T)(IsNullable<T>() ?
                parser.ParseNullable(s) :
                parser.Parse(s));
        }
        public static T Parse<T>(string s, NumberStyles style, IFormatProvider provider)
        {
            var parser = GetParser(typeof(T));
            return (T)(IsNullable<T>() ?
                parser.ParseNullable(s, style, provider) :
                parser.Parse(s, style, provider));
        }
        public static T Parse<T>(string s, IFormatProvider provider) 
        {
            var parser = GetParser(typeof(T));
            return (T)(IsNullable<T>() ?
                parser.ParseNullable(s, provider) :
                parser.Parse(s, provider));
        }
        public static T Parse<T>(string s, NumberStyles style)
        {
            var parser = GetParser(typeof(T));
            return (T)(IsNullable<T>() ?
                parser.ParseNullable(s, style) :
                parser.Parse(s, style));
        }

        public static bool TryParse<T>(string s, out T result)
        {
            var parser = GetParser(typeof(T));
            var r = (IsNullable<T>() ?
                parser.TryParseNullable(s, out object tmp) :
                parser.TryParse(s, out tmp));
            result = (T)tmp;
            return r;
        }
        public static bool TryParse<T>(string s, NumberStyles style, IFormatProvider provider, out T result)
        {
            var parser = GetParser(typeof(T));
            var r = (IsNullable<T>() ?
                parser.TryParseNullable(s, style, provider, out object tmp) :
                parser.TryParse(s, style, provider, out tmp));
            result = (T)tmp;
            return r;
        }

        public static object Parse(Type type, string s)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.ParseNullable(s) :
                parser.Parse(s);
        }
        public static object Parse(Type type, string s, NumberStyles style, IFormatProvider provider)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.ParseNullable(s, style, provider) :
                parser.Parse(s, style, provider);
        }
        public static object Parse(Type type, string s, IFormatProvider provider)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.ParseNullable(s, provider) :
                parser.Parse(s, provider);
        }
        public static object Parse(Type type, string s, NumberStyles style)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.ParseNullable(s, style) :
                parser.Parse(s, style);
        }

        public static bool TryParse(Type type, string s, out object result)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.TryParseNullable(s, out result) :
                parser.TryParse(s, out result);
        }
        public static bool TryParse(Type type, string s, NumberStyles style, IFormatProvider provider, out object result)
        {
            var parser = GetParser(type);
            return IsNullable(type) ?
                parser.TryParseNullable(s, style, provider, out result) :
                parser.TryParse(s, style, provider, out result);
        }
    }

    internal interface INumericValueParser
    {
        object Parse(string s);
        object Parse(string s, NumberStyles style, IFormatProvider provider);
        object Parse(string s, IFormatProvider provider);
        object Parse(string s, NumberStyles style);

        bool TryParse(string s, out object result);
        bool TryParse(string s, NumberStyles style, IFormatProvider provider, out object result);

        object ParseNullable(string s);
        object ParseNullable(string s, NumberStyles style, IFormatProvider provider);
        object ParseNullable(string s, IFormatProvider provider);
        object ParseNullable(string s, NumberStyles style);

        bool TryParseNullable(string s, out object result);
        bool TryParseNullable(string s, NumberStyles style, IFormatProvider provider, out object result);
    }

    internal class NumericValueParser<T> : INumericValueParser
        where T : struct
    {
        private delegate T ParseDelegate1(string s);
        private delegate T ParseDelegate2(string s, NumberStyles style, IFormatProvider provider);
        private delegate T ParseDelegate3(string s, IFormatProvider provider);
        private delegate T ParseDelegate4(string s, NumberStyles style);

        private delegate bool TryParseDelegate1(string s, out T result);
        private delegate bool TryParseDelegate2(string s, NumberStyles style, IFormatProvider provider, out T result);

        private static readonly ParseDelegate1 parseMethod1;
        private static readonly ParseDelegate2 parseMethod2;
        private static readonly ParseDelegate3 parseMethod3;
        private static readonly ParseDelegate4 parseMethod4;

        private static readonly TryParseDelegate1 tryParseMethod1;
        private static readonly TryParseDelegate2 tryParseMethod2;

        private static readonly string tryParseName = nameof(int.TryParse);
        private static readonly string parseName = nameof(int.Parse);

        static NumericValueParser()
        {
            if (!NumericTypeHelper.IsNumericType(typeof(T)))
                throw new InvalidOperationException($"Cannot use the {nameof(NumericValueParser<T>)} class with non numeric types");

            parseMethod1 = GetMethod<ParseDelegate1>(parseName);
            parseMethod2 = GetMethod<ParseDelegate2>(parseName);
            parseMethod3 = GetMethod<ParseDelegate3>(parseName);
            parseMethod4 = GetMethod<ParseDelegate4>(parseName);

            tryParseMethod1 = GetMethod<TryParseDelegate1>(tryParseName);
            tryParseMethod2 = GetMethod<TryParseDelegate2>(tryParseName);
        }



        private static TDelegate GetMethod<TDelegate>(string name)
            where TDelegate : class
        {
            return
                MemberAccessorDelegateBuilder.MethodBuilder.BuildFromDelegate<TDelegate>(
                typeof(T).GetMethod(name, BindingFlags.Public | BindingFlags.Static, null,
                    typeof(TDelegate).GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray(), null));
        }


        public T Parse(string s) => parseMethod1(s);
        public T Parse(string s, NumberStyles style, IFormatProvider provider) => parseMethod2(s, style, provider);
        public T Parse(string s, IFormatProvider provider) => parseMethod3(s, provider);
        public T Parse(string s, NumberStyles style) => parseMethod4(s, style);

        public bool TryParse(string s, out T result) => tryParseMethod1(s, out result);
        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out T result) => tryParseMethod2(s, style, provider, out result);

        public bool TryParse(string s, out object result)
        {
            var check = tryParseMethod1(s, out var t);
            result = t;
            return check;
        }

        public bool TryParse(string s, NumberStyles style, IFormatProvider provider, out object result)
        {
            var check = tryParseMethod2(s, style, provider, out var t);
            result = t;
            return check;
        }

        object INumericValueParser.Parse(string s) => Parse(s);
        object INumericValueParser.Parse(string s, NumberStyles style, IFormatProvider provider) => Parse(s, style, provider);
        object INumericValueParser.Parse(string s, IFormatProvider provider) => Parse(s, provider);
        object INumericValueParser.Parse(string s, NumberStyles style) => Parse(s, style);


        private bool CheckNull(string s)
        {
            return string.IsNullOrEmpty(s) 
                || StringComparer.OrdinalIgnoreCase.Compare(s,"null") == 0;
        }

        public T? ParseNullable(string s)
        {
            if (CheckNull(s)) return null;
            return (T?)parseMethod1(s);
        }
        public T? ParseNullable(string s, NumberStyles style, IFormatProvider provider)
        {
            if (CheckNull(s)) return null;
            return (T?)parseMethod2(s, style, provider);
        }

        public T? ParseNullable(string s, IFormatProvider provider)
        {
            if (CheckNull(s)) return null;
            return (T?)parseMethod3(s, provider);
        }
        public T? ParseNullable(string s, NumberStyles style)
        {
            if (CheckNull(s)) return null;
            return (T?)parseMethod4(s, style);
        }

        public bool TryParseNullable(string s, out T? result)
        {
            var r = tryParseMethod1(s, out T tmp);
            result = (T?)tmp;
            return r;
        }
        public bool TryParseNullable(string s, NumberStyles style, IFormatProvider provider, out T? result)
        {
            var r = tryParseMethod2(s, style, provider, out var tmp);
            result = (T?)tmp;
            return r;
        }

        public bool TryParseNullable(string s, out object result)
        {
            var r = tryParseMethod1(s, out var tmp);
            result = (T?)tmp;
            return r;
        }

        public bool TryParseNullable(string s, NumberStyles style, IFormatProvider provider, out object result)
        {
            var r = tryParseMethod2(s, style, provider, out var tmp);
            result = (T?)tmp;
            return r;
        }

        object INumericValueParser.ParseNullable(string s) => ParseNullable(s);
        object INumericValueParser.ParseNullable(string s, NumberStyles style, IFormatProvider provider) => ParseNullable(s, style, provider);
        object INumericValueParser.ParseNullable(string s, IFormatProvider provider) => ParseNullable(s, provider);
        object INumericValueParser.ParseNullable(string s, NumberStyles style) => ParseNullable(s, style);
    }
}
