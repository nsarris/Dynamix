using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class NumericTypeDescriptor
    {
        public Type Type { get; }
        public Type EffectiveType { get; }
        public int SizeBits { get; }
        public byte SizeBytes { get; }
        public bool Signed { get; }
        public bool Nullable { get; }
        public bool IsIntegral { get; }
        public NumericTypeDescriptor(Type type)
        {
            var nullableUnderlyingType = System.Nullable.GetUnderlyingType(type);

            Type = type;
            EffectiveType = nullableUnderlyingType ?? type;
            
            if (!sizeInBytes.TryGetValue(EffectiveType, out var sizeBytes))
                throw new InvalidOperationException("Type " + type.Name + " is not a numeric type");

            SizeBytes = sizeBytes;
            Signed = signedTypes.Contains(EffectiveType);
            IsIntegral = integralTypes.Contains(EffectiveType);
            Nullable = nullableUnderlyingType != null;
            SizeBits = (byte)(SizeBytes * 8);
        }

        internal static Type[] SupportedTypes { get; }
            = new Type[]
            {
                typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
                 typeof(int), typeof(uint), typeof(long), typeof(ulong),
                 typeof(float),typeof(double),typeof(decimal)
            };

        
        readonly static Dictionary<Type, byte> sizeInBytes = new Dictionary<Type, byte>()
        {
            {  typeof(sbyte), 1 }, {  typeof(byte), 1 },
            {  typeof(short), 2 }, {  typeof(ushort), 2 },
            {  typeof(int), 3 }, {  typeof(uint), 3 },
            {  typeof(long), 4 }, {  typeof(ulong), 4 },
            {  typeof(float), 3 }, {  typeof(double), 4 },
            {  typeof(decimal), 16 }
        };

        readonly static Type[] integralTypes =
            new Type[]
            {
                typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
                typeof(int), typeof(uint), typeof(long), typeof(ulong),
            };

        readonly static Type[] signedTypes =
            new Type[]
            {
                typeof(sbyte), typeof(short),  typeof(int), typeof(long),
                typeof(float), typeof(double), typeof(decimal)
            };
    }

    public static class NumericTypeHelper
    {
        static readonly Dictionary<Type, NumericTypeDescriptor> numericTypes;
        static readonly byte maxIntegralSizeBytes;
        static readonly Type commonConvertibleType;

        public static IEnumerable<Type> SupportedTypes => numericTypes.Keys;

        static NumericTypeHelper()
        {
            numericTypes = NumericTypeDescriptor.SupportedTypes
                .Concat(NumericTypeDescriptor.SupportedTypes
                    .Select(x => typeof(Nullable<>).MakeGenericType(x)))
                .ToDictionary(x => x, x => new NumericTypeDescriptor(x));

            
            
            maxIntegralSizeBytes = numericTypes.Values.Where(x => x.IsIntegral).Max(x => x.SizeBytes);

            var maxSizeBytes = numericTypes.Values.Where(x => !x.IsIntegral && x.Signed).Max(x => x.SizeBytes);

            if (maxSizeBytes > 0)
                commonConvertibleType = numericTypes.Values.Where(x => x.SizeBytes == maxSizeBytes).Select(x => x.EffectiveType).FirstOrDefault();
        }

        public static bool IsNumericType(Type type, bool includeNullable = true)
        {

            if (numericTypes.TryGetValue(type, out var numericTypeDefinition))
                return includeNullable || !numericTypeDefinition.Nullable;
            else
                return false;
        }

        public static bool IsNumericType(Type type, out NumericTypeDescriptor numericTypeDefinition, bool includeNullable = true)
        {
            return numericTypes.TryGetValue(type, out numericTypeDefinition);
        }

        public static NumericTypeDescriptor GetNumericTypeDefinition(Type type)
        {
            if (numericTypes.TryGetValue(type, out var numericTypeDefinition))
                return numericTypeDefinition;
            else
                throw new InvalidOperationException("Type " + type.Name + " is not a numeric type");
        }

        public static Type GetCommonTypeForConvertion(Type left, Type right)
        {
            if (left == right)
                return left;

            var leftDefinition = GetNumericTypeDefinition(left);
            var rightDefinition = GetNumericTypeDefinition(right);

            Type returnType;

            if (leftDefinition.EffectiveType == rightDefinition.EffectiveType)
            {
                returnType = leftDefinition.EffectiveType;
            }
            else if (leftDefinition.IsIntegral && rightDefinition.IsIntegral)
            {
                var maxSize = Math.Max(leftDefinition.SizeBytes, rightDefinition.SizeBytes);

                if (leftDefinition.Signed == rightDefinition.Signed)
                    returnType = leftDefinition.SizeBytes > rightDefinition.SizeBytes ?
                        leftDefinition.EffectiveType : rightDefinition.EffectiveType;
                else if (maxSize == maxIntegralSizeBytes)
                    returnType = commonConvertibleType;
                else
                    returnType = numericTypes.Values.FirstOrDefault(x => x.Signed && x.SizeBytes == maxSize + 1).EffectiveType;
            }
            else if(!leftDefinition.IsIntegral && !rightDefinition.IsIntegral)
            {
                returnType = leftDefinition.SizeBytes > rightDefinition.SizeBytes ?
                       leftDefinition.EffectiveType : rightDefinition.EffectiveType;
            }
            else 
            {
                returnType = leftDefinition.IsIntegral ?
                       leftDefinition.EffectiveType : 
                       rightDefinition.EffectiveType;
            }

            if (left.IsNullable() || right.IsNullable())
                return typeof(Nullable<>).MakeGenericTypeCached(returnType);

            return returnType;
        }

        public static Type GetCommonTypeForConvertion(params Type[] types)
        {
            return types.Aggregate((current, next) => GetCommonTypeForConvertion(current, next));
        }

        public static void ConvertToComparableType(ref object left, ref object right, out Type comparableType)
        {
            AssertNumericType(left, true);
            AssertNumericType(right, true);

            if (left == null && right == null)
                comparableType = typeof(int?);
            else if (left == null)
                comparableType = right.GetType();
            else if (right == null)
                comparableType = left.GetType();
            else
                comparableType = GetCommonTypeForConvertion(left.GetType(), right.GetType());

            left = Convert.ChangeType(left, comparableType);
            right = Convert.ChangeType(left, comparableType);
        }

        public static object ConvertToComparableType(object value, Type targetType, out Type comparableType)
        {
            comparableType = targetType;

            AssertNumericType(targetType);
            AssertNumericType(value, true);

            if (value?.GetType() == targetType)
                return value;

            comparableType =
                value == null ? targetType : GetCommonTypeForConvertion(value.GetType(), targetType);

            return Convert.ChangeType(value, comparableType);
        }

        public static object ConvertToComparableType(object value, Type targetType)
        {
            return ConvertToComparableType(value, targetType, out var _);
        }

        public static void ConvertToComparableType(ref object left, ref object right)
        {
            ConvertToComparableType(ref left, ref right, out var _);
        }

        private static void AssertNumericType(object value, bool ignoreNull = false)
        {
            if ((value == null && !ignoreNull) ||
                (value != null && !IsNumericType(value.GetType())))
                throw new InvalidOperationException($"Type {value.GetType().Name} is not numeric");
        }

        private static void AssertNumericType(Type numericType)
        {
            if (!IsNumericType(numericType))
                throw new InvalidOperationException($"Type {numericType.Name} is not numeric");
        }

        public static object AsNumeric(object value, Type numericType)
        {
            return AsNumeric(value, numericType, out var _);
        }
        public static object AsNumeric(object value, Type numericType, out Type comparableType)
        {
            return AsNumeric(value, numericType, NumberStyles.Any, CultureInfo.CurrentCulture.NumberFormat, out comparableType);
        }

        public static object AsNumeric(object value, Type numericType, NumberStyles numberStyles, IFormatProvider formatProvider)
        {
            return AsNumeric(value, numericType, NumberStyles.Any, CultureInfo.CurrentCulture.NumberFormat, out var _);
        }

        public static object AsNumeric(object value, Type numericType, NumberStyles numberStyles, IFormatProvider formatProvider, out Type comparableType)
        {
            AssertNumericType(numericType);

            comparableType = null;

            if (value is null)
            {
                if (Nullable.GetUnderlyingType(numericType) != null)
                    return null;
                else
                    throw new InvalidOperationException($"Cannot convert from null to a non nullable type of {numericType.Name}");
            }

            var effectiveType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
            var enumUnderlyingType = effectiveType.IsEnum ? Enum.GetUnderlyingType(effectiveType) : null;

            if (enumUnderlyingType != null)
                value = Convert.ChangeType(value, enumUnderlyingType);
            else
            {

                if (!IsNumericType(value.GetType())
                            && !NumericValueParser.TryParse(numericType, value.ToString(), numberStyles, formatProvider, out value))
                    throw new InvalidOperationException($"Value {value} is not a number or cannot be converted to type {numericType.Name}");
            }

            comparableType = GetCommonTypeForConvertion(numericType, value.GetType());
            if (comparableType != value.GetType())
                value = Convert.ChangeType(value, comparableType);

            return value;
        }

        public static object AsNumericFromEnum(object value, Type enumType)
        {
            return AsNumericFromEnum(value, enumType, out var _);
        }

        public static object AsNumericFromEnum(object value, Type enumType, out Type comparableType)
        {
            comparableType = null;

            var effectiveValueType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
            var effectiveEnumType = Nullable.GetUnderlyingType(enumType) ?? enumType;
            
            if (value is null)
            {
                if (enumType == effectiveEnumType)
                    return null;
                else
                    throw new InvalidOperationException($"Cannot convert from null to a non nullable target type of ${enumType.Name}");
            }      

            if (!effectiveEnumType.IsEnum)
                throw new InvalidOperationException($"Type {effectiveEnumType.Name} is not an enum");

            var enumUnderlyingType = effectiveEnumType.IsEnum ? Enum.GetUnderlyingType(effectiveEnumType) : null;

            if (!effectiveValueType.IsEnum
                && !IsNumericType(value.GetType()) 
                && !EnumParser.TryParse(enumType, value.ToString(), out value))
                throw new InvalidOperationException($"Value {value} is not a number or cannot be converted to enum type {enumType.Name}");

            value = ConvertToComparableType(EnumToNumber(value), enumUnderlyingType, out comparableType);

            return value;
        }

        private static object EnumToNumber(object value)
        {
            if (value == null) return null;

            var effectiveValueType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();

            if (!effectiveValueType.IsEnum)
                return value;

            var underlyingEnumType = Enum.GetUnderlyingType(effectiveValueType);

            return Convert.ChangeType(value, underlyingEnumType);
        }

        public static object NarrowNumber(object value)
        {
            if (value == null)
                return null;

            if (!value.GetType().IsNumeric())
                throw new InvalidOperationException("Value is not a number");

            var valueAsDecimal = (decimal)value;
            decimal wholePart = Math.Truncate(valueAsDecimal);

            if (wholePart == valueAsDecimal)
            {
                if (sbyte.MinValue <= wholePart && wholePart <= sbyte.MaxValue)
                    return (sbyte)wholePart;
                if (byte.MinValue <= wholePart && wholePart <= byte.MaxValue)
                    return (byte)wholePart;
                if (short.MinValue <= wholePart && wholePart <= short.MaxValue)
                    return (short)wholePart;
                if (ushort.MinValue <= wholePart && wholePart <= ushort.MaxValue)
                    return (ushort)wholePart;
                if (int.MinValue <= wholePart && wholePart <= int.MaxValue)
                    return (int)wholePart;
                if (uint.MinValue <= wholePart && wholePart <= uint.MaxValue)
                    return (uint)wholePart;
                if (long.MinValue <= wholePart && wholePart <= long.MaxValue)
                    return (long)wholePart;
                if (ulong.MinValue <= wholePart && wholePart <= ulong.MaxValue)
                    return (ulong)wholePart;
            }
            else
            {
                if (new decimal(float.MinValue) <= valueAsDecimal && valueAsDecimal <= new decimal(float.MinValue))
                    return (float)value;
                if (new decimal(double.MinValue) <= valueAsDecimal && valueAsDecimal <= new decimal(double.MinValue))
                    return (double)value;
            }
            return value;
        }
    }
}
