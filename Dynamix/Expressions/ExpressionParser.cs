using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Reflection;
using Dynamix.Expressions.Extensions;

namespace Dynamix.Expressions
{
    public class ObjectInitLambda
    {
        public LambdaExpression Expression { get; private set; }
        public IReadOnlyList<ParameterExpression> Parameters { get; private set; }
        public IReadOnlyList<InitMemberAssignment> Members { get; private set; }

        public ObjectInitLambda(LambdaExpression expression, IEnumerable<InitMemberAssignment> members)
        {
            this.Expression = expression;
            Parameters = expression.Parameters.ToList();
            Members = members.ToList();
        }

    }

    public enum InitMemberAssignmentType
    {
        Expression,
        Property,
        Constant,
    }
    public class InitMemberAssignment
    {
        public PropertyInfo Property { get; set; }
        public object ConstantValue { get; set; }
        public object EffectiveValue { get; set; }
        public Expression ValueExpression { get; set; }
        public LambdaExpression ValueLambdaExpression { get; set; }
        Delegate compiledLambdaExpression = null;
        public Delegate CompiledLambdaExpression { get { if (compiledLambdaExpression == null) { if (ValueLambdaExpression != null) compiledLambdaExpression = ValueLambdaExpression.Compile(); } return compiledLambdaExpression; } }
        public InitMemberAssignmentType Type { get; set; }
        public PropertyInfo MappedProperty { get; set; }
    }

    public static class ExpressionParser
    {
        #region Init Epression
        private static List<InitMemberAssignment> ParseInitExpression<T>(MemberInitExpression memberInitExpression)
            where T : class
        {
            var r = new List<InitMemberAssignment>();

            foreach (MemberBinding binding in memberInitExpression.Bindings)
            {
                string propertyName = binding.Member.Name;

                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment == null)
                    throw new ArgumentException("The init expression MemberBinding must only by type MemberAssignment.", "memberInitExpression");

                var valueExpression = memberAssignment.Expression;
                var lambda = Expression.Lambda(valueExpression);

                var initMemberAssignment = new InitMemberAssignment
                {
                    Property = binding.Member as PropertyInfo,
                    ValueExpression = valueExpression,
                    ValueLambdaExpression = lambda,
                };

                if (valueExpression.NodeType == ExpressionType.Constant && valueExpression is ConstantExpression constantExpression)
                {
                    initMemberAssignment.Type = InitMemberAssignmentType.Constant;
                    initMemberAssignment.ConstantValue = constantExpression.Value;
                    initMemberAssignment.EffectiveValue = initMemberAssignment.ConstantValue;
                }
                else
                {
                    initMemberAssignment.EffectiveValue = initMemberAssignment.CompiledLambdaExpression.DynamicInvoke();
                }

                r.Add(initMemberAssignment);
            }

            return r;
        }

        private static List<InitMemberAssignment> ParseInitExpressionInternal<Tin, Tout>(Expression<Func<Tin, Tout>> initExpression, Tin input = null)
            where Tin : class where Tout : class
        {

            var memberInitExpression = GetBody(initExpression);
            var inputParameter = initExpression.Parameters.First();

            var r = new List<InitMemberAssignment>();

            foreach (MemberBinding binding in memberInitExpression.Bindings)
            {
                string propertyName = binding.Member.Name;

                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment == null)
                    throw new ArgumentException("The init expression MemberBinding must only by type MemberAssignment.", "memberInitExpression");

                var valueExpression = memberAssignment.Expression;
                var lambda = Expression.Lambda(valueExpression, inputParameter);

                var initMemberAssignment = new InitMemberAssignment
                {
                    Property = binding.Member as PropertyInfo,
                    ValueExpression = valueExpression,
                    ValueLambdaExpression = lambda,
                };

                if (valueExpression.NodeType == ExpressionType.Constant && valueExpression is ConstantExpression constantExpression)
                {
                    initMemberAssignment.Type = InitMemberAssignmentType.Constant;
                    initMemberAssignment.ConstantValue = constantExpression.Value;
                }
                else if (valueExpression.NodeType == ExpressionType.MemberAccess 
                    && valueExpression is MemberExpression memberExpression
                    && memberExpression.Expression == inputParameter)
                {
                    initMemberAssignment.MappedProperty = memberExpression.Member as PropertyInfo;
                    initMemberAssignment.Type = InitMemberAssignmentType.Property;
                }

                if (input != null && initMemberAssignment.Type != InitMemberAssignmentType.Constant)
                    initMemberAssignment.EffectiveValue = initMemberAssignment.CompiledLambdaExpression.DynamicInvoke(input);

                r.Add(initMemberAssignment);
            }

