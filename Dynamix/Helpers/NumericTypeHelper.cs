using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class NumericTypeDefinition
    {
        public Type Type { get; }
        public Type EffectiveType { get; }
        public int SizeBits { get; }
        public byte SizeBytes { get; }
        public bool Signed { get; }
        public bool Nullable { get; }
        public bool IsIntegral { get; }
        public NumericTypeDefinition(Type type)
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
        static readonly Dictionary<Type, NumericTypeDefinition> numericTypes;
        static readonly byte maxIntegralSizeBytes;
        static readonly Type commonConvertibleType;
        static NumericTypeHelper()
        {
            numericTypes = NumericTypeDefinition.SupportedTypes
                .Concat(NumericTypeDefinition.SupportedTypes
                    .Select(x => typeof(Nullable<>).MakeGenericType(x)))
                .ToDictionary(x => x, x => new NumericTypeDefinition(x));

            
            
            maxIntegralSizeBytes = numericTypes.Values.Where(x => x.IsIntegral).Max(x => x.SizeBytes);

            var maxSizeBytes = numericTypes.Values.Where(x => !x.IsIntegral && x.Signed).Max(x => x.SizeBytes);

            if (maxSizeBytes > 0)
                commonConvertibleType = numericTypes.Values.Where(x => x.SizeBytes == maxSizeBytes).Select(x => x.EffectiveType).FirstOrDefault();
        }

        public static bool IsNumericType(Type type)
        {
            return numericTypes.ContainsKey(type);
        }

        public static bool IsNumericType(Type type, out NumericTypeDefinition numericTypeDefinition)
        {
            return numericTypes.TryGetValue(type, out numericTypeDefinition);
        }

        public static NumericTypeDefinition GetNumericTypeDefinition(Type type)
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

            var targetIsIntegral = leftDefinition.IsIntegral && rightDefinition.IsIntegral;

            if (leftDefinition.IsIntegral && rightDefinition.IsIntegral)
            {
                return
                    leftDefinition.SizeBytes == rightDefinition.SizeBytes 
                        && leftDefinition.SizeBytes == maxIntegralSizeBytes
                        && leftDefinition.Signed != rightDefinition.Signed ?
                        commonConvertibleType :
                        leftDefinition.SizeBytes > rightDefinition.SizeBytes ?
                            leftDefinition.EffectiveType : rightDefinition.EffectiveType;
            }
            else if(!leftDefinition.IsIntegral && !rightDefinition.IsIntegral)
            {
                return leftDefinition.SizeBytes > rightDefinition.SizeBytes ?
                       leftDefinition.EffectiveType : rightDefinition.EffectiveType;
            }
            else 
            {
                return leftDefinition.IsIntegral ?
                       leftDefinition.EffectiveType : 
                       rightDefinition.EffectiveType;
            }
        }

        public static IEnumerable<Type> SupportedTypes => numericTypes.Keys;

        //public static Type[] IntegralTypes { get; } =
        //    new Type[]
        //    {
        //        typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
        //        typeof(int), typeof(uint), typeof(long), typeof(ulong),

        //        typeof(sbyte?), typeof(byte?), typeof(short?), typeof(ushort?),
        //        typeof(int?), typeof(uint), typeof(long), typeof(ulong)
        //    };

        //public static Type[] DecimalTypes { get; } =
        //    new Type[]
        //    {
        //        typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
        //        typeof(int), typeof(uint), typeof(long), typeof(ulong)
        //    };
    }
}
