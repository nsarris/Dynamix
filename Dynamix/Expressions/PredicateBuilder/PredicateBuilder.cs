using Dynamix.Expressions;
using Dynamix.Expressions.Extensions;
using Dynamix.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.Expressions.PredicateBuilder
{

    public class PredicateBuilder
    {
        #region Fields and Properties

        private Type enumUnderlyingType;
        private EnumerableTypeDescriptor enumerableTypeDescriptor;
        private NumericTypeDescriptor numericTypeDescriptor;
        

        public Expression Expression { get; }
        public ExpressionOperator Operator { get; }
        public object Value { get; }
        public bool IsNullable { get; }
        public bool IsNullableType { get; }
        public Type Type => Expression.Type;
        public Type EffectiveType { get; }
        public PredicateDataType DataType { get; }
        public PredicateBuilderConfiguration Configuration { get; }
        

        #endregion

        #region ctor

        public PredicateBuilder(Expression sourceExpression, ExpressionOperator @operator, object value, PredicateBuilderConfiguration configuration = null)
        {
            Configuration = configuration ?? PredicateBuilderConfiguration.Default;
            Expression = sourceExpression;
            Operator = @operator;
            Value = value;
            EffectiveType = Nullable.GetUnderlyingType(Expression.Type) ?? Expression.Type;
            IsNullableType = EffectiveType != Expression.Type;
            IsNullable = IsNullableType || !EffectiveType.IsValueType;
            DataType = GetDataType(Expression.Type);
        }

        #endregion

        #region Public API

        public static Expression GetPredicateExpression
            (ParameterExpression instanceParameter, string sourceExpression, ExpressionOperator @operator, object value, PredicateBuilderConfiguration configuration = null)
        {
            var left = MemberExpressionBuilder.GetExpressionSelector(instanceParameter, sourceExpression).Body;
                                
            return GetPredicateExpression(left, @operator, value, configuration);
        }

        public static Expression GetPredicateExpression
            (Expression sourceExpression, ExpressionOperator @operator, object value, PredicateBuilderConfiguration configuration = null)
        {
            var predicateBuilder = new PredicateBuilder(sourceExpression, @operator, value, configuration);

            return predicateBuilder.BuildPredicateExpression();

        }

        public Expression BuildPredicateExpression()
        {
            return
                AssertNullOrEmptyPredicate() ??
                BuildCollectionPredicate() ??
                BuildIsContainedInPredicate() ??
                BuildStringPredicate() ??
                BuildEnumPredicate() ??
                BuildBooleanPredicate() ??
                BuildNumericPredicate() ??
                BuildDateTimePredicate() ??
                BuildTimeSpanPredicate() ??
                ThrowInvalidPredicateException();
        }

        #endregion

        #region Helpers

        private PredicateDataType GetDataType(Type type)
        {
            return
                type.Is<string>() ?
                    PredicateDataType.String :
                type.IsOrNullable<bool>() ?
                    PredicateDataType.Boolean :
                type.IsOrNullable<DateTime>() ?
                    PredicateDataType.DateTime :
                type.IsOrNullable<TimeSpan>() ?
                    PredicateDataType.TimeSpan :
                type.IsEnumOrNullableEnum(out enumUnderlyingType) ?
                    PredicateDataType.Enum :
                type.IsNumericOrNullableNumeric(out numericTypeDescriptor) ?
                    PredicateDataType.Number :
                type.IsEnumerable(out enumerableTypeDescriptor) ?
                    PredicateDataType.Collection :

                    PredicateDataType.Unsupported;
        }

        private bool ValueIsNull(object value)
        {
            return (value is null || Configuration.IsNullString(value));
        }

        private bool GetIsEmptyable()
        {
            return DataType == PredicateDataType.Collection
                || Configuration.GetEmptyValues(DataType).Any();
        }

        private bool TryParseNumber(string s, Type numericType, out object number)
        {
            return NumericValueParser.TryParse(numericType, s, Configuration.NumberStyles, Configuration.FormatProvider, out number);
        }

        private bool TryParseBool(string s, out bool d)
        {
            d = false;
            if (Configuration.IsTrueString(s)) { d = true; return true; }
            else if (Configuration.IsFalseString(s)) { d = true; return false; }
            else return false;
        }

        private bool TryParseDateTime(string s, out DateTime d)
        {
            return Configuration.DateTimeFormats.Length == 0 ?
                DateTime.TryParse(s, Configuration.FormatProvider, Configuration.DateTimeStyles, out d) :
                DateTime.TryParseExact(s, Configuration.DateTimeFormats, Configuration.FormatProvider, Configuration.DateTimeStyles, out d);
        }

        private bool TryParseTimeSpan(string s, out TimeSpan t)
        {
            return Configuration.TimeSpanFormats.Length == 0 ?
                TimeSpan.TryParse(s, Configuration.FormatProvider, out t) :
                TimeSpan.TryParseExact(s, Configuration.TimeSpanFormats, Configuration.FormatProvider, out t);
        }

        private void AssertCompatibleValues(IEnumerable<object> values, out Type comparableType)
        {
            comparableType = values.Aggregate(Type, (ct, item) => { AssertCompatibleValue(item, ct, out ct); return ct; });
        }

        private bool TryParseToPredicateType(string s, out object value)
        {
            value = null;
            if (ValueIsNull(s))
                return !IsNullable;

            if (EffectiveType.Is<string>())
            {
                value = s;
                return true;
            }
            else if (DataType == PredicateDataType.Number)
            {
                var r = TryParseNumber(s, typeof(decimal), out value);
                value = NumericTypeHelper.NarrowNumber(value);
                return r;
            }
            else if (DataType == PredicateDataType.Boolean)
            {
                var r = TryParseBool(s, out var b); value = b; return r;
            }
            else if (DataType == PredicateDataType.DateTime)
            {
                var r = TryParseDateTime(s, out var d); value = d; return r;
            }
            else if (DataType == PredicateDataType.TimeSpan)
            {
                var r = TryParseTimeSpan(s, out var t); value = t; return r;
            }

            return false;
        }

        private static readonly char[] quoteCharacters = new[] { '"', '\'' };

        private string StripQuotes(string s) => StripQuotes(s, quoteCharacters);
        private string StripQuotes(string s, params char[] quoteChars)
        {
            if (s == null) return null;

            foreach (var c in quoteChars)
                if ((s.First() == c) && s.Last() == c)
                    return s.Substring(1, s.Length - 2);

            return s;
        }

        private bool TryParseArray(string s, out IEnumerable<object> array)
        {
            array = new List<object>();

            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (s.StartsWith("[") && s.EndsWith("]"))
            {
                foreach (var x in
                    s.Substring(1, s.Length - 2)
                    .Split(',')
                    .Select(x => StripQuotes(x)))
                {
                    // then find comparable type from smallest and predicate type and cast to it
                    if (!TryParseToPredicateType(x, out var value))
                        return false;
                    ((List<object>)array).Add(value);
                }

            }

            return true;
        }

        private void AssertCompatibleValue(object value, Type targetType, out Type comparableType)
        {
            comparableType = targetType;

            if (value == null)
            {
                if (IsNullable)
                {
                    if(!targetType.IsNullable())
                        comparableType = typeof(Nullable<>).MakeGenericTypeCached(targetType);

                    return;
                }
            }
            else
            {
                var valueType = value.GetType();
                switch (DataType)
                {
                    case PredicateDataType.String:
                        return;
                    case PredicateDataType.Boolean:
                    case PredicateDataType.DateTime:
                    case PredicateDataType.TimeSpan:
                        if (value.GetType().StripNullable() == EffectiveType)
                            return;
                        break;
                    case PredicateDataType.Number:
                        if (valueType.IsEnumOrNullableEnum())
                        {
                            comparableType = 
                                NumericTypeHelper.GetCommonTypeForConvertion(comparableType, EffectiveType, Enum.GetUnderlyingType(valueType));
                            return;
                        }
                        if (valueType.IsNumericOrNullableNumeric())
                        {
                            comparableType = NumericTypeHelper.GetCommonTypeForConvertion(comparableType, EffectiveType, valueType);
                            return;
                        }
                        break;
                    case PredicateDataType.Enum:
                        if (valueType.IsEnumOrNullableEnum())
                        {
                            comparableType = NumericTypeHelper.GetCommonTypeForConvertion(comparableType, enumUnderlyingType, Enum.GetUnderlyingType(valueType));
                            return;
                        }
                        if (valueType.IsNumericOrNullableNumeric())
                        {
                            comparableType = NumericTypeHelper.GetCommonTypeForConvertion(comparableType, enumUnderlyingType, valueType);
                            return;
                        }
                        break;
                }
            }

            throw new InvalidOperationException($"Value '{value}' is not compatible with type {Type.Name}");
        }

        private static object AsNullable(object value)
        {
            if (value == null || value.GetType().IsNullable())
                return value;

            var valueType = value.GetType();

            return typeof(Nullable<>)
                .MakeGenericTypeCached(valueType)
                .GetConstructorEx(new[] { valueType })
                .Invoke(value);
        }

        #endregion

        #region Builders

        private Expression AssertNullOrEmptyPredicate()
        {
            var isNullable = IsNullable;
            var isEmptyable = GetIsEmptyable();
            var @operator = Operator;

            var value = (ValueIsNull(Value)) ? null : Value;

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
                if (!IsNullable)
                {
                    var not = @operator == ExpressionOperator.IsNotNull;

                    if (isEmptyable)
                        @operator = not ? ExpressionOperator.IsNotEmpty : ExpressionOperator.IsEmpty;
                    else
                        return ExpressionEx.Constants.Bool(not);
                }

                return BuildIsNullOrEmptyExpression(Expression, @operator);
            }
            else if (@operator == ExpressionOperator.IsEmpty || @operator == ExpressionOperator.IsNotEmpty)
            {
                if (!isEmptyable)
                {
                    var not = @operator == ExpressionOperator.IsNotEmpty;

                    if (isNullable)
                        @operator = not ? ExpressionOperator.IsNotNull : ExpressionOperator.IsNull;
                    else
                        return ExpressionEx.Constants.Bool(not);
                }

                return BuildIsNullOrEmptyExpression(Expression, @operator);
            }
            else if (@operator == ExpressionOperator.IsNullOrEmpty || @operator == ExpressionOperator.IsNotNullOrEmpty)
            {
                var not = @operator == ExpressionOperator.IsNotNullOrEmpty;

                if (!isNullable && isEmptyable)
                    @operator = not ? ExpressionOperator.IsNotEmpty : ExpressionOperator.IsEmpty;
                else if (!isEmptyable && isNullable)
                    @operator = not ? ExpressionOperator.IsNotNull : ExpressionOperator.IsNull;
                else if (!isNullable && !isEmptyable)
                    return ExpressionEx.Constants.Bool(not);

                return BuildIsNullOrEmptyExpression(Expression, @operator);
            }
            //Any other operation with null returns false
            else if (value is null)
            {
                return ExpressionEx.Constants.False;
            }

            return null;
        }

        private Expression BuildCollectionPredicate()
        {
            if (DataType != PredicateDataType.Collection)
                return null;

            Expression right;
            if (Value != null)
            {
                var valuePredicateDataType = GetDataType(Value.GetType());
                if (valuePredicateDataType == PredicateDataType.Collection
                    || valuePredicateDataType == PredicateDataType.Unsupported)
                    return null;

                right = Expression.Constant(Value);
            }
            else if (IsNullable)
            {
                right = Expression.Constant(new object[] { null });
            }
            else
            {
                right = ExpressionEx.Constants.Bool(Operator == ExpressionOperator.DoesNotContain);
            }
            
            return BuildCollectionSpecificExpression(Expression, Operator, right);
        }

        private Expression BuildStringPredicate()
        {
            if (DataType != PredicateDataType.String)
                return null;

            var value = Value?.ToString();
            var right = value is null ? ExpressionEx.Constants.Null : Expression.Constant(value);

            return
                BuildEquitableExpression(Expression, Operator, right)
                ?? BuildComparableExpression(Expression, Operator, right)
                ?? BuildStringSpecificExpression(Expression, Operator, right);
        }

        private Expression BuildNumericPredicate()
        {
            if (DataType != PredicateDataType.Number)
                return null;

            var value = NumericTypeHelper.AsNumeric(Value, EffectiveType, Configuration.NumberStyles, Configuration.FormatProvider, out var comparableType);

            var left = Expression;
            if (numericTypeDescriptor.Nullable)
                left = Expression.Property(Expression, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, comparableType);

            var right = Expression.Constant(value);

            return BuildEquitableExpression(left, Operator, right)
                ?? BuildComparableExpression(left, Operator, right);
        }

        private Expression BuildEnumPredicate()
        {
            if (DataType != PredicateDataType.Enum)
                return null;

            var value = NumericTypeHelper.AsNumericFromEnum(Value, EffectiveType, out var comparableType);

            var left = Expression;
            if (IsNullableType)
                left = Expression.Property(left, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, comparableType);

            var right = Expression.Constant(value);

            return BuildEquitableExpression(left, Operator, right)
                ?? BuildComparableExpression(left, Operator, right);
        }

        private Expression BuildBooleanPredicate()
        {
            if (DataType != PredicateDataType.Boolean)
                return null;

            bool value = false;

            if (Value is bool b)
                value = b;
            else if (!TryParseBool(Value.ToString(), out value))
                 throw new InvalidOperationException($"Value {value} cannot be converted to boolean");
            
            return BuildEquitableExpression(Expression, Operator, ExpressionEx.Constants.Bool(value));
        }

        private Expression BuildDateTimePredicate()
        {
            if (DataType != PredicateDataType.DateTime)
                return null;

            if (!(Value is DateTime d)
                && !TryParseDateTime(Value.ToString(), out d))
                throw new InvalidOperationException($"Value {Value} cannot be converted to DateTime");

            var right = Expression.Constant(d);

            return BuildEquitableExpression(Expression, Operator, right)
                   ?? BuildComparableExpression(Expression, Operator, right);
        }

        private Expression BuildTimeSpanPredicate()
        {
            if (DataType != PredicateDataType.TimeSpan)
                return null;

            TimeSpan timeSpan = TimeSpan.Zero;

            if (Value is TimeSpan t
                || !TryParseTimeSpan(Value.ToString(), out t))
                timeSpan = t;
            if (Value is DateTime d
                || !TryParseDateTime(Value.ToString(), out d))
                timeSpan = d.TimeOfDay;
            else
                throw new InvalidOperationException($"Value {Value} cannot be converted to TimeSpan");

            var right = Expression.Constant(timeSpan);

            return BuildEquitableExpression(Expression, Operator, right)
                   ?? BuildComparableExpression(Expression, Operator, right);
        }

        private Expression ThrowInvalidPredicateException()
        {
            throw new NotSupportedException($"Could not build predicate for expression of type {Type.Name} and operator {Operator}");
        }

        #endregion

        #region ExpressionBuilders

        private Expression BuildIsContainedInPredicate()
        {
            if (Operator == ExpressionOperator.IsContainedIn || Operator == ExpressionOperator.IsNotContainedIn)
            {
                if (Value != null)
                {
                    var elementType = EnumerableTypeDescriptor.Get(Value.GetType())?.ElementType;

                    if (elementType == null)
                    {
                        if (!TryParseArray(Value.ToString(), out var array))
                            throw new InvalidOperationException($"Expression '{Value}' is not a valid array");

                        return BuildIsContainedInValuesExpression(Expression, array);
                    }
                    else
                    {
                        if (DataType == PredicateDataType.String)
                        {
                            var data = (!elementType.Is<string>()) ?
                                        ((IEnumerable)Value).Cast<object>().Select(x => x?.ToString()) :
                                        (IEnumerable)Value;

                            return BuildIsContainedInValuesExpression(Expression, data);
                        }

                        if (elementType.Is<string>() || elementType.IsOrNullable<bool>() ||
                            elementType.IsOrNullable<DateTime>() || elementType.IsOrNullable<TimeSpan>())
                        {
                            if (elementType.StripNullable() != EffectiveType)
                                //TODO: Configuration option strict type checking
                                return ExpressionEx.Constants.Bool(Operator == ExpressionOperator.IsNotContainedIn);

                            return BuildIsContainedInValuesExpression(Expression, (IEnumerable)Value);
                        }
                        else if (elementType.IsEnumOrNullableEnum() || elementType.IsNumericOrNullableNumeric())
                        {
                            if (DataType != PredicateDataType.Number && DataType != PredicateDataType.Enum)
                                throw new InvalidOperationException($"An array of {elementType.Name} cannot be compared against a {DataType}");

                            return BuildIsContainedInValuesExpression(Expression, (IEnumerable)Value);
                        }
                    }
                }
                else
                    return ExpressionEx.Constants.Bool(Operator == ExpressionOperator.IsNotContainedIn);
            }

            return null;
        }

        private Expression BuildEquitableExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
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

        private Expression BuildComparableExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
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

        private Expression BuildStringSpecificExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.Contains:
                    return ExpressionEx.StringContains(left, right);
                case ExpressionOperator.DoesNotContain:
                    return Expression.Not(ExpressionEx.StringContains(left, right));
                case ExpressionOperator.StartsWith:
                    return ExpressionEx.StringStartsWith(left, right);
                case ExpressionOperator.EndsWith:
                    return ExpressionEx.StringEndsWith(left, right);
                case ExpressionOperator.IsContainedIn:
                    return ExpressionEx.StringContains(right, left);
                case ExpressionOperator.IsNotContainedIn:
                    return Expression.Not(ExpressionEx.StringContains(right, left));
                default:
                    return null;
            }
        }

        private Expression BuildIsNullOrEmptyExpression(Expression left, ExpressionOperator expressionOperator)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.IsNull:
                    return Expression.Equal(left, ExpressionEx.Constants.Null);
                case ExpressionOperator.IsNotNull:
                    return Expression.NotEqual(left, ExpressionEx.Constants.Null);
                case ExpressionOperator.IsEmpty:
                    return BuildIsEmptyExpression(false, left);
                case ExpressionOperator.IsNotEmpty:
                    return BuildIsEmptyExpression(true, left);
                case ExpressionOperator.IsNullOrEmpty:
                    return Expression.Or(
                        BuildIsEmptyExpression(false, left),
                        Expression.Equal(left, ExpressionEx.Constants.Null));
                case ExpressionOperator.IsNotNullOrEmpty:
                    return Expression.Not(Expression.Or(
                        BuildIsEmptyExpression(false, left),
                        Expression.Equal(left, ExpressionEx.Constants.Null)
                        ));
                default:
                    return null;
            }
        }

        private Expression BuildCollectionSpecificExpression(Expression left, ExpressionOperator expressionOperator, Expression right)
        {
            switch (expressionOperator)
            {
                case ExpressionOperator.Contains:
                    return Expression.Call(left, EnumerableExtensions.Methods.Contains, right);
                case ExpressionOperator.DoesNotContain:
                    return Expression.Not(Expression.Call(left, EnumerableExtensions.Methods.Contains, right));
                default:
                    return null;
            }
        }

        private static object AsNumericNullable(object value)
        {
            if (value == null) return null;

            var valueType = value.GetType();

            var nonNullableType = valueType.StripNullable();

            if (valueType.IsEnumOrNullableEnum())
            {
                valueType = Enum.GetUnderlyingType(valueType);
                value = Convert.ChangeType(Convert.ChangeType(value, nonNullableType), valueType);
            }
            else if(valueType.IsNullable())
            {
                return value;
            }

            return typeof(Nullable<>)
                .MakeGenericTypeCached(valueType)
                .GetConstructorEx(new[] { valueType })
                .Invoke(value);
        }

        private Expression BuildIsContainedInValuesExpression(Expression expression, IEnumerable values)
        {
            var elementType = EnumerableTypeDescriptor.Get(values.GetType()).ElementType;
            
            var castedValues = values.Cast<object>().ToArray();

            AssertCompatibleValues(castedValues, out var comparableType);

            values =
                (elementType.IsNullable() && elementType.IsEnumOrNullableEnum()) ?
                castedValues.Select(AsNumericNullable).ToCastedList(comparableType) :
                castedValues.Select(x => x == null ? null : Convert.ChangeType(x, comparableType.StripNullable())).ToCastedList(comparableType);

            var left = ExpressionEx.ConvertIfNeeded(expression, comparableType);

            if (!castedValues.Any())
                return ExpressionEx.Constants.False;
            if (castedValues.Count() == 1)
                return Expression.Equal(
                    left,
                    Expression.Constant(values.DynamicFirst()));
            else
                return Expression.Call(
                    null,
                    EnumerableExtensions.Methods.Contains.MakeGenericMethodCached(comparableType),
                    Expression.Constant(values),
                    left);
        }

        private Expression BuildIsEmptyExpression(bool not, Expression expression)
        {
            return (DataType == PredicateDataType.Collection ?
                    (Expression)Expression.Call(expression, EnumerableExtensions.Methods.Any) :
                    BuildIsContainedInValuesExpression(expression, Configuration.GetEmptyValues(DataType)))
                    .ConditionalNot(not);
        }

        #endregion
    }
}
