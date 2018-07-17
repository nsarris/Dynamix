using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix.Expressions
{
    internal class ExpressionComparisonVisitor
    {
        public bool AreEqual { get; }

        public ExpressionComparisonVisitor(Expression a, Expression b)
        {
            AreEqual = Visit(a, b);
        }


        private bool Visit(Expression a, Expression b)
        {
            if (a == null && b == null)
                return true;

            if ((a == null || b == null)
                || a.NodeType != b.NodeType
                || a.Type != b.Type)
            {
                return false;
            }

            switch (a.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                    return VisitUnary((UnaryExpression)a, (UnaryExpression)b);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Power:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)a, (BinaryExpression)b);
                case ExpressionType.TypeIs:
                    return VisitTypeBinary((TypeBinaryExpression)a, (TypeBinaryExpression)b);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)a, (ConditionalExpression)b);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)a, (ConstantExpression)b);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)a, (ParameterExpression)b);
                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)a, (MemberExpression)b);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)a, (MethodCallExpression)b);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)a, (LambdaExpression)b);
                case ExpressionType.New:
                    return VisitNew((NewExpression)a, (NewExpression)b);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)a, (NewArrayExpression)b);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)a, (InvocationExpression)b);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)a, (MemberInitExpression)b);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)a, (ListInitExpression)b);
                default:
                    throw new ArgumentException(string.Format("Unhandled expression type: '{0}'", a.NodeType));
            }
        }





        protected virtual bool VisitBinding(MemberBinding a, MemberBinding b)
        {
            if (a.BindingType != b.BindingType)
                return false;

            switch (a.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)a, (MemberAssignment)b);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)a, (MemberMemberBinding)b);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)a, (MemberListBinding)b);
                default:
                    throw new ArgumentException(string.Format("Unhandled binding type '{0}'", a.BindingType));
            }
        }

        protected virtual bool VisitList<T>(ReadOnlyCollection<T> a, ReadOnlyCollection<T> b, Func<T, T, bool> visitor)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            if (a.Count != b.Count)
                return false;

            foreach (var res in a.Zip(b, visitor))
                if (!res) return false;

            return true;
        }

        protected virtual bool VisitExpressionList(ReadOnlyCollection<Expression> a, ReadOnlyCollection<Expression> b)
        {
            return VisitList(a,b, Visit);
        }

        protected virtual bool VisitBindingList(ReadOnlyCollection<MemberBinding> a, ReadOnlyCollection<MemberBinding> b)
        {
            return VisitList(a,b, VisitBinding);
        }

        protected virtual bool VisitElementInitializerList(ReadOnlyCollection<ElementInit> a, ReadOnlyCollection<ElementInit> b)
        {
            return VisitList(a,b, VisitElementInitializer);
        }

        protected virtual bool VisitMemberAssignment(MemberAssignment a, MemberAssignment b)
        {
            return CheckMember(a.Member,b.Member)
                && Visit(a.Expression, b.Expression);
        }


        protected virtual bool VisitElementInitializer(ElementInit a, ElementInit b)
        {
            return CheckMethod(a.AddMethod,b.AddMethod)
                && VisitExpressionList(a.Arguments, b.Arguments);
        }

        protected virtual bool VisitMemberMemberBinding(MemberMemberBinding a, MemberMemberBinding b)
        {
            return CheckMember(a.Member, b.Member) 
                && VisitBindingList(a.Bindings, b.Bindings);
        }

        protected virtual bool VisitMemberListBinding(MemberListBinding a, MemberListBinding b)
        {
            return CheckMember(a.Member, b.Member)
                && VisitElementInitializerList(a.Initializers, b.Initializers);
        }


        private bool VisitLambda(LambdaExpression a, LambdaExpression b)
        {
            return 
                a.ReturnType == b.ReturnType
                && VisitList(a.Parameters, b.Parameters, (p1, p2)
                    => CheckParameter(p1.Name, p1.Type, p1.IsByRef, p2.Name, p2.Type, p2.IsByRef))
                && Visit(a.Body, b.Body);
        }

        private bool VisitUnary(UnaryExpression a, UnaryExpression b)
        {
            return CheckMethod(a.Method, b.Method, a.IsLifted, a.IsLiftedToNull,b.IsLifted, b.IsLiftedToNull)
                && Visit(a.Operand, b.Operand);
        }

        private bool VisitBinary(BinaryExpression a, BinaryExpression b)
        {
            return CheckMethod(a.Method, b.Method, a.IsLifted, a.IsLiftedToNull, b.IsLifted, b.IsLiftedToNull)
                && Visit(a.Left, b.Left)
                && Visit(a.Right, b.Right)
                && Visit(a.Conversion, b.Conversion);
        }


        private bool VisitTypeBinary(TypeBinaryExpression a, TypeBinaryExpression b)
        {
            return CheckTypeOperand(a.TypeOperand, b.TypeOperand)
                && Visit(a.Expression, b.Expression);
        }


        private bool VisitConstant(ConstantExpression a, ConstantExpression b)
        {
            return CheckConstant(a?.Value, b?.Value);
        }

        private bool VisitConditional(ConditionalExpression a, ConditionalExpression b)
        {
            return Visit(a.Test, b.Test)
                && Visit(a.IfTrue, b.IfTrue)
                && Visit(a.IfFalse, b.IfFalse);
        }


        private bool VisitParameter(ParameterExpression a, ParameterExpression b)
        {
            return CheckParameter(a.Name,a.Type, a.IsByRef, b.Name,b.Type,b.IsByRef);
        }

        private bool VisitMember(MemberExpression a, MemberExpression b)
        {
            return CheckMember(a.Member, b.Member)
                && Visit(a.Expression, b.Expression);
        }

        private bool VisitMethodCall(MethodCallExpression a, MethodCallExpression b)
        {
            return CheckMethod(a.Method,b.Method)
                && Visit(a.Object, b.Object)
                && VisitExpressionList(a.Arguments, b.Arguments);
        }

        private bool VisitNew(NewExpression a, NewExpression b)
        {
            return CheckConstructor(a.Constructor, b.Constructor)
                && VisitList(a.Members, b.Members, CheckMember)
                && VisitExpressionList(a.Arguments, b.Arguments);
        }

        private bool VisitMemberInit(MemberInitExpression a, MemberInitExpression b)
        {
            return VisitNew(a.NewExpression, b.NewExpression)
                && VisitBindingList(a.Bindings, b.Bindings);
        }

        private bool VisitListInit(ListInitExpression a, ListInitExpression b)
        {
            return VisitNew(a.NewExpression, b.NewExpression)
                && VisitElementInitializerList(a.Initializers, b.Initializers);
        }



        private bool VisitInvocation(InvocationExpression a, InvocationExpression b)
        {
            return VisitExpressionList(a.Arguments, b.Arguments)
                && Visit(a.Expression, b.Expression);
        }

        private bool VisitNewArray(NewArrayExpression a, NewArrayExpression b)
        {
            return VisitExpressionList(a.Expressions, b.Expressions);
        }




        protected virtual bool CheckTypeOperand(Type a, Type b)
        {
            return CheckEqual(a, b);
        }

        protected virtual bool CheckConstructor(ConstructorInfo a, ConstructorInfo b)
        {
            return CheckEqual(a, b);
        }

        protected virtual bool CheckMethod(
            MethodInfo a, MethodInfo b,
            bool? aIsLifted = null, bool? aIsLiftedToNull = null,
            bool? bIsLifted = null, bool? bIsLiftedToNull = null)
        {
            return CheckEqual(a, b, CheckEqualsMethod.ObjectEquals, true)
                && CheckEqual(aIsLifted, bIsLifted)
                && CheckEqual(aIsLiftedToNull, bIsLiftedToNull);
        }

        protected virtual bool CheckMember(MemberInfo a, MemberInfo b)
        {
            return CheckEqual(a, b);
        }

        protected virtual bool CheckConstant(object a, object b)
        {
            return CheckEqual(a,b, CheckEqualsMethod.DefaultEqualityComparer, true);
        }

        protected virtual bool CheckParameter(
            string aName, Type aType, bool aIsByRef,
            string bName, Type bType, bool bIsByRef)
        {
            return CheckEqual(aType, bType);
        }


        private enum CheckEqualsMethod
        {
            ObjectEquals,
            ReferenceEquals,
            DefaultEqualityComparer
        }

        private bool CheckEqual<T>(T t, T candidate, CheckEqualsMethod method = CheckEqualsMethod.ObjectEquals, bool checkNull = false)
        {
            if (checkNull)
            {
                if (t == null && candidate == null)
                    return true;

                if (t == null || candidate == null)
                    return false;
            }

            var result =
                method == CheckEqualsMethod.DefaultEqualityComparer ?
                    EqualityComparer<T>.Default.Equals(t, candidate) :
                method == CheckEqualsMethod.ObjectEquals ?
                    object.Equals(t, candidate) :
                    object.ReferenceEquals(t, candidate);

            if (result)
                return true;
            else
                return false;
        }
    }
}
