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

        public static Expression GetPredicateExpression
            (ParameterExpression instanceParameter, string sourceExpression, ExpressionOperator @operator, object value)
        {
            var left = System.Linq.Dynamic.DynamicExpression.Parse(
                                new[] { instanceParameter }, null, instanceParameter.Name + "." + sourceExpression);

            return GetPredicateExpression(instanceParameter, left, @operator, value);
        }

        public static Expression GetPredicateExpression
            (ParameterExpression instanceParameter, Expression sourceExpression, ExpressionOperator @operator, object value)
        {
            var sourceType = sourceExpression.Type;
            var valueIsNull = value is null || (value is string && ((string)value).ToLower() == "null");

            Expression e = null;

            if (IsCollection(sourceType, out var enumerableTypeDescriptor))
            {
                //validate list item type
                //if numeric types use common

                var right = Expression.Constant((
                    (IEnumerable)value).ToCastedList(sourceType));

                e = BuildCollectionExpression(sourceExpression, @operator, right);
            }
            else if (sourceType == typeof(string))
            {
                var v = valueIsNull ? null : value.ToString();
                var right = Expression.Constant(v);

                e = BuildEquitableExpression(sourceExpression, @operator, right)
                    ?? BuildComparableExpression(sourceExpression, @operator, right)
                    ?? BuildStringSpecificExpression(sourceExpression, @operator, right);
            }
            else
            {
                var underlyingType = Nullable.GetUnderlyingType(sourceType);
                var effectiveSourceType = underlyingType ?? sourceType;
                var valueType = value?.GetType();
                var isNullable = underlyingType != null;
                var isNullCheck = @operator == ExpressionOperator.IsNullOrEmpty || @operator == ExpressionOperator.IsNull;
                var isNotNullCheck = @operator == ExpressionOperator.IsNotNullOrEmpty || @operator == ExpressionOperator.IsNotNull;
                var isEmptyCheck = @operator == ExpressionOperator.IsEmpty || @operator == ExpressionOperator.IsNotEmpty;
                var left = sourceExpression;

                if (isEmptyCheck)
                    throw new Exception("IsEmpty can only be used with strings and collections");

                if (isNullCheck || isNotNullCheck)
                {
                    valueIsNull = true; value = null;
                    @operator = isNullCheck ? ExpressionOperator.Equals : ExpressionOperator.DoesNotEqual;
                }

                if (valueIsNull)
                {
                    if (!isNullable)
                        return Expression.Constant(isNotNullCheck);
                    else
                        return @operator == ExpressionOperator.Equals ?
                            Expression.Equal(sourceExpression, nullConstantExpresion) :
                            Expression.NotEqual(sourceExpression, nullConstantExpresion);
                }



                if (IsEnum(sourceType, out var enumUnderlyingType))
                {
                    //Validate
                    //Parse

                    var conversionType = NumericTypeHelper.GetCommonTypeForConvertion(enumUnderlyingType, valueType);

                    left = ExpressionEx.ConvertIfNeeded(left, conversionType);
                    var right = Expression.Constant(Convert.ChangeType(value, conversionType));
                    
                    e = BuildEquitableExpression(left, @operator, right)
                        ?? BuildComparableExpression(left, @operator, right);
                }
                if (IsBoolean(sourceType))
                {
                    //Validate
                    //Parse

                    var v = Convert.ToBoolean(value);
                    
                    e = BuildEquitableExpression(left, @operator, v ? trueConstantExpresion : falseConstantExpresion);
                }
                if (IsNumericType(sourceType, out var numericTypeDefinition))
                {
                    //Validate
                    if (!IsNumericType(valueType)
                        && !NumericValueParser.TryParse(effectiveSourceType, value.ToString(), out value))
                        throw new InvalidOperationException($"Value {value} is not a number or cannot be converted to type {effectiveSourceType.Name}");

                    valueType = value.GetType();

                    if (isNullable)
                        left = Expression.Property(left, nameof(Nullable<int>.Value));

                    
                    var conversionType = NumericTypeHelper.GetCommonTypeForConvertion(sourceType, valueType);
                    if (conversionType != valueType)
                        value = Convert.ChangeType(value, conversionType);

                    left = ExpressionEx.ConvertIfNeeded(sourceExpression, conversionType);
                    var right = Expression.Constant(value);

                    e = BuildEquitableExpression(left, @operator, right)
                        ?? BuildComparableExpression(left, @operator, right);
                }
                if (IsDateTimeType(sourceType) || IsTimeSpan(sourceType))
                {
                    //Validate
                    //Parse

                    if (isNullable)
                        sourceExpression = Expression.Property(sourceExpression, nameof(Nullable<int>.Value));

                    var right = Expression.Constant(value);

                    e = BuildEquitableExpression(left, @operator, right)
                        ?? BuildComparableExpression(left, @operator, right);
                }
            }

            if (e == null)
                throw new NotSupportedException("Could not build predicate for property and operator");

            return e;
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

        private static bool IsDateTimeType(Type Type)
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

        private static Expression BuildCollectionExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.IsContainedIn:
                    return GetIsContainedInConstantArrayExpression(left, right);
                case ExpressionOperator.IsNotContainedIn:
                    return Expression.Not(GetIsContainedInConstantArrayExpression(left, right));
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

        private static bool IsEnum(Type type, out Type underlyingType)
        {
            underlyingType = null;

            if (type.IsEnum)
            {
                underlyingType = Enum.GetUnderlyingType(type);
                return true;
            }
            
            var u = Nullable.GetUnderlyingType(type);

            return (u != null) ? IsEnum(u, out underlyingType) : false;
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
    }


}
