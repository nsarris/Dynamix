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
        public IEnumerable<ParameterExpression> Parameters { get; private set; }
        public IEnumerable<InitMemberAssignment> Members { get; private set; }

        public ObjectInitLambda(LambdaExpression expression, IEnumerable<InitMemberAssignment> members)
        {
            this.Expression = expression;
            Parameters = expression.Parameters.ToList();
            Members = members.ToList();
        }

    }

    public enum InitMemberAssignmentType
    {
        Constant,
        Expression
    }
    public class InitMemberAssignment
    {
        public PropertyInfo Property { get; set; }
        public object ConstantValue { get; set; }
        public Expression ExpressionValue { get; set; }
        public InitMemberAssignmentType Type { get; set; }

        //public Expression
    }

    public static class ExpressionParser
    {
        #region Init Epression
        private static List<InitMemberAssignment> ParseInitExpression<T>(MemberInitExpression memberInitExpression)
        {
            var r = new List<InitMemberAssignment>();

            foreach (MemberBinding binding in memberInitExpression.Bindings)
            {

                string propertyName = binding.Member.Name;

                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment == null)
                    throw new ArgumentException("The update expression MemberBinding must only by type MemberAssignment.", "updateExpression");

                Expression memberExpression = memberAssignment.Expression;

                ParameterExpression parameterExpression = null;

                //ExpressionForEachOfTypeVisitor<ParameterExpression>.Visit(memberExpression,
                //memberExpression.VisitOfType<ParameterExpression>(
                //    (ParameterExpression p) =>
                //    {
                //        if (p.Type == typeof(T))
                //            parameterExpression = p;

                //        return p;
                //    });
                parameterExpression = memberExpression.GetFirstParameterOfType<T>();

                //Constant value
                if (parameterExpression == null)
                {
                    object value;

                    if (memberExpression.NodeType == ExpressionType.Constant)
                    {
                        var constantExpression = memberExpression as ConstantExpression;
                        if (constantExpression == null)
                            throw new ArgumentException(
                                "The MemberAssignment expression is not a ConstantExpression.", "updateExpression");

                        value = constantExpression.Value;
                    }
                    else
                    {
                        LambdaExpression lambda = Expression.Lambda(memberExpression, null);
                        value = lambda.Compile().DynamicInvoke();
                    }

                    r.Add(new InitMemberAssignment { Property = binding.Member as PropertyInfo, ConstantValue = value, Type = InitMemberAssignmentType.Constant });
                }
                //Expression
                else
                {
                    r.Add(new InitMemberAssignment { Property = binding.Member as PropertyInfo, ExpressionValue = memberExpression, Type = InitMemberAssignmentType.Expression });
                }
            }

            return r;
        }

        private static List<InitMemberAssignment> ParseInitExpression<Tin, Tout>(Tin input, MemberInitExpression memberInitExpression)
        {
            var r = new List<InitMemberAssignment>();

            foreach (MemberBinding binding in memberInitExpression.Bindings)
            {
                string propertyName = binding.Member.Name;

                var memberAssignment = binding as MemberAssignment;
                if (memberAssignment == null)
                    throw new ArgumentException("The update expression MemberBinding must only by type MemberAssignment.", "updateExpression");

                Expression memberExpression = memberAssignment.Expression;

                //ParameterExpression parameterExpression = null;

                //ExpressionForEachOfTypeVisitor<ParameterExpression>.Visit(memberExpression,
                //memberExpression.VisitOfType<ParameterExpression>(
                //    (ParameterExpression p) =>
                //    {
                //        if (p.Type == typeof(T))
                //            parameterExpression = p;

                //        return p;
                //    });
                var parameterExpressionOut = memberExpression.GetFirstParameterOfType<Tout>();
                var parameterExpressionIn = memberExpression.GetFirstParameterOfType<Tin>();

                //Constant value
                if (parameterExpressionOut == null)
                {
                    object value;

                    if (memberExpression.NodeType == ExpressionType.Constant)
                    {
                        var constantExpression = memberExpression as ConstantExpression;
                        if (constantExpression == null)
                            throw new ArgumentException(
                                "The MemberAssignment expression is not a ConstantExpression.", "updateExpression");

                        value = constantExpression.Value;
                    }
                    else
                    {
                        if (parameterExpressionIn != null)
                        {
                            var lambda = Expression.Lambda(memberExpression, parameterExpressionIn);
                            value = lambda.Compile().DynamicInvoke(input);
                        }
                        else
                        {
                            var lambda = Expression.Lambda(memberExpression);
                            value = lambda.Compile().DynamicInvoke();
                        }
                    }

                    r.Add(new InitMemberAssignment { Property = binding.Member as PropertyInfo, ConstantValue = value, Type = InitMemberAssignmentType.Constant });
                }
                //Expression
                else
                {
                    r.Add(new InitMemberAssignment { Property = binding.Member as PropertyInfo, ExpressionValue = memberExpression, Type = InitMemberAssignmentType.Expression });
                }
            }

            return r;
        }


        private static MemberInitExpression GetBody(LambdaExpression initExpression)
        {


            var memberInitExpression = initExpression.Body as MemberInitExpression;
            if (memberInitExpression == null)
                throw new ArgumentException("The update expression must be of type MemberInitExpression.", "updateExpression");

            return memberInitExpression;
        }

        public static ObjectInitLambda ParseInitExpression<T>(Expression<Func<T>> initExpression)
        {
            return new ObjectInitLambda(initExpression, ParseInitExpression<T>(GetBody(initExpression)));
        }

        public static ObjectInitLambda ParseInitExpression<T>(Expression<Func<T, T>> initExpression)
        {
            return new ObjectInitLambda(initExpression, ParseInitExpression<T>(GetBody(initExpression)));
        }

        public static ObjectInitLambda ParseInitExpression<Tin, Tout>(Tin input, Expression<Func<Tin, Tout>> initExpression)
        {
            return new ObjectInitLambda(initExpression, ParseInitExpression<Tin, Tout>(input, GetBody(initExpression)));
        }

        //public static ObjectInitLambda ParseInitExpression<Tin, Tout>(Tin input, Expression<Func<Tin, Tout, Tout>> initExpression)
        //{
        //    return new ObjectInitLambda(initExpression, ParseInitExpression<Tin, Tout>(input, GetBody(initExpression)));
        //}

        #endregion

        #region New Expression (Anonymous type)
        public static ObjectInitLambda ParseNewExpression<Tin, Tout>(Tin input, Expression<Func<Tin, Tout>> newExpression)
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
                var parameterExpressionIn = argumentExpression.GetFirstParameterOfType<Tin>();

                object value;

                if (argumentExpression.NodeType == ExpressionType.Constant)
                {
                    var constantExpression = argumentExpression as ConstantExpression;
                    if (constantExpression == null)
                        throw new ArgumentException(
                            "The Member Argument expression is not a ConstantExpression.", "updateExpression");

                    value = constantExpression.Value;
                }
                else
                {
                    if (parameterExpressionIn != null)
                    {
                        var lambda = Expression.Lambda(argumentExpression, parameterExpressionIn);
                        value = lambda.Compile().DynamicInvoke(input);
                    }
                    else
                    {
                        var lambda = Expression.Lambda(argumentExpression);
                        value = lambda.Compile().DynamicInvoke();
                    }
                }

                r.Add(new InitMemberAssignment { Property = member as PropertyInfo, ConstantValue = value, Type = InitMemberAssignmentType.Constant });
            }

            return new ObjectInitLambda(newExpression, r);
        }



        #endregion
    }
}
