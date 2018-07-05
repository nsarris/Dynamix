using Dynamix.Expressions.Extensions;
using Dynamix.Expressions.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    public static class ExpressionParameterReplacer
    {
        #region Replace a parameter with another parameter

        public static Expression<T> Replace<T>
                     (Expression<T> expression,
                      ParameterExpression source,
                      ParameterExpression target)
        {
            return new ParameterReplacerVisitor(source, target)
                      .Visit(expression);
        }

        public static LambdaExpression Replace
             (LambdaExpression expression,
              ParameterExpression source,
              ParameterExpression target)
        {
            return new ParameterReplacerVisitor(source, target)
                      .Visit(expression);
        }

        public static Expression Replace
                     (Expression expression,
                      ParameterExpression source,
                      ParameterExpression target)
        {
            return new ParameterReplacerVisitor(source, target)
                      .Visit(expression);
        }

        #endregion

        #region Replace a parameter of a specific type with another parameter

        public static Expression<T> Replace<T>
             (Expression<T> expression,
              Type sourceType,
              ParameterExpression parameterExpression)
        {
            return new ParameterReplacerOfTypeVisitor(sourceType, parameterExpression)
                .Visit(expression);
        }

        public static LambdaExpression Replace
             (LambdaExpression expression,
              Type sourceType,
              ParameterExpression parameterExpression)
        {
            return new ParameterReplacerOfTypeVisitor(sourceType, parameterExpression)
                .Visit(expression);
        }

        public static Expression Replace
             (Expression expression,
              Type sourceType,
              ParameterExpression parameterExpression)
        {
            return new ParameterReplacerOfTypeVisitor(sourceType, parameterExpression)
                .Visit(expression);
        }

        #endregion

        #region Replace a parameter of a specific name with another parameter

        public static Expression<T> Replace<T>
            (Expression<T> expression,
            string parameterName,
            ParameterExpression target)
        {
            return new ParameterReplacerOfNameVisitor(parameterName, target)
                      .Visit(expression);
        }

        public static LambdaExpression Replace
             (LambdaExpression expression,
              string parameterName,
              ParameterExpression target)
        {
            return new ParameterReplacerOfNameVisitor(parameterName, target)
                      .Visit(expression);
        }

        public static Expression Replace
             (Expression expression,
              string parameterName,
              ParameterExpression target)
        {
            return new ParameterReplacerOfNameVisitor(parameterName, target)
                      .Visit(expression);
        }

        #endregion

        #region Replace all parameters of a specific type with new parameters of a different type

        public static LambdaExpression ChangeType
             (LambdaExpression expression,
              Type sourceType,
              Type targetType)
        {
            return expression
                .Parameters
                .Where(x => x.Type == sourceType)
                .Aggregate(
                    expression,
                    (aggregate, parameterToReplace) =>
                        new ParameterReplacerVisitor(parameterToReplace, Expression.Parameter(targetType, parameterToReplace.Name))
                                .Visit(aggregate));
        }

        public static Expression ChangeType
             (Expression expression,
              Type sourceType,
              Type targetType)
        {
            if (expression is LambdaExpression lambdaExpression)
                return ChangeType(lambdaExpression, sourceType, targetType);

            return expression
                .GetParametersOfType(sourceType)
                .Aggregate(
                    expression,
                    (aggregate, parameterToReplace) =>
                        new ParameterReplacerVisitor(parameterToReplace, Expression.Parameter(targetType, parameterToReplace.Name))
                                .Visit(aggregate));
        }

        #endregion

        #region Replace all parameters of a specific name with new parameters of a different type

        public static LambdaExpression ChangeType
             (LambdaExpression expression,
              string parameterName,
              Type targetType)
        {
            return expression
                .Parameters
                .Where(x => x.Name == parameterName)
                .Aggregate(
                    expression,
                    (aggregate, parameterToReplace) =>
                        new ParameterReplacerVisitor(parameterToReplace, Expression.Parameter(targetType, parameterToReplace.Name))
                                .Visit(aggregate));
        }

        public static Expression ChangeType
             (Expression expression,
              string parameterName,
              Type targetType)
        {
            if (expression is LambdaExpression lambdaExpression)
                return ChangeType(lambdaExpression, parameterName, targetType);

            return expression
                .GetParameters(x => x.Name == parameterName)
                .Aggregate(
                    expression,
                    (aggregate, parameterToReplace) =>
                        new ParameterReplacerVisitor(parameterToReplace, Expression.Parameter(targetType, parameterToReplace.Name))
                                .Visit(aggregate));
        }

        #endregion
    }
}
