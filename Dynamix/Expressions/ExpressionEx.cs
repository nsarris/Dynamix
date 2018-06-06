using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    public static class ExpressionEx
    {
        public static class Constants
        {
            public static Expression Null { get; } = Expression.Constant(null);
            public static Expression EmptyString { get; } = Expression.Constant(string.Empty);
            public static Expression True { get; } = Expression.Constant(true);
            public static Expression False { get; } = Expression.Constant(true);
            public static Expression Bool(bool value)
            {
                return value ? True : False;
            }

            public static Expression Bool(bool? value)
            {
                return value.HasValue ?
                        value.Value ? True : False :
                        null;
            }
        }
        /// <summary>
        /// Returns either a conversion expression if the types differ, or the same expression if they are the same
        /// </summary>
        /// <param name="expression">The source expression</param>
        /// <param name="type">The target type to convert to</param>
        /// <returns></returns>
        public static Expression ConvertIfNeeded(Expression expression, Type type)
        {
            if (type.IsByRef)
                type = type.GetElementType();

            if (type == expression.Type)
                return expression;
            else
                return Expression.Convert(expression, type);
        }
        /// <summary>
        /// Casts an expression to the specified type. If typeSafe is specified it selects Expression.TypeAs over Expression.Convert for reference Types as it's slightly faster. Use with caution as this will produce null and not an exception for invalid cast operations
        /// </summary>
        /// <param name="expression">The source expression</param>
        /// <param name="type">The type to cast to</param>
        /// <param name="typeSafe">Specified is TypeAs will be used for reference types</param>
        /// <returns></returns>
        public static Expression Cast(Expression expression, Type type, bool typeSafe = false)
        {
            if (type == expression.Type)
                return expression;

            if (type.IsValueType)
                return Expression.Convert(expression, type);
            else
                return Expression.TypeAs(expression, type);
        }

        /// <summary>
        /// Casts an expression to the specified type using Expression.TypeAs over Expression.Convert for reference Types as it's slightly faster. Use with caution as this will produce null and not an exception for invalid cast operations
        /// </summary>
        /// <param name="expression">The source expression</param>
        /// <param name="type">The type to cast to</param>
        /// <returns></returns>
        public static Expression CastTypeSafe(Expression expression, Type type)
        {
            return Cast(expression, type, true);
        }

        /// <summary>
        /// Checks if any of the elements of the binary operation and true or false constants and ommits them
        /// </summary>
        /// <param name="left">The left expression</param>
        /// <param name="right">The right expression</param>
        /// <returns>If the deterministic outcome is True/False returns an appropriate constant. Otherwise falls back to Expression.AndAlso()</returns>
        public static Expression AndAlso(Expression left, Expression right)
        {
            if (left != null && right != null
                && (left.NodeType == ExpressionType.Constant
                    || right.NodeType == ExpressionType.Constant))
            { 
                if (left is ConstantExpression leftConstantExpression)
                    return true.Equals(leftConstantExpression.Value) ? right : Constants.False;
                else if (right is ConstantExpression rightConstantExpression)
                    return true.Equals(rightConstantExpression.Value) ? left : Constants.False;
            }

            return Expression.AndAlso(left, right);
        }

        /// <summary>
        /// Checks if any of the elements of the binary operation and true or false constants and ommits them
        /// </summary>
        /// <param name="left">The left expression</param>
        /// <param name="right">The right expression</param>
        /// <returns>If the deterministic outcome is True/False returns an appropriate constant. Otherwise falls back to Expression.AndAlso()</returns>
        public static Expression OrElse(Expression left, Expression right)
        {
            if (left != null && right != null
                && (left.NodeType == ExpressionType.Constant 
                    || right.NodeType == ExpressionType.Constant))
            {
                if (left is ConstantExpression leftConstantExpression)
                    return true.Equals(leftConstantExpression.Value) ? Constants.True : right;
                else if (right is ConstantExpression rightConstantExpression)
                    return true.Equals(rightConstantExpression.Value) ? Constants.True : left;
            }

            return Expression.OrElse(left, right);
        }

        public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
        {
            var elementType = loopVar.Type;
            var enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(elementType);

            var enumeratorVar = Expression.Variable(enumeratorType, "enumerator");
            var getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod("GetEnumerator"));
            var enumeratorAssign = Expression.Assign(enumeratorVar, getEnumeratorCall);

            // The MoveNext method's actually on IEnumerator, not IEnumerator<T>
            var moveNextCall = Expression.Call(enumeratorVar, typeof(IEnumerator).GetMethod("MoveNext"));

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { enumeratorVar },
                enumeratorAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block(new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                            loopContent
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }

        public static Expression For(ParameterExpression loopVar, Expression initValue, Expression condition, Expression increment, Expression loopContent)
        {
            var initAssign = Expression.Assign(loopVar, initValue);

            var breakLabel = Expression.Label("LoopBreak");

            var loop = Expression.Block(new[] { loopVar },
                initAssign,
                Expression.Loop(
                    Expression.IfThenElse(
                        condition,
                        Expression.Block(
                            loopContent,
                            increment
                        ),
                        Expression.Break(breakLabel)
                    ),
                breakLabel)
            );

            return loop;
        }
    }
}
