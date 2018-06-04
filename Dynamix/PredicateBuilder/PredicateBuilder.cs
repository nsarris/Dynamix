using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.PredicateBuilder
{

    public static class PredicateBuilder
    {
        static readonly MethodInfoEx stringContainsMethod = typeof(string).GetMethodEx(nameof(string.Contains), new Type[] { typeof(string) });
        static readonly MethodInfoEx stringStartsWithMethod = typeof(string).GetMethodEx(nameof(string.StartsWith), new Type[] { typeof(string) });
        static readonly MethodInfoEx stringEndsWithMethod = typeof(string).GetMethodEx(nameof(string.EndsWith), new Type[] { typeof(string) });

        static readonly MethodInfo enumerableContainsMethod = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Contains) && x.GetParameters().Length == 2).FirstOrDefault();
        static readonly MethodInfo enumerableAnyMethod = typeof(Enumerable).GetMethods().Where(x => x.Name == nameof(Enumerable.Any) && x.GetParameters().Length == 1).FirstOrDefault();
        static readonly MethodInfo enumerableCastMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
        static readonly MethodInfo enumerableToListMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.ToList));

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
            public Expression Expression { get; }
            public ExpressionOperator Operator { get; }
            public object Value { get; }
            public bool IsNullable { get; }
            public bool IsNullableType { get; }
            public Type Type => Expression.Type;
            public Type EffectiveType { get; }

            public Predicate(Expression sourceExpression, ExpressionOperator @operator, object value)
            {
                Expression = sourceExpression;
                Operator = @operator;
                Value = value;
                EffectiveType = Nullable.GetUnderlyingType(Expression.Type) ?? Expression.Type;
                IsNullableType = EffectiveType != Expression.Type;
                IsNullable = IsNullableType || !EffectiveType.IsValueType;
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

            return
                AssertNullOrEmptyPredicate(predicate) ??
                BuildCollectionPredicate(predicate) ??
                BuildStringPredicate(predicate) ??
                BuildEnumPredicate(predicate) ??
                BuildBooleanPredicate(predicate) ??
                BuildNumericPredicate(predicate) ??
                BuildDateTimePredicate(predicate) ??
                BuildTimeSpanPredicate(predicate) ??
                ThrowInvalidPredicateException(predicate);

        }

        private static bool ValueIsNull(object value)
        {
            return (value is null || (value is string && ((string)value).ToLower() == "null"));
        }

        private static bool IsEmptyable(Type type)
        {
            return type == typeof(string)
                || IsCollection(type);
        }

        private static Expression ConditionalNot(this Expression expression, bool condition)
        {
            return condition ? Expression.Not(expression) : expression;
        }

        private static Expression GetIsNullExpression(this Expression expression)
        {
            return Expression.Equal(expression, nullConstantExpresion);

        }

        private static Expression GetIsNullOrEmptyExpression(this Expression expression)
        {
            return Expression.Or(GetIsNullExpression(expression), GetIsEmptyExpression(expression));
        }

        private static Expression GetIsEmptyExpression(this Expression expression)
        {
            return IsCollection(expression.Type) ?
                    (Expression)Expression.Call(expression, enumerableAnyMethod) :
                    Expression.Equal(expression, GetEmptyValueExpression(expression.Type));
        }

        private static Expression GetEmptyValueExpression(Type type)
        {
            if (type == typeof(string))
                return emptyStringConstantExpresion;
            else
                return nullConstantExpresion;
        }


        private static Expression AssertNullOrEmptyPredicate(Predicate predicate)
        {
            var isNullable = predicate.IsNullable;
            var isEmptyable = IsEmptyable(predicate.Type);
            var @operator = predicate.Operator;

            var value = (ValueIsNull(predicate.Value)) ? null : predicate.Value;

            //Convert Equals/DoesNotEqual null to IsNull/IsNotNull
            if (value is null)
            {
                if (@operator == ExpressionOperator.Equals)
                    @operator = ExpressionOperator.IsNull;
                else if (@operator == ExpressionOperator.DoesNotEqual)
                    @operator = ExpressionOperator.IsNotNull;
            }

            //Convert IsNull to IsEmpty or vice versa if one the two is supported.
            //If none are supported return true only when IsNotNull/IsNotEmpty otherwise false

            if (@operator == ExpressionOperator.IsNull || @operator == ExpressionOperator.IsNotNull)
            {
                var not = @operator == ExpressionOperator.IsNotNull;

                if (isNullable)
                    return predicate.Expression.GetIsNullExpression().ConditionalNot(not);
                else if (isEmptyable)
                    return predicate.Expression.GetIsEmptyExpression().ConditionalNot(not);
                else
                    return not ? trueConstantExpresion : falseConstantExpresion;
            }
            else if (@operator == ExpressionOperator.IsEmpty || @operator == ExpressionOperator.IsNotEmpty)
            {
                var not = @operator == ExpressionOperator.IsNotEmpty;

                if (isEmptyable)
                    return predicate.Expression.GetIsEmptyExpression().ConditionalNot(not);
                if (isNullable)
                    return predicate.Expression.GetIsNullExpression().ConditionalNot(not);
                else
                    return not ? trueConstantExpresion : falseConstantExpresion;
            }
            else if (@operator == ExpressionOperator.IsNullOrEmpty || @operator == ExpressionOperator.IsNullOrEmpty)
            {
                var not = @operator == ExpressionOperator.IsNotNullOrEmpty;
                if (isNullable && isEmptyable)
                    return predicate.Expression.GetIsNullOrEmptyExpression().ConditionalNot(not);
                if (!isNullable && isEmptyable)
                    return predicate.Expression.GetIsEmptyExpression().ConditionalNot(not);
                else if (!isEmptyable && isNullable)
                    return predicate.Expression.GetIsNullExpression().ConditionalNot(not);
                else if (!isNullable && !isEmptyable)
                    return not ? trueConstantExpresion : falseConstantExpresion;
            }
            //Any other operation with null returns false
            else if (value is null)
            {
                return falseConstantExpresion;
            }

            return null;
        }

        private static Expression BuildCollectionPredicate(Predicate predicate)
        {
            if (!IsCollection(predicate.Type, out var enumerableTypeDescriptor))
                return null;

            //TODO: Check element type is supported and try cast value to it
            var right = Expression.Constant(predicate.Value);

            return BuildCollectionSpecificExpression(predicate.Expression, predicate.Operator, right);
        }

        private static Expression BuildStringPredicate(Predicate predicate)
        {
            if (predicate.Type != typeof(string))
                return null;

            var value = predicate.Value?.ToString();
            var right = value is null ? nullConstantExpresion : Expression.Constant(value);

            return BuildEquitableExpression(predicate.Expression, predicate.Operator, right)
                ?? BuildComparableExpression(predicate.Expression, predicate.Operator, right)
                ?? BuildStringSpecificExpression(predicate.Expression, predicate.Operator, right);
        }

        private static Expression BuildNumericPredicate(Predicate predicate)
        {
            if (!IsNumericType(predicate.Type, out var numericTypeDefinition))
                return null;

            //Validate and convert to numeric if string
            var value = AsNumeric(predicate.Value, numericTypeDefinition.EffectiveType);

            //Convert to a type that can be compared
            var conversionType = NumericTypeHelper.GetCommonTypeForConvertion(predicate.Type, value.GetType());
            if (conversionType != value.GetType())
                value = Convert.ChangeType(value, conversionType);

            //Get left operand
            var left = predicate.Expression;
            if (numericTypeDefinition.Nullable)
                left = Expression.Property(predicate.Expression, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, conversionType);

            //Get right operand
            var right = Expression.Constant(value);

            //Execute
            return BuildEquitableExpression(left, predicate.Operator, right)
                ?? BuildComparableExpression(left, predicate.Operator, right);
        }

        private static Expression BuildEnumPredicate(Predicate predicate)
        {
            if (!IsEnum(predicate.Type, out var enumUnderlyingType))
                return null;

            //Validate and convert to numeric if string
            var value = AsNumericFromEnum(predicate.Value, predicate.EffectiveType);

            //Convert to a type that can be compared
            var conversionType = NumericTypeHelper.GetCommonTypeForConvertion(enumUnderlyingType, value.GetType());

            //Get left operand
            var left = predicate.Expression;
            if (predicate.IsNullableType)
                left = Expression.Property(left, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, conversionType);

            //Get right operand
            var right = Expression.Constant(Convert.ChangeType(value, conversionType));

            //Execute
            return BuildEquitableExpression(left, predicate.Operator, right)
                ?? BuildComparableExpression(left, predicate.Operator, right);
        }

        private static Expression BuildBooleanPredicate(Predicate predicate)
        {
            if (!IsBoolean(predicate.Type))
                return null;

            bool value = false;

            if (predicate.Value is bool b)
                value = b;
            else
            {
                var stringValue = predicate.Value.ToString().ToLower() as object;
                if (trueValueStrings.Contains(stringValue))
                    value = true;
                else if (falseValueStrings.Contains(stringValue))
                    value = false;
                else
                    throw new InvalidOperationException($"Value {value} cannot be converted to boolean");
            }

            return BuildEquitableExpression(predicate.Expression, predicate.Operator, value ? trueConstantExpresion : falseConstantExpresion);
        }

        private static Expression BuildDateTimePredicate(Predicate predicate)
        {
            if (!IsDateTime(predicate.Type))
                return null;

            if (!(predicate.Value is DateTime d)
                && !DateTime.TryParse(predicate.Value.ToString(), out d))
                throw new InvalidOperationException($"Value {predicate.Value} cannot be converted to DateTime");

            var right = Expression.Constant(d);

            return BuildEquitableExpression(predicate.Expression, predicate.Operator, right)
                   ?? BuildComparableExpression(predicate.Expression, predicate.Operator, right);
        }

        private static Expression BuildTimeSpanPredicate(Predicate predicate)
        {
            if (!IsTimeSpan(predicate.Type))
                return null;

            TimeSpan timeSpan = TimeSpan.Zero;
            if (predicate.Value is DateTime d
                || !DateTime.TryParse(predicate.Value.ToString(), out d))
                timeSpan = d.TimeOfDay;
            else if (predicate.Value is TimeSpan t
                || !TimeSpan.TryParse(predicate.Value.ToString(), out t))
                timeSpan = t;
            else
                throw new InvalidOperationException($"Value {predicate.Value} cannot be converted to TimeSpan");

            var right = Expression.Constant(timeSpan);

            return BuildEquitableExpression(predicate.Expression, predicate.Operator, right)
                   ?? BuildComparableExpression(predicate.Expression, predicate.Operator, right);
        }

        private static Expression ThrowInvalidPredicateException(Predicate predicate)
        {
            throw new NotSupportedException($"Could not build predicate for expression of type {predicate.Expression.Type} and operator {predicate.Operator.ToString()}");
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

        private static Expression BuildIsContainedInExpression(Expression left, ExpressionOperator expressionOperator, object value)
        {
            var right = Expression.Constant((
                    (IEnumerable)value).ToCastedList(left.Type));

            switch (expressionOperator)
            {
                case ExpressionOperator.IsContainedIn:
                    return Expression.Call(right, enumerableContainsMethod, left);
                case ExpressionOperator.IsNotContainedIn:
                    return Expression.Not(Expression.Call(right, enumerableContainsMethod, left));
                default:
                    return null;
            }
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
                //case ExpressionOperator.IsNull:
                //    return Expression.Equal(left, nullConstantExpresion);
                //case ExpressionOperator.IsNotNull:
                //    return Expression.NotEqual(left, nullConstantExpresion);
                //case ExpressionOperator.IsEmpty:
                //    return Expression.Equal(left, emptyStringConstantExpresion);
                //case ExpressionOperator.IsNotEmpty:
                //    return Expression.NotEqual(left, emptyStringConstantExpresion);
                //case ExpressionOperator.IsNullOrEmpty:
                //    return Expression.Or(
                //        Expression.Equal(left, emptyStringConstantExpresion),
                //        Expression.Equal(left, nullConstantExpresion));
                //case ExpressionOperator.IsNotNullOrEmpty:
                //    return Expression.Not(Expression.Or(
                //        Expression.Equal(left, emptyStringConstantExpresion),
                //        Expression.Equal(left, nullConstantExpresion)
                //        ));
                default:
                    return null;
            }
        }

        private static Expression BuildCollectionSpecificExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.Contains:
                    return Expression.Call(left, enumerableContainsMethod, right);
                case ExpressionOperator.DoesNotContain:
                    return Expression.Not(Expression.Call(left, enumerableContainsMethod, right));
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

            var castedData = enumerableCastMethod.MakeGenericMethodCached(typeToCastTo).Invoke(null, new object[] { enumerable });
            var returnData = enumerableToListMethod.MakeGenericMethodCached(typeToCastTo).Invoke(null, new object[] { castedData });

            return returnData;
        }


    }


}
