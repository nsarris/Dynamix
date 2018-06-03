//using Dynamix;
//using Dynamix.Reflection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Web;

//namespace Dynamix.PredicateBuilder
//{
//    public static class PredicateBuilder1
//    {
//        static readonly MethodInfoEx stringContainsMethod = typeof(string).GetMethodEx(nameof(string.Contains), new Type[] { typeof(string) });
//        static readonly MethodInfoEx stringStartsWithMethod = typeof(string).GetMethodEx(nameof(string.StartsWith), new Type[] { typeof(string) });
//        static readonly MethodInfoEx stringEndsWithMethod = typeof(string).GetMethodEx(nameof(string.EndsWith), new Type[] { typeof(string) });
//        static readonly MethodInfoEx enumerableContainsMethod = typeof(Enumerable).GetMethodEx(nameof(Enumerable.Contains), new Type[] { typeof(string) });

//        public static Expression GetPredicateExpression
//            (ParameterExpression instanceParameter, string sourceExpression, ExpressionOperator @operator, object value)
//        {
//            var left = System.Linq.Dynamic.DynamicExpression.Parse(
//                                new[] { instanceParameter }, null, instanceParameter.Name + "." + sourceExpression);

//            return GetPredicateExpression(instanceParameter, left, @operator, value);
//        }

//        public static Expression GetPredicateExpression
//            (ParameterExpression instanceParameter, Expression sourceExpression, ExpressionOperator @operator, object value)
//        {
//            var instanceType = instanceParameter.Type;

//            var left = sourceExpression;  System.Linq.Dynamic.DynamicExpression.Parse(
//                                new[] { instanceParameter }, null, instanceParameter.Name + "." + sourceExpression);

//            var expressionType = left.Type;

//            Expression right = null;

//            if (value != null && value.GetType() != typeof(string))
//            {
//                var iface = value.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)).FirstOrDefault();
//                if (iface != null)
//                {
//                    var enumerableType = iface.GetGenericArguments().First();
//                    if (@operator == ExpressionOperator.IsContainedIn)
//                    {
//                        return GetConstantArrayExpression(left, value);
//                    }
//                    else if (@operator == ExpressionOperator.IsNotContainedIn)
//                    {
//                        return Expression.Not(GetConstantArrayExpression(left, value));
//                    }
//                    else
//                        throw new Exception("Enumerable values can only be used with IsContainedIn and IsNotContainedIn");
//                }
//            }

//            if (@operator == ExpressionOperator.IsEmpty || @operator == ExpressionOperator.IsNotEmpty)
//            {
//                if (expressionType == typeof(string))
//                {
//                    Expression e = Expression.Equal(left, Expression.Constant(string.Empty));
//                    if (@operator == ExpressionOperator.IsNotEmpty) e = Expression.Not(e);
//                    return e;
//                }
//                else
//                    throw new Exception("IsEmpty can only be used with strings");
//            }
//            else if ((@operator == ExpressionOperator.IsNotNullOrEmpty || @operator == ExpressionOperator.IsNullOrEmpty)
//                && expressionType == typeof(string))
//            {
//                Expression e = Expression.Or(
//                        Expression.Equal(left, Expression.Constant(string.Empty)),
//                        Expression.Equal(left, Expression.Constant(null)));
//                if (@operator == ExpressionOperator.IsNotNullOrEmpty) e = Expression.Not(e);
//                return e;
//            }
//            else if (@operator == ExpressionOperator.IsNullOrEmpty || @operator == ExpressionOperator.IsNull)
//            {
//                return Expression.Equal(left, Expression.Constant(null));
//            }
//            else if (@operator == ExpressionOperator.IsNotNullOrEmpty || @operator == ExpressionOperator.IsNotNull)
//            {
//                return Expression.Not(Expression.Equal(left, Expression.Constant(null)));
//            }
//            else if (IsNumericType(expressionType))
//            {
//                #region Integral value types

