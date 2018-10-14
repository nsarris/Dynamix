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
    internal sealed class ExpressionHashCodeCalculationVisitor : EqualityExpressionVisitor
    {
        public int HashCode { get; private set; }

        public ExpressionHashCodeCalculationVisitor(Expression expression)
        {
            Visit(expression);
        }

        private void Add(int i)
        {
            HashCode *= 37;
            HashCode ^= i;
        }

        //Add the NodeType and return Type of each visited expression
        protected override void Visit(Expression expression)
        {

            if (expression != null)
            {
                Add((int)expression.NodeType);
                Add(expression.Type.GetHashCode());
            }
            base.Visit(expression);
        }

        //Add the count of each collection of items visited
        protected override void VisitList<T>(ReadOnlyCollection<T> list, Action<T> visitor)
        {
            Add(list.Count);

            base.VisitList(list, visitor);
        }

        //Add the type of a type cast expression
        protected override void TypeOperandReference(Type type)
        {
            Add(type.GetHashCode());
        }

        //Add a visited constructor
        protected override void CtorReference(ConstructorInfo constructor)
        {
            Add(constructor.GetHashCode());
        }

        //Add a visited method
        protected override void MethodReference(MethodInfo method, bool? isLifted = null, bool? isLiftedToNull = null)
        {
            Add(method.GetHashCode());
            if (isLifted == true) Add(1);
            if (isLiftedToNull == true) Add(1);
        }

        //Add a visited or accessed member
        protected override void MemberReference(MemberInfo member)
        {
            Add(member.GetHashCode());
        }

        //Add a visited constant
        protected override void ConstantReference(object constant)
        {
            Add(constant.GetHashCode());
        }

        //Add a visited parameter type
        protected override void ParameterReference(string name, Type type, bool isByRef)
        {
            Add(type.GetHashCode());
            if (isByRef) Add(1);
        }

        //Add the return type of a lambda
        protected override void LambdaReturnTypeReference(Type type)
        {
            Add(type.GetHashCode());
        }
    }
}
