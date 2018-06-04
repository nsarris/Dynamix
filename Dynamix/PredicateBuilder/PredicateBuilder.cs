using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Dynamix.PredicateBuilder
{

    public static class PredicateBuilder
    {
        static readonly MethodInfoEx stringContainsMethod = typeof(string).GetMethodEx(nameof(string.Contains), new Type[] { typeof(string) });
        static readonly MethodInfoEx stringStartsWithMethod = typeof(string).GetMethodEx(nameof(string.StartsWith), new Type[] { typeof(string) });
        static readonly MethodInfoEx stringEndsWithMethod = typeof(string).GetMethodEx(nameof(string.EndsWith), new Type[] { typeof(string) });
        static readonly MethodInfoEx enumerableContainsMethod = typeof(Enumerable).GetMethodEx(nameof(Enumerable.Contains), new Type[] { typeof(string) });

        static readonly MethodInfoEx enumerableAnyMethod = typeof(Enumerable).GetMethodEx(nameof(Enumerable.Any), new Type[] { typeof(IEnumerable<>) });
        static readonly MethodInfoEx enumerableCastMethod = typeof(Enumerable).GetMethodEx(nameof(Enumerable.Cast), new Type[] { typeof(Type) });
        static readonly MethodInfoEx enumerableToListMethod = typeof(Enumerable).GetMethodEx(nameof(Enumerable.ToList), new Type[] { });

        static readonly Expression nullConstantExpresion = Expression.Constant(null);
        static readonly Expression emptyStringConstantExpresion = Expression.Constant(string.Empty);
        static readonly Expression trueConstantExpresion = Expression.Constant(true);
        static readonly Expression falseConstantExpresion = Expression.Constant(true);

        private static readonly string[] trueValueStrings = new[] { "true", "1" };
        private static readonly string[] falseValueStrings = new[] { "false", "0" };

        private enum PredicateDataType
        {
            Number,
            Boolean,
            String,
            DateTime,
            TimeStamp,
            Collection,
            Unsupported
        }
        private class Predicate
        {
            public Expression SourceExpression { get; }
            public ExpressionOperator Operator { get; }
            public object Value { get; set; }
            public bool IsNullable { get; }
            public bool IsNullableType { get; }
            public Type EffectiveType { get; }
            public PredicateDataType PredicateDataType { get; }
            //public bool isEmptyable { get; }
            
            public Predicate(Expression sourceExpression, ExpressionOperator @operator, object value)
            {
                SourceExpression = sourceExpression;
                Operator = @operator;
                Value = value;
                EffectiveType = Nullable.GetUnderlyingType(SourceExpression.Type);
                IsNullableType = EffectiveType != SourceExpression.Type;
                IsNullable = IsNullableType || !EffectiveType.IsValueType;

                if (ValueIsNull(value))
                    value = null;

                PredicateDataType = GetDataType();

                var isEmptyable =
                    PredicateDataType == PredicateDataType.String ||
                    PredicateDataType == PredicateDataType.Collection;

                var equalsNullOrEmptyCheck = Operator == ExpressionOperator.IsNullOrEmpty || Operator == ExpressionOperator.IsNull;
                var notEqualsNullOrEmptyCheck = Operator == ExpressionOperator.IsNotNullOrEmpty || Operator == ExpressionOperator.IsNotNull;

                var isNullCheck = Operator == ExpressionOperator.IsNull || Operator == ExpressionOperator.IsNotNull;
                var isEmptyCheck = Operator == ExpressionOperator.IsEmpty || Operator == ExpressionOperator.IsNotEmpty;
                var isNullOrEmptyCheck = Operator == ExpressionOperator.IsNullOrEmpty || Operator == ExpressionOperator.IsNullOrEmpty;

                if (isNullCheck)
                    value = null;
                if (isEmptyCheck || isNullOrEmptyCheck)
                    value = GetEmptyValue(SourceExpression.Type);
                

                if (!isEmptyable && IsNullable 
                    && (isEmptyCheck || isNullOrEmptyCheck))
                {
                    Operator = equalsNullOrEmptyCheck ? ExpressionOperator.IsNull : ExpressionOperator.IsNotNull;
                    
                }
                else if (isEmptyable && !IsNullable
                    && (isNullCheck || isNullOrEmptyCheck))
                {
                    Operator = equalsNullOrEmptyCheck ? ExpressionOperator.IsEmpty : ExpressionOperator.IsNotEmpty;
                    value = null;
                }
                else if(!isEmptyable && !IsNullable
                    && (isNullCheck || isEmptyCheck || isNullOrEmptyCheck))
                    throw new Exception($"IsNull/IsEmpty cannot be used with a type of {SourceExpression.Type}");
            }

            private object GetEmptyValue(Type type)
            {
                switch (PredicateDataType)
                {
                    case PredicateDataType.String:
                        return string.Empty;
                    case PredicateDataType.Number:
                    case PredicateDataType.Boolean:
                    case PredicateDataType.DateTime:
                    case PredicateDataType.TimeStamp:
                    case PredicateDataType.Collection:
                    case PredicateDataType.Unsupported:
                    default:
                        return null;
                }
            }

            private PredicateDataType GetDataType()
            {
                return
                    SourceExpression.Type == typeof(string) ?
                        PredicateDataType.String :
                    IsNumericType(SourceExpression.Type) ?
                        PredicateDataType.Number :
                    IsBoolean(SourceExpression.Type) ?
                        PredicateDataType.Boolean :
                    IsDateTime(SourceExpression.Type) ?
                        PredicateDataType.DateTime :
                    IsTimeSpan(SourceExpression.Type) ?
                        PredicateDataType.TimeStamp :
                    IsCollection(SourceExpression.Type) ?
                        PredicateDataType.Collection :
                        PredicateDataType.Unsupported;
            }
        }

        public static Expression GetPredicateExpression
            (ParameterExpression instanceParameter, string sourceExpression, ExpressionOperator @operator, object value)
        {
            var left = System.Linq.Dynamic.DynamicExpression.Parse(
                                new[] { instanceParameter }, null, instanceParameter.Name + "." + sourceExpression);

            return GetPredicateExpression(left, @operator, value);
        }

        public static Expression GetPredicateExpression
            (Expression sourceExpression, ExpressionOperator @operator, object value)
        {
            var predicate = new Predicate(sourceExpression, @operator, value);

            if (ValueIsNull(value))
                value = null;

            //if (IsCollection(sourceExpression.Type, out var enumerableTypeDescriptor))
            //{
            //    //validate list item type
            //    //if numeric types use common

            //    var right = Expression.Constant((
            //        (IEnumerable)value).ToCastedList(sourceExpression.Type));

            //    e = BuildCollectionExpression(sourceExpression, @operator, right);
            //}
            //else 
            if (sourceExpression.Type == typeof(string))
            {
                return BuildStringPredicate(sourceExpression, @operator, value);
            }
            else
            {
                return
                    AssertNullCheck(predicate) ??
                    BuildEnumPredicate(sourceExpression, @operator, value) ??
                    BuildBooleanPredicate(sourceExpression, @operator, value) ??
                    BuildNumericPredicate(sourceExpression, @operator, value) ??
                    BuildDateTimePredicate(sourceExpression, @operator, value) ??
                    BuildTimeSpanPredicate(sourceExpression, @operator, value) ??
                    ThrowInvalidPredicateException(sourceExpression, @operator, value);
            }
        }

        private static bool ValueIsNull(object value)
        {
            return (value is null || (value is string && ((string)value).ToLower() == "null"));
        }

        private static Expression AssertNullCheck(Predicate predicate)
        {
            var PredicateDataType = predicate.PredicateDataType;
            var Operator = predicate.Operator;
            var value = predicate.Value;
            var IsNullable = predicate.IsNullable;
            var SourceExpression = predicate.SourceExpression;

            var isEmptyable =
                PredicateDataType == PredicateDataType.String ||
                PredicateDataType == PredicateDataType.Collection;

            var equalsNullOrEmptyCheck = Operator == ExpressionOperator.IsNullOrEmpty || Operator == ExpressionOperator.IsNull;
            var notEqualsNullOrEmptyCheck = Operator == ExpressionOperator.IsNotNullOrEmpty || Operator == ExpressionOperator.IsNotNull;

            var isNullCheck = Operator == ExpressionOperator.IsNull || Operator == ExpressionOperator.IsNotNull;
            var isEmptyCheck = Operator == ExpressionOperator.IsEmpty || Operator == ExpressionOperator.IsNotEmpty;
            var isNullOrEmptyCheck = Operator == ExpressionOperator.IsNullOrEmpty || Operator == ExpressionOperator.IsNullOrEmpty;

            if (isNullCheck)
            {
                value = null;
            }
            if (isEmptyCheck || isNullOrEmptyCheck)
                value = null;// GetEmptyValue(SourceExpression.Type);

            if (!isEmptyable && IsNullable
                && (isEmptyCheck || isNullOrEmptyCheck))
            {
                Operator = equalsNullOrEmptyCheck ? ExpressionOperator.Equals : ExpressionOperator.DoesNotEqual;
                value = null;
            }
            else if (isEmptyable && !IsNullable
                && (isNullCheck || isNullOrEmptyCheck))
            {
                Operator = equalsNullOrEmptyCheck ? ExpressionOperator.Equals : ExpressionOperator.DoesNotEqual;
                value = null; // GetEmptyValue(SourceExpression.Type);
            }
            else if (!isEmptyable && !IsNullable
                && (isNullCheck || isEmptyCheck || isNullOrEmptyCheck))
                throw new Exception($"IsNull/IsEmpty cannot be used with a type of {SourceExpression.Type}");


            if (value is null)
            {
                return
                    !predicate.IsNullable ?
                    predicate.Operator == ExpressionOperator.IsNotNullOrEmpty 
                        || predicate.Operator == ExpressionOperator.IsNotNull ? 
                        
                        trueConstantExpresion : falseConstantExpresion:

                    predicate.Operator == ExpressionOperator.Equals ?

                        Expression.Equal(predicate.SourceExpression, nullConstantExpresion) :
                        Expression.NotEqual(predicate.SourceExpression, nullConstantExpresion);

            }

            return null;
        }

        private static Expression BuildStringPredicate(Expression left, ExpressionOperator @operator, object value)
        {
            if (left.Type != typeof(string))
                return null;

            var v = value?.ToString();
            var right = v is null ? nullConstantExpresion : Expression.Constant(v);

            return BuildEquitableExpression(left, @operator, right)
                ?? BuildComparableExpression(left, @operator, right)
                ?? BuildStringSpecificExpression(left, @operator, right);
        }

        private static Expression BuildNumericPredicate(Expression left, ExpressionOperator @operator, object value)
        {
            if (!IsNumericType(left.Type, out var numericTypeDefinition))
                return null;

            //Validate and convert to numeric if string
            value = AsNumeric(value, numericTypeDefinition.EffectiveType);

            //Convert to a type that can be compared
            var conversionType = NumericTypeHelper.GetCommonTypeForConvertion(left.Type, value.GetType());
            if (conversionType != value.GetType())
                value = Convert.ChangeType(value, conversionType);

            //Get left operand
            if (numericTypeDefinition.Nullable)
                left = Expression.Property(left, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, conversionType);

            //Get right operand
            var right = Expression.Constant(value);

            //Execute
            return BuildEquitableExpression(left, @operator, right)
                ?? BuildComparableExpression(left, @operator, right);
        }

        private static Expression BuildEnumPredicate(Expression left, ExpressionOperator @operator, object value)
        {
            if (!IsEnum(left.Type, out var enumUnderlyingType))
                return null;

            var enumType = Nullable.GetUnderlyingType(left.Type) ?? left.Type;
            var isNullable = enumType != left.Type;

            //Validate and convert to numeric if string
            value = AsNumericFromEnum(value, enumType);

            //Convert to a type that can be compared
            var conversionType = NumericTypeHelper.GetCommonTypeForConvertion(enumUnderlyingType, value.GetType());

            //Get left operand
            if (isNullable)
                left = Expression.Property(left, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, conversionType);

            //Get right operand
            var right = Expression.Constant(Convert.ChangeType(value, conversionType));

            //Execute
            return BuildEquitableExpression(left, @operator, right)
                ?? BuildComparableExpression(left, @operator, right);
        }
   
        private static Expression BuildBooleanPredicate(Expression left, ExpressionOperator @operator, object value)
        {
            if (!IsBoolean(left.Type))
                return null;

            if ((value is bool))
            {
                var v = value.ToString().ToLower();
                if (trueValueStrings.Contains(v))
                    value = true;
                else if (falseValueStrings.Contains(value))
                    value = false;
                else
                    throw new InvalidOperationException($"Value {value} cannot be converted to boolean");
            }


            return BuildEquitableExpression(left, @operator, (bool)value ? trueConstantExpresion : falseConstantExpresion);
        }

        private static Expression BuildDateTimePredicate(Expression left, ExpressionOperator @operator, object value)
        {
            if (!IsDateTime(left.Type))
                return null;

            if (!(value is DateTime d)
                && !DateTime.TryParse(value.ToString(), out d))
                throw new InvalidOperationException($"Value {value} cannot be converted to DateTime");

            var right = Expression.Constant(d);

            return BuildEquitableExpression(left, @operator, right)
                   ?? BuildComparableExpression(left, @operator, right);
        }

        private static Expression BuildTimeSpanPredicate(Expression left, ExpressionOperator @operator, object value)
        {
            if (!IsTimeSpan(left.Type))
                return null;

            TimeSpan timeSpan = TimeSpan.Zero;
            if (value is DateTime d
                || !DateTime.TryParse(value.ToString(), out d))
                timeSpan = d.TimeOfDay;
            else if (value is TimeSpan t
                || !TimeSpan.TryParse(value.ToString(), out t))
                timeSpan = t;
            else
                throw new InvalidOperationException($"Value {value} cannot be converted to TimeSpan");

            var right = Expression.Constant(timeSpan);

            return BuildEquitableExpression(left, @operator, right)
                   ?? BuildComparableExpression(left, @operator, right);
        }

        private static Expression ThrowInvalidPredicateException(Expression left, ExpressionOperator @operator, object value)
        {
            throw new NotSupportedException($"Could not build predicate for expression of type {left.Type} and operator {@operator.ToString()}");
        }

        private static bool IsCollection(Type type)
        {
            var enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(type);

            if (enumerableTypeDescriptor == null)
                return false;

            if (enumerableTypeDescriptor.IsDictionary)
                throw new NotSupportedException("Dictionary not supported as predicate builder property");

            return true;
        }

        private static bool IsCollection(Type type, out EnumerableTypeDescriptor enumerableTypeDescriptor)
        {
            enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(type);

            if (enumerableTypeDescriptor == null)
                return false;

            if (enumerableTypeDescriptor.IsDictionary)
                throw new NotSupportedException("Dictionary not supported as predicate builder property");

            return true;
        }

        private static bool IsDateTime(Type Type)
        {
            return
                Type.Equals(typeof(DateTime)) || Type.Equals(typeof(DateTime?));
        }

        private static bool IsTimeSpan(Type Type)
        {
            return
                Type.Equals(typeof(TimeSpan)) || Type.Equals(typeof(TimeSpan?));
        }

        private static bool IsBoolean(Type Type)
        {
            return
                Type.Equals(typeof(bool)) || Type.Equals(typeof(bool?));
        }

        private static bool IsNumericType(Type type)
        {
            return NumericTypeHelper.IsNumericType(type);
        }

        private static bool IsNumericType(Type type, out NumericTypeDefinition numericTypeDefinition)
        {
            return NumericTypeHelper.IsNumericType(type, out numericTypeDefinition);
        }

        private static object AsNumeric(object value, Type numericType)
        {
            if (value is null)
                return null;

            if (!IsNumericType(value.GetType())
                        && !NumericValueParser.TryParse(numericType, value.ToString(), out value))
                throw new InvalidOperationException($"Value {value} is not a number or cannot be converted to type {numericType.Name}");

            return value;
        }

        private static object AsNumericFromEnum(object value, Type enumType)
        {
            if (value is null)
                return null;

            enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;

            var numericTypeDefinition = NumericTypeHelper.GetNumericTypeDefinition(Enum.GetUnderlyingType(enumType));
            var isNumeric = IsNumericType(value.GetType());

            if (!isNumeric && !EnumParser.TryParse(enumType, value.ToString(), out value))
                throw new InvalidOperationException($"Value {value} is not a number or cannot be converted to enum type {enumType.Name}");

            if (!isNumeric)
                value = Convert.ChangeType(value, numericTypeDefinition.EffectiveType);

            return value;
        }

        private static Expression BuildCollectionAnyExpression(Expression left, ExpressionOperator expressionOperator)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.IsNull:
                    return Expression.Equal(left, nullConstantExpresion);
                case ExpressionOperator.IsNotNull:
                    return Expression.NotEqual(left, nullConstantExpresion);
                case ExpressionOperator.IsEmpty:
                    return GetAnyExpression(left);
                case ExpressionOperator.IsNotEmpty:
                    return GetAnyExpression(left);
                case ExpressionOperator.IsNullOrEmpty:
                    return Expression.Or(
                        Expression.Equal(left, nullConstantExpresion),
                        GetAnyExpression(left));
                case ExpressionOperator.IsNotNullOrEmpty:
                    return Expression.Not(Expression.Or(
                        Expression.Equal(left, nullConstantExpresion),
                        GetAnyExpression(left)));
                default:
                    return null;
            }
        }

        private static Expression BuildCollectionIsContainedExpression(Expression left, ExpressionOperator expressionOperator, object value)
        {
            var right = Expression.Constant((
                    (IEnumerable)value).ToCastedList(left.Type));

            switch (expressionOperator)
            {
                case ExpressionOperator.IsContainedIn:
                    return GetIsContainedInConstantArrayExpression(left, right);
                case ExpressionOperator.IsNotContainedIn:
                    return Expression.Not(GetIsContainedInConstantArrayExpression(left, right));
                default:
                    return null;
            }
        }

        private static Expression GetIsContainedInConstantArrayExpression(Expression source, Expression array)
        {
            return Expression.Call(array, enumerableContainsMethod, source);
        }

        private static Expression GetAnyExpression(Expression source)
        {
            return Expression.Call(source, enumerableAnyMethod);
        }

        private static Expression BuildEquitableExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.Equals:
                    return Expression.Equal(left, right);
                case ExpressionOperator.DoesNotEqual:
                    return Expression.NotEqual(left, right);
                default:
                    return null;
            }
        }

        private static Expression BuildComparableExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.LessThan:
                    return Expression.LessThan(left, right);
                case ExpressionOperator.GreaterThan:
                    return Expression.GreaterThan(left, right);
                case ExpressionOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right);
                case ExpressionOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);
                default:
                    return null;
            }
        }

        private static Expression BuildStringSpecificExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.Contains:
                    return Expression.Call(left, stringContainsMethod, right);
                case ExpressionOperator.DoesNotContain:
                    return Expression.Not(Expression.Call(left, stringContainsMethod, right));
                case ExpressionOperator.StartsWith:
                    return Expression.Call(left, stringStartsWithMethod, right);
                case ExpressionOperator.EndsWith:
                    return Expression.Call(left, stringEndsWithMethod, right);
                case ExpressionOperator.IsContainedIn:
                    return Expression.Call(right, stringContainsMethod, left);
                case ExpressionOperator.IsNotContainedIn:
                    return Expression.Not(Expression.Call(right, stringContainsMethod, left));
                case ExpressionOperator.IsNull:
                    return Expression.Equal(left, nullConstantExpresion);
                case ExpressionOperator.IsNotNull:
                    return Expression.NotEqual(left, nullConstantExpresion);
                case ExpressionOperator.IsEmpty:
                    return Expression.Equal(left, emptyStringConstantExpresion);
                case ExpressionOperator.IsNotEmpty:
                    return Expression.NotEqual(left, emptyStringConstantExpresion);
                case ExpressionOperator.IsNullOrEmpty:
                    return Expression.Or(
                        Expression.Equal(left, emptyStringConstantExpresion),
                        Expression.Equal(left, nullConstantExpresion));
                case ExpressionOperator.IsNotNullOrEmpty:
                    return Expression.Not(Expression.Or(
                        Expression.Equal(left, emptyStringConstantExpresion),
                        Expression.Equal(left, nullConstantExpresion)
                        ));
                default:
                    return null;
            }
        }


        private static bool IsEnum(Type type)
        {
            return (Nullable.GetUnderlyingType(type) ?? type).IsEnum;
        }

        private static bool IsEnum(Type type, out Type underlyingType)
        {
            underlyingType = GetEnumUnderlyingType(type);
            return underlyingType != null;
        }

        private static Type GetEnumUnderlyingType(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type.IsEnum)
                return Enum.GetUnderlyingType(type);
            else
                return null;
        }

        private static object ToCastedList(this IEnumerable enumerable, Type typeToCastTo = null)
        {
            if (typeToCastTo == null)
                typeToCastTo = typeof(object);

            var castMethod = enumerableCastMethod.MethodInfo.MakeGenericMethodCached(typeToCastTo);
            var toListMethod = enumerableToListMethod.MethodInfo.MakeGenericMethodCached(typeToCastTo);

            var castedData = castMethod.Invoke(null, new object[] { enumerable });
            var returnData = toListMethod.Invoke(null, new object[] { castedData });

            return returnData;
        }

        private static TReturn Pipe<T, TReturn>(T p1, params Func<T,TReturn>[] delegates)
            where TReturn : class
        {
            return delegates.Aggregate((TReturn)null,(current,next) => current ?? next(p1));
        }

        private static TReturn Pipe<T1,T2, TReturn>(T1 p1, T2 p2, params Func<T1,T2, TReturn>[] delegates)
            where TReturn : class
        {
            return delegates.Aggregate((TReturn)null, (current, next) => current ?? next(p1, p2));
        }

        private static TReturn Pipe<T1, T2, T3, TReturn>(T1 p1, T2 p2, T3 p3, params Func<T1, T2,T3, TReturn>[] delegates)
            where TReturn : class
        {
            return delegates.Aggregate((TReturn)null, (current, next) => current ?? next(p1, p2, p3));
        }
    }

    public static class Piper
    {
        public static Piper<T1, T2, T3> With<T1, T2, T3>(T1 p1, T2 p2, T3 p3)
        {
            return new Piper<T1, T2, T3>(p1, p2, p3);
        }
    }

    public class Piper<T1,T2,T3>
    {
        readonly T1 p1;
        readonly T2 p2;
        readonly T3 p3;

        //IEnumerable<Func<T1, T2, T3>>

        public Piper(T1 p1, T2 p2, T3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public TReturn Pipe<TReturn>(IEnumerable<Func<T1, T2, T3, TReturn>>[] delegates)
            where TReturn : class
        {
            return Pipe(delegates.ToArray());
        }

        public TReturn Pipe<TReturn>(params Func<T1, T2, T3, TReturn>[] delegates)
            where TReturn : class
        {
            return delegates.Aggregate((TReturn)null, (current, next) => current ?? next(p1, p2, p3));
        }
    }
}