//                if (expressionType == typeof(byte) || expressionType == typeof(byte?))
//                {
//                    right = Expression.Constant(Convert.ToByte(value));

//                    if (expressionType == typeof(byte?))
//                        right = Expression.Convert(right, typeof(byte?));
//                }
//                else if (expressionType == typeof(short) || expressionType == typeof(short?))
//                {
//                    right = Expression.Constant(Convert.ToInt16(value));

//                    if (expressionType == typeof(short?))
//                        right = Expression.Convert(right, typeof(short?));
//                }
//                else if (expressionType == typeof(int) || expressionType == typeof(int?))
//                {
//                    right = Expression.Constant(Convert.ToInt32(value));

//                    if (expressionType == typeof(int?))
//                        right = Expression.Convert(right, typeof(int?));
//                }
//                else if (expressionType == typeof(long) || expressionType == typeof(long?))
//                {
//                    right = Expression.Constant(Convert.ToInt64(value));

//                    if (expressionType == typeof(long?))
//                        right = Expression.Convert(right, typeof(long?));
//                }

//                #endregion

//                #region Floating-point value types

//                else if (expressionType == typeof(float) || expressionType == typeof(float?))
//                {
//                    right = Expression.Constant(Convert.ToSingle(value));

//                    if (expressionType == typeof(float?))
//                        right = Expression.Convert(right, typeof(float?));
//                }
//                else if (expressionType == typeof(double) || expressionType == typeof(double?))
//                {
//                    right = Expression.Constant(Convert.ToDouble(value));

//                    if (expressionType == typeof(double?))
//                        right = Expression.Convert(right, typeof(double?));
//                }

//                #endregion

//                #region Decimal type

//                else if (expressionType == typeof(decimal) || expressionType == typeof(decimal?))
//                {
//                    right = Expression.Constant(Convert.ToDecimal(value));

//                    if (expressionType == typeof(decimal?))
//                        right = Expression.Convert(right, typeof(decimal?));
//                }

//                #endregion

//                else
//                    throw new NotImplementedException();

//                return GetCommonExpression(left, @operator, right);
//            }
//            else if (expressionType == typeof(bool) || expressionType == typeof(bool?))
//            {
//                if (value == null || (value.ToString() == "null"))
//                {
//                    if (expressionType == typeof(bool?))
//                        value = (bool?)null;
//                    else
//                        value = false;
//                }
//                else if (value.ToString().ToUpper() == "TRUE" || (value.ToString().ToUpper() != "FALSE" && value.ToString() != "0"))
//                    value = true;
//                else
//                    value = false;

//                right = Expression.Constant(value);
//                if (expressionType == typeof(bool?))
//                    right = Expression.Convert(right, typeof(bool?));

//                switch (@operator)
//                {
//                    case ExpressionOperator.Equals:
//                        return Expression.Equal(left, right);
//                    case ExpressionOperator.DoesNotEqual:
//                        return Expression.NotEqual(left, right);
//                    default:
//                        throw new NotImplementedException();
//                }
//            }
//            else if (expressionType == typeof(DateTime) || expressionType == typeof(DateTime?))
//            {

//                if (value == null || value.ToString() == "" || (value.ToString() == "null"))
//                {
//                    if (expressionType == typeof(DateTime?))
//                        value = (DateTime?)null;
//                    else
//                        value = default(DateTime);
//                }
//                else
//                    value = Convert.ToDateTime(value);

//                right = Expression.Constant(value);

//                if (expressionType == typeof(DateTime?) && !(value is DateTime?))
//                    right = Expression.Convert(right, typeof(DateTime?));

//                return GetCommonExpression(left, @operator, right);
//            }
//            else if (expressionType == typeof(string))
//            {
//                var v = value.ToString();
//                if (v == "null")
//                    v = null;
//                right = Expression.Constant(v);

