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
    internal class EqualityExpressionVisitor
    {
        protected virtual void Visit(Expression expression)
        {
            if (expression == null)
                return;

            switch (expression.NodeType)
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
                    VisitUnary((UnaryExpression)expression);
                    break;
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
                    VisitBinary((BinaryExpression)expression);
                    break;
                case ExpressionType.TypeIs:
                    VisitTypeBinary((TypeBinaryExpression)expression);
                    break;
                case ExpressionType.Conditional:
                    VisitConditional((ConditionalExpression)expression);
                    break;
                case ExpressionType.Constant:
                    VisitConstant((ConstantExpression)expression);
                    break;
                case ExpressionType.Parameter:
                    VisitParameter((ParameterExpression)expression);
                    break;
                case ExpressionType.MemberAccess:
                    VisitMember((MemberExpression)expression);
                    break;
                case ExpressionType.Call:
                    VisitMethodCall((MethodCallExpression)expression);
                    break;
                case ExpressionType.Lambda:
                    VisitLambda((LambdaExpression)expression);
                    break;
                case ExpressionType.New:
                    VisitNew((NewExpression)expression);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    VisitNewArray((NewArrayExpression)expression);
                    break;
                case ExpressionType.Invoke:
                    VisitInvocation((InvocationExpression)expression);
                    break;
                case ExpressionType.MemberInit:
                    VisitMemberInit((MemberInitExpression)expression);
                    break;
                case ExpressionType.ListInit:
                    VisitListInit((ListInitExpression)expression);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unhandled expression type: '{0}'", expression.NodeType));
            }
        }

        protected virtual void VisitBinding(MemberBinding node)
        {
            switch (node.BindingType)
            {
                case MemberBindingType.Assignment:
                    VisitMemberAssignment((MemberAssignment)node);
                    break;
                case MemberBindingType.MemberBinding:
                    VisitMemberMemberBinding((MemberMemberBinding)node);
                    break;
                case MemberBindingType.ListBinding:
                    VisitMemberListBinding((MemberListBinding)node);
                    break;
                default:
                    throw new ArgumentException(string.Format("Unhandled binding type '{0}'", node.BindingType));
            }
        }

        protected virtual void VisitList<T>(ReadOnlyCollection<T> list, Action<T> visitor)
        {
            foreach (T element in list)
                visitor(element);
        }

        protected virtual void VisitExpressionList(ReadOnlyCollection<Expression> list)
        {
            VisitList(list, Visit);
        }

        protected virtual void VisitBindingList(ReadOnlyCollection<MemberBinding> list)
        {
            VisitList(list, VisitBinding);
        }

        protected virtual void VisitElementInitializerList(ReadOnlyCollection<ElementInit> list)
        {
            VisitList(list, VisitElementInitializer);
        }




        protected virtual void VisitElementInitializer(ElementInit node)
        {
            MethodReference(node.AddMethod);

            VisitExpressionList(node.Arguments);
        }

        protected virtual void VisitMemberMemberBinding(MemberMemberBinding node)
        {
            MemberReference(node.Member);

            VisitBindingList(node.Bindings);
        }

        protected virtual void VisitMemberListBinding(MemberListBinding node)
        {
            MemberReference(node.Member);

            VisitElementInitializerList(node.Initializers);
        }


        protected virtual void VisitLambda(LambdaExpression node)
        {
            VisitList(node.Parameters, p => ParameterReference(p.Name,p.Type,p.IsByRef));

            Visit(node.Body);
        }



        protected virtual void VisitUnary(UnaryExpression node)
        {
            if (node.Method != null)
                MethodReference(node.Method, node.IsLifted, node.IsLiftedToNull);

            Visit(node.Operand);
        }

        protected virtual void VisitBinary(BinaryExpression node)
        {
            if (node.Method != null)
                MethodReference(node.Method, node.IsLifted, node.IsLiftedToNull);

            Visit(node.Left);
            Visit(node.Right);
            Visit(node.Conversion);
        }

        protected virtual void VisitTypeBinary(TypeBinaryExpression node)
        {
            TypeOperandReference(node.TypeOperand);

            Visit(node.Expression);
        }

        protected virtual void VisitConstant(ConstantExpression node)
        {
            ConstantReference(node?.Value);
        }

        protected virtual void VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);
            Visit(node.IfTrue);
            Visit(node.IfFalse);
        }

        protected virtual void VisitParameter(ParameterExpression node)
        {
            ParameterReference(node.Name, node.Type, node.IsByRef);
        }

        protected virtual void VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            MemberReference(memberAssignment.Member);
            
            Visit(memberAssignment.Expression);
        }

        protected virtual void VisitMember(MemberExpression node)
        {
            MemberReference(node.Member);

            Visit(node.Expression);
        }

        protected virtual void VisitMethodCall(MethodCallExpression node)
        {
            MethodReference(node.Method);

            Visit(node.Object);
            VisitExpressionList(node.Arguments);
        }

        protected virtual void VisitNew(NewExpression node)
        {
            CtorReference(node.Constructor);

            if (node.Members != null)
                VisitList(node.Members, MemberReference);

            VisitExpressionList(node.Arguments);
        }

        protected virtual void VisitMemberInit(MemberInitExpression node)
        {
            VisitNew(node.NewExpression);
            VisitBindingList(node.Bindings);
        }

        protected virtual void VisitListInit(ListInitExpression node)
        {
            VisitNew(node.NewExpression);
            VisitElementInitializerList(node.Initializers);
        }

        protected virtual void VisitNewArray(NewArrayExpression node)
        {
            VisitExpressionList(node.Expressions);
        }

        protected virtual void VisitInvocation(InvocationExpression node)
        {
            VisitExpressionList(node.Arguments);
            Visit(node.Expression);
        }

        protected virtual void LambdaReturnTypeReference(Type type)
        {

        }

        protected virtual void TypeOperandReference(Type type)
        {

        }

        protected virtual void CtorReference(ConstructorInfo constructor)
        {

        }

        protected virtual void MethodReference(MethodInfo method, bool? isLifted = null, bool? isLiftedToNull = null)
        {

        }

        protected virtual void MemberReference(MemberInfo member)
        {

        }

        protected virtual void ConstantReference(object constant)
        {

        }

        protected virtual void ParameterReference(string name, Type type, bool isByRef)
        {

        }
    }
}
