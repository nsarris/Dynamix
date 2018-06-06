using Dynamix.Expressions;
using Dynamix.Expressions.Extensions;
using Dynamix.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Dynamix.PredicateBuilder
{

    public class PredicateBuilder
    {
        #region Fields and Properties

        static readonly MethodInfoEx stringContainsMethod = typeof(string).GetMethodEx(nameof(string.Contains), new Type[] { typeof(string) });
        static readonly MethodInfoEx stringStartsWithMethod = typeof(string).GetMethodEx(nameof(string.StartsWith), new Type[] { typeof(string) });
        static readonly MethodInfoEx stringEndsWithMethod = typeof(string).GetMethodEx(nameof(string.EndsWith), new Type[] { typeof(string) });

        EnumerableTypeDescriptor enumerableTypeDescriptor;
        NumericTypeDescriptor numericTypeDescriptor;
        Type enumUnderlyingType;

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
            //TODO: Fix it parameter
            var left = System.Linq.Dynamic.DynamicExpression.Parse(
                                new[] { instanceParameter }, null, instanceParameter.Name + "." + sourceExpression);

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
                type.IsNumericOrNullable(out numericTypeDescriptor) ?
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
            comparableType = Type;
            values.Aggregate(Type, (ct, item) => { AssertCompatibleValue(item, out ct); return ct; });
        }

        private void AssertCompatibleValue(object value, out Type comparableType)
        {
            comparableType = Type;

            if (value == null)
            {
                if (IsNullable)
                    return;
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
                        if (valueType.IsNumericOrNullable())
                        {
                            comparableType = NumericTypeHelper.GetCommonTypeForConvertion(EffectiveType, valueType);
                            return;
                        }
                        break;
                    case PredicateDataType.Enum:
                        if (valueType.IsEnumOrNullableEnum())
                        {
                            comparableType = NumericTypeHelper.GetCommonTypeForConvertion(Enum.GetUnderlyingType(EffectiveType), Enum.GetUnderlyingType(valueType));
                            return;
                        }
                        if (valueType.IsNumericOrNullable())
                        {
                            comparableType = NumericTypeHelper.GetCommonTypeForConvertion(Enum.GetUnderlyingType(EffectiveType), valueType);
                            return;
                        }
                        break;
                }
            }

            throw new InvalidOperationException($"Value '{value}' is not compatible with type {Type.Name}");
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
            else if (@operator == ExpressionOperator.IsNullOrEmpty || @operator == ExpressionOperator.IsNullOrEmpty)
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

            //TODO: Check element type is supported and try cast value to it
            //If numeric times need be casted a where expression is needed
            var right = Expression.Constant(Value);

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

            //Validate and convert to numeric if string
            var value = NumericTypeHelper.AsNumeric(Value, EffectiveType, Configuration.NumberStyles, Configuration.FormatProvider, out var comparableType);

            //Get left operand
            var left = Expression;
            if (numericTypeDescriptor.Nullable)
                left = Expression.Property(Expression, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, comparableType);

            //Get right operand
            var right = Expression.Constant(value);

            //Execute
            return BuildEquitableExpression(left, Operator, right)
                ?? BuildComparableExpression(left, Operator, right);
        }

        private Expression BuildEnumPredicate()
        {
            if (DataType != PredicateDataType.Enum)
                return null;

            //Validate and convert to numeric if string
            var value = NumericTypeHelper.AsNumericFromEnum(Value, EffectiveType, out var comparableType);

            //Get left operand
            var left = Expression;
            if (IsNullableType)
                left = Expression.Property(left, nameof(Nullable<int>.Value));
            left = ExpressionEx.ConvertIfNeeded(left, comparableType);

            //Get right operand
            var right = Expression.Constant(value);

            //Execute
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
            else
            {
                var stringValue = Value.ToString() as object;
                if (Configuration.IsTrueString(Value))
                    value = true;
                else if (Configuration.IsFalseString(Value))
                    value = false;
                else
                    throw new InvalidOperationException($"Value {value} cannot be converted to boolean");
            }

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
            //TODO: fix
            if (Operator == ExpressionOperator.IsContainedIn || Operator == ExpressionOperator.IsNotContainedIn)
            {
                if (Value != null)
                {
                    var enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(Value.GetType());

                    if (enumerableTypeDescriptor == null) //or string
                    {
                        //Try to parse
                        var s = Value.ToString();
                        var items = s
                            .Substring(1, s.Length - 2)
                            .Split(',')
                            //quoted string or else same else null
                            //.Select(x => x[0] == "\" ||)
                            //.Where(x => x != null)
                            ;
                        //if date, bool, time => tryparse each
                        //if enum/numeric to decimal
                        //enumerableTypeDescriptor = EnumerableTypeDescriptor.Get(pasredValue);
                    }

                    if (enumerableTypeDescriptor != null)
                    {
                        var elementType = enumerableTypeDescriptor.ElementType;

                        if (elementType.Is<string>() || elementType.IsOrNullable<bool>() ||
                            elementType.IsOrNullable<DateTime>() || elementType.IsOrNullable<TimeSpan>())
                        {
                            //types dont match
                            if ((Nullable.GetUnderlyingType(elementType) ?? elementType) != EffectiveType)
                                return ExpressionEx.Constants.False;

                            var data = (IEnumerable)Value;// ((IEnumerable)Value).DynamicToList(elementType);

                            return BuildIsContainedInValuesExpression(Expression, data);
                        }
                        else if (elementType.IsEnumOrNullableEnum())
                        {
                            //left must be numeric or enum
                            //cast both to common numeric type
                        }
                        else if (elementType.IsNumericOrNullable())
                        {
                            //left must be numeric
                            //cast both to common numeric type
                        }
                    }
                }
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

        private Expression BuildIsContainedInValuesExpression(Expression expression, IEnumerable values)
        {
            var castedValues = values.Cast<object>().ToArray();

            AssertCompatibleValues(castedValues, out var comparableType);

            var left = ExpressionEx.ConvertIfNeeded(expression, comparableType);

            if (!castedValues.Any())
                return ExpressionEx.Constants.False;
            if (castedValues.Count() == 1)
                return Expression.Equal(
                    left,
                    Expression.Constant(Convert.ChangeType(castedValues.First(), comparableType)));
            else
                return Expression.Call(
                    null,
                    EnumerableExtensions.Methods.Contains.MakeGenericMethodCached(comparableType),
                    Expression.Constant(values.DynamicCast(comparableType)),
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