//                switch (@operator)
//                {
//                    case ExpressionOperator.Contains:
//                        return Expression.Call(left, stringContainsMethod, right);
//                    case ExpressionOperator.DoesNotContain:
//                        return Expression.Not(Expression.Call(left, stringContainsMethod, right));
//                    case ExpressionOperator.StartsWith:
//                        return Expression.Call(left, stringStartsWithMethod, right);
//                    case ExpressionOperator.EndsWith:
//                        return Expression.Call(left, stringEndsWithMethod, right);
//                    case ExpressionOperator.Equals:
//                        return Expression.Equal(left, right);
//                    case ExpressionOperator.DoesNotEqual:
//                        return Expression.NotEqual(left, right);
//                    case ExpressionOperator.IsContainedIn:
//                        return Expression.Call(right, stringContainsMethod, left);
//                    case ExpressionOperator.IsNotContainedIn:
//                        return Expression.Not(Expression.Call(right, stringContainsMethod, left));
//                    default:
//                        throw new NotImplementedException();
//                }
//            }



//            else if (expressionType == typeof(TimeSpan) || expressionType == typeof(TimeSpan?))
//            {
//                var v = (TimeSpan)value;
//                right = Expression.Constant(v);

//                if (expressionType == typeof(TimeSpan?))
//                    right = Expression.Convert(right, typeof(TimeSpan?));

//                return GetCommonExpression(left, @operator, right);
//            }

//            else if (IsEnumOrNullableEnum(expressionType))
//            {
//                var v = Convert.ToInt32(value);
//                right = Expression.Constant(v);
//                right = Expression.Convert(right, expressionType);

//                return GetCommonExpression(left, @operator, right);
//            }
//            else
//                throw new NotImplementedException();

//        }

//        private static Expression GetConstantArrayExpression(Expression sourceExpression, object value)
//        {
//            var castedValue = new DynamicQueryable(((System.Collections.IEnumerable)value).AsQueryable()).ToCastList(sourceExpression.Type);

//            return Expression.Call(Expression.Constant(castedValue), enumerableContainsMethod, sourceExpression);
//        }


//        private static bool IsNumericType(Type Type)
//        {
//            return (
//                Type.Equals(typeof(byte)) || Type.Equals(typeof(byte?)) ||
//                Type.Equals(typeof(short)) || Type.Equals(typeof(short?)) ||
//                Type.Equals(typeof(int)) || Type.Equals(typeof(int?)) ||
//                Type.Equals(typeof(long)) || Type.Equals(typeof(long?)) ||

//                Type.Equals(typeof(float)) || Type.Equals(typeof(float?)) ||
//                Type.Equals(typeof(double)) || Type.Equals(typeof(double?)) ||

//                Type.Equals(typeof(decimal)) || Type.Equals(typeof(decimal?))
//            );
//        }
//        private static Expression GetCommonExpression(Expression left, ExpressionOperator Operator, Expression right)
//        {
//            if (Nullable.GetUnderlyingType(left.Type) != null)
//                left = Expression.Property(left, "Value");

//            switch (Operator)
//            {
//                case ExpressionOperator.Equals:
//                    return Expression.Equal(left, right);
//                case ExpressionOperator.DoesNotEqual:
//                    return Expression.NotEqual(left, right);
//                case ExpressionOperator.LessThan:
//                    return Expression.LessThan(left, right);
//                case ExpressionOperator.GreaterThan:
//                    return Expression.GreaterThan(left, right);
//                case ExpressionOperator.LessThanOrEqual:
//                    return Expression.LessThanOrEqual(left, right);
//                case ExpressionOperator.GreaterThanOrEqual:
//                    return Expression.GreaterThanOrEqual(left, right);
//                default:
//                    throw new NotImplementedException();
//            }
//        }
//        private static bool IsEnumOrNullableEnum(Type Type)
//        {
//            if (Type.IsEnum)
//                return true;

//            Type u = Nullable.GetUnderlyingType(Type);
//            return (u != null) && u.IsEnum;
//        }

//        private static bool IsNullable(Type Type)
//        {
//            return Nullable.GetUnderlyingType(Type) != null;
//        }
//    }
//}