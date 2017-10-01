using Dynamix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace Dynamix.PredicateBuilder
{
    public static class PredicateBuilder
    {
        public static Expression GetPredicateExpression
            (Type Type, string Property, ExpressionOperator Operator, object Value, ParameterExpression input = null)
        {
            var property = Type.GetProperty(Property);
            var propertyType = property.PropertyType;

            var left = Expression.Property(input == null ? Expression.Variable(Type, "x") : input, property);
            Expression right = null;


            if (IsNumericType(propertyType))
            {
                #region Integral value types

                if (propertyType == typeof(byte) || propertyType == typeof(byte?))
                {
                    right = Expression.Constant(Convert.ToByte(Value));

                    if (propertyType == typeof(byte?))
                        right = Expression.Convert(right, typeof(byte?));
                }
                else if (propertyType == typeof(short) || propertyType == typeof(short?))
                {
                    right = Expression.Constant(Convert.ToInt16(Value));

                    if (propertyType == typeof(short?))
                        right = Expression.Convert(right, typeof(short?));
                }
                else if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    right = Expression.Constant(Convert.ToInt32(Value));

                    if (propertyType == typeof(int?))
                        right = Expression.Convert(right, typeof(int?));
                }
                else if (propertyType == typeof(long) || propertyType == typeof(long?))
                {
                    right = Expression.Constant(Convert.ToInt64(Value));

                    if (propertyType == typeof(long?))
                        right = Expression.Convert(right, typeof(long?));
                }

                #endregion

                #region Floating-point value types

                else if (propertyType == typeof(float) || propertyType == typeof(float?))
                {
                    right = Expression.Constant(Convert.ToSingle(Value));

                    if (propertyType == typeof(float?))
                        right = Expression.Convert(right, typeof(float?));
                }
                else if (propertyType == typeof(double) || propertyType == typeof(double?))
                {
                    right = Expression.Constant(Convert.ToDouble(Value));

                    if (propertyType == typeof(double?))
                        right = Expression.Convert(right, typeof(double?));
                }

                #endregion

                #region Decimal type

                else if (propertyType == typeof(decimal) || propertyType == typeof(decimal?))
                {
                    right = Expression.Constant(Convert.ToDecimal(Value));

                    if (propertyType == typeof(decimal?))
                        right = Expression.Convert(right, typeof(decimal?));
                }

                #endregion

                else
                    throw new NotImplementedException();

                return GetCommonExpression(left, Operator, right);
            }
            else if (propertyType == typeof(bool) || propertyType == typeof(bool?))
            {
                var v = Convert.ToBoolean(Value);
                right = Expression.Constant(v);

                if (propertyType == typeof(bool?))
                    right = Expression.Convert(right, typeof(bool?));

                switch (Operator)
                {
                    case ExpressionOperator.Equals:
                        return Expression.Equal(left, right);
                    case ExpressionOperator.DoesNotEqual:
                        return Expression.NotEqual(left, right);
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (propertyType == typeof(DateTime) || propertyType == typeof(DateTime?))
            {
                var v = Convert.ToDateTime(Value);
                right = Expression.Constant(v);

                if (propertyType == typeof(DateTime?))
                    right = Expression.Convert(right, typeof(DateTime?));

                return GetCommonExpression(left, Operator, right);
            }
            else if (propertyType == typeof(string))
            {
                var v = Value.ToString();
                right = Expression.Constant(v);

                switch (Operator)
                {
                    case ExpressionOperator.Contains:
                        return Expression.Call(left, typeof(string).GetMethod("Contains"), right);
                    case ExpressionOperator.DoesNotContain:
                        return Expression.Not(Expression.Call(left, typeof(string).GetMethod("Contains"), right));
                    case ExpressionOperator.StartsWith:
                        return Expression.Call(left, typeof(string).GetMethods().Where(x => x.Name == "StartsWith").First(), right);
                    case ExpressionOperator.EndsWith:
                        return Expression.Call(left, typeof(string).GetMethods().Where(x => x.Name == "EndsWith").First(), right);
                    case ExpressionOperator.Equals:
                        return Expression.Equal(left, right);
                    case ExpressionOperator.DoesNotEqual:
                        return Expression.NotEqual(left, right);
                    default:
                        throw new NotImplementedException();
                }
            }



            else if (propertyType == typeof(TimeSpan) || propertyType == typeof(TimeSpan?))
            {
                var v = Convert.ToDateTime(Value);
                right = Expression.Constant(v);

                if (propertyType == typeof(TimeSpan?))
                    right = Expression.Convert(right, typeof(TimeSpan?));

                return GetCommonExpression(left, Operator, right);
            }

            else if (isEnumOrNullableEnum(propertyType))
            {
                var v = Convert.ToInt32(Value);
                right = Expression.Constant(v);
                right = Expression.Convert(right, propertyType);

                return GetCommonExpression(left, Operator, right);
            }
            else
                throw new NotImplementedException();

        }


        private static bool IsNumericType(Type Type)
        {
            return (
                Type.Equals(typeof(byte)) || Type.Equals(typeof(byte?)) ||
                Type.Equals(typeof(short)) || Type.Equals(typeof(short?)) ||
                Type.Equals(typeof(int)) || Type.Equals(typeof(int?)) ||
                Type.Equals(typeof(long)) || Type.Equals(typeof(long?)) ||

                Type.Equals(typeof(float)) || Type.Equals(typeof(float?)) ||
                Type.Equals(typeof(double)) || Type.Equals(typeof(double?)) ||

                Type.Equals(typeof(decimal)) || Type.Equals(typeof(decimal?))
            );
        }
        private static Expression GetCommonExpression(MemberExpression left, ExpressionOperator Operator, Expression right)
        {
            switch (Operator)
            {
                case ExpressionOperator.Equals:
                    return Expression.Equal(left, right);
                case ExpressionOperator.DoesNotEqual:
                    return Expression.NotEqual(left, right);
                case ExpressionOperator.LessThan:
                    return Expression.LessThan(left, right);
                case ExpressionOperator.GreaterThan:
                    return Expression.GreaterThan(left, right);
                case ExpressionOperator.LessThanOrEqual:
                    return Expression.LessThanOrEqual(left, right);
                case ExpressionOperator.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(left, right);
                default:
                    throw new NotImplementedException();
            }
        }
        private static bool isEnumOrNullableEnum(Type Type)
        {
            if (Type.IsEnum)
                return true;

            Type u = Nullable.GetUnderlyingType(Type);
            return (u != null) && u.IsEnum;
        }
    }
}