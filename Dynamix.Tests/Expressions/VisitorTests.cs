using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dynamix.Expressions.Extensions;
using Dynamix.Expressions;

namespace Dynamix.Tests.Expressions
{
    [TestFixture]
    public class VisitorTests
    {
        public class A
        {
            public int a;
            public int b;
        }

        public class B : A { }
        [Test]
        public void TestParameterReplacer()
        {
            Expression<Func<int, int, int>> expression =
                (x , y) => x + y;

            var newParam = Expression.Parameter(typeof(int));

            //Replace Parameters

            //Typed Lambda
            var case1 = ExpressionParameterReplacer.Replace(expression, expression.Parameters[0], newParam);
            Assert.AreEqual(newParam, case1.Parameters[0]);
            Assert.AreEqual(((BinaryExpression)case1.Body).Left, newParam);

            //Untyped Lambda
            var case2 = ExpressionParameterReplacer.Replace((LambdaExpression)expression, expression.Parameters[0], newParam);
            Assert.AreEqual(newParam, case2.Parameters[0]);
            Assert.AreEqual(((BinaryExpression)case2.Body).Left, newParam);

            //Lambda as Expression
            var case3 = ExpressionParameterReplacer.Replace((Expression)expression, expression.Parameters[0], newParam);
            Assert.AreEqual(newParam, ((LambdaExpression)case3).Parameters[0]);
            Assert.AreEqual(((BinaryExpression)((LambdaExpression)case3).Body).Left, newParam);

            //Expression (no lambda)
            var case4 = ExpressionParameterReplacer.Replace(expression.Body, expression.Parameters[0], newParam);
            Assert.AreEqual(((BinaryExpression)case4).Left, newParam);

            //Replace Parameters by name

            //Typed Lambda
            var case11 = ExpressionParameterReplacer.Replace(expression, "x", newParam);
            Assert.AreEqual(newParam, case11.Parameters[0]);
            Assert.AreEqual(((BinaryExpression)case11.Body).Left, newParam);

            //Untyped Lambda
            var case12 = ExpressionParameterReplacer.Replace((LambdaExpression)expression, "x", newParam);
            Assert.AreEqual(newParam, case12.Parameters[0]);
            Assert.AreEqual(((BinaryExpression)case12.Body).Left, newParam);

            //Lambda as Expression
            var case13 = ExpressionParameterReplacer.Replace((Expression)expression, "x", newParam);
            Assert.AreEqual(newParam, ((LambdaExpression)case13).Parameters[0]);
            Assert.AreEqual(((BinaryExpression)((LambdaExpression)case13).Body).Left, newParam);

            //Expression (no lambda)
            var case14 = ExpressionParameterReplacer.Replace(expression.Body, "x", newParam);
            Assert.AreEqual(((BinaryExpression)case14).Left, newParam);

            //Replace Parameters by type, throws error because to ints

            //Untyped Lambda
            Assert.Throws<InvalidOperationException>(() => ExpressionParameterReplacer.Replace((LambdaExpression)expression, typeof(int), newParam));

            //Replace Parameters by type

            Expression<Func<int, int>> simpleExpression =
                x => x;

            //Typed Lambda
            var case21 = ExpressionParameterReplacer.Replace(simpleExpression, typeof(int), newParam);
            Assert.AreEqual(newParam, case21.Parameters[0]);
            Assert.AreEqual(case21.Body, newParam);

            //Untyped Lambda
            var case22 = ExpressionParameterReplacer.Replace((LambdaExpression)simpleExpression, typeof(int), newParam);
            Assert.AreEqual(newParam, case22.Parameters[0]);
            Assert.AreEqual(case22.Body, newParam);

            //Lambda as Expression
            var case23 = ExpressionParameterReplacer.Replace((Expression)simpleExpression, typeof(int), newParam);
            Assert.AreEqual(newParam, ((LambdaExpression)case23).Parameters[0]);
            Assert.AreEqual(((LambdaExpression)case23).Body, newParam);


            //Expression (no lambda)
            var case24 = ExpressionParameterReplacer.Replace(expression.Body, typeof(int), newParam);
            Assert.AreEqual(((BinaryExpression)case24).Left, newParam);

            //Change Type by type

            Expression<Func<A, A, bool>> expression2 =
                (x, y) => x.a > 0 && y.b > 0;

            //Untyped Lambda
            var case31 = ExpressionParameterReplacer.ChangeType((LambdaExpression)expression2, typeof(A), typeof(B));
            Assert.AreEqual(case31.Parameters[0].Type, typeof(B));
            
            //Lambda as Expression
            var case32 = ExpressionParameterReplacer.ChangeType((Expression)expression2, typeof(A), typeof(B));
            Assert.IsTrue(case32.GetParameters(x => x.Type == typeof(B)).Any());

            //Expression
            var case32a = ExpressionParameterReplacer.ChangeType(expression2.Body, typeof(A), typeof(B));
            Assert.IsTrue(case32a.GetParameters(x => x.Type == typeof(B)).Any());

            //Change Type by name

            //Untyped Lambda
            var case33 = ExpressionParameterReplacer.ChangeType((LambdaExpression)expression2, "x", typeof(B));
            Assert.AreEqual(case33.Parameters[0].Type, typeof(B));

            //Lambda as Expression
            var case34 = ExpressionParameterReplacer.ChangeType((Expression)expression2, "x", typeof(B));
            Assert.IsTrue(case34.GetParameters(x => x.Type == typeof(B)).Any());

            //Expression
            var case35 = ExpressionParameterReplacer.ChangeType(expression2.Body, "x", typeof(B));
            Assert.IsTrue(case35.GetParameters(x => x.Type == typeof(B)).Any());

        }
    }
}