            return r;
        }


        private static MemberInitExpression GetBody(LambdaExpression initExpression)
        {
            var memberInitExpression = initExpression.Body as MemberInitExpression;
            if (memberInitExpression == null)
                throw new ArgumentException("The init expression must be of type MemberInitExpression.", "initExpression");

            return memberInitExpression;
        }

        //private static Expression GetRootExpression(Expression e)
        //{
        //    if (e.NodeType == ExpressionType.)
        //}

        public static ObjectInitLambda ParseInitExpression<T>(Expression<Func<T>> initExpression)
            where T : class
        {
            return new ObjectInitLambda(initExpression, ParseInitExpression<T>(GetBody(initExpression)));
        }

        //public static ObjectInitLambda ParseInitExpression<T>(Expression<Func<T, T>> initExpression)
        //    where T : class
        //{
        //    return new ObjectInitLambda(initExpression, ParseInitExpression<T,T>(GetBody(initExpression)));
        //}

        //public static ObjectInitLambda ParseInitExpression<Tin, Tout>(Expression<Func<Tin, Tout>> initExpression, Tin input = null)
        //    where Tin : class where Tout : class
        //{
        //    return new ObjectInitLambda(initExpression, ParseInitExpression<Tin, Tout>(initExpression, input));
        //}

        public static ObjectInitLambda ParseInitExpression<Tin, Tout>(Expression<Func<Tin, Tout>> initExpression, Tin input = null)
            where Tin : class where Tout : class
        {
            return new ObjectInitLambda(initExpression, ParseInitExpressionInternal<Tin, Tout>(initExpression, input));
        }

        #endregion

        #region New Expression (Anonymous type)
        public static ObjectInitLambda ParseNewExpression<Tin, Tout>(Expression<Func<Tin, Tout>> newExpression, Tin input = null)
            where Tin : class where Tout : class
        {
            var body = newExpression.Body as NewExpression;
            if (body == null)
                throw new ArgumentException("Expression must be a new anonymous initialiazer");

            var r = new List<InitMemberAssignment>();

            var i = 0;
            foreach (var member in body.Members)
            {
                var propertyName = member.Name;
                var property = member.DeclaringType.GetProperty(member.Name);

                var argumentExpression = body.Arguments[i++];
                var inputParameter = argumentExpression.GetFirstParameterOfType<Tin>();

                var lambda = Expression.Lambda(argumentExpression, newExpression.Parameters);

                var initMemberAssignment = new InitMemberAssignment
                {
                    Property = member as PropertyInfo,
                    ValueExpression = argumentExpression,
                    ValueLambdaExpression = lambda,
                };

                if (argumentExpression.NodeType == ExpressionType.Constant && argumentExpression is ConstantExpression constantExpression)
                {
                    initMemberAssignment.Type = InitMemberAssignmentType.Constant;
                    initMemberAssignment.ConstantValue = constantExpression.Value;
                }
                else if (argumentExpression.NodeType == ExpressionType.MemberAccess
                    && argumentExpression is MemberExpression memberExpression
                    && memberExpression.Expression == inputParameter)
                {
                    initMemberAssignment.MappedProperty = memberExpression.Member as PropertyInfo;
                    initMemberAssignment.Type = InitMemberAssignmentType.Property;
                }

                if (input != null && initMemberAssignment.Type != InitMemberAssignmentType.Constant)
                    initMemberAssignment.EffectiveValue = initMemberAssignment.CompiledLambdaExpression.DynamicInvoke(input);

                r.Add(initMemberAssignment);
            }

            return new ObjectInitLambda(newExpression, r);
        }



        #endregion
    }
}
