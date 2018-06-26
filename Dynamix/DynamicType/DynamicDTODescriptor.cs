using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynamix
{
    public class DynamicDtoDescriptor
    {
        public class DynamicDtoPropertyConfig
        {
            public string PropertyName { get; internal set; }
            public LambdaExpression SourceExpression { get; internal set; }
            public string TargetProperty { get; internal set; }
            public Type TargetType { get; internal set; }
            public bool AsNullable { get; internal set; }
            public string PropertyPath { get; internal set; }
        }
        
        static Random random = new Random();
        readonly List<DynamicDtoPropertyConfig> props = new List<DynamicDtoPropertyConfig>();
        int unnamedpropcount = 1;

        public IReadOnlyCollection<DynamicDtoPropertyConfig> Properties
        {
            get
            {
                return props.AsReadOnly();
            }
        }

        public DynamicDtoDescriptor AddProperty(
            string PropertyName, Type PropertyType, string source
            , string TargetProperty = null, bool AsNullable = false)
        {
            if (string.IsNullOrEmpty(PropertyName))
                throw new ArgumentNullException(nameof(PropertyName));

            if (string.IsNullOrEmpty(TargetProperty))
                TargetProperty = PropertyName;

            props.Add(new DynamicDtoPropertyConfig()
            {
                PropertyName = PropertyName,
                PropertyPath = source,
                SourceExpression = null,
                TargetProperty = TargetProperty,
                TargetType = PropertyType,
                AsNullable = AsNullable
            });

            return this;
        }

        public DynamicDtoDescriptor AddProperty<TProp>(
            string PropertyName, string path
            , string TargetProperty = null)
        {
            return AddProperty(PropertyName, typeof(TProp), path, TargetProperty);
        }
        public DynamicDtoDescriptor AddProperty(
            LambdaExpression sourceExpression, string path, string TargetProperty = null,
            string PropertyName = null, Type OverrideType = null, bool AsNullable = false)
        {
            if (string.IsNullOrEmpty(PropertyName))
            {
                try
                {
                    var prop = ReflectionHelper.GetProperty(sourceExpression);
                    PropertyName = prop.Name;
                    OverrideType = prop.PropertyType;
                }
                catch
                {
                    if (string.IsNullOrEmpty(TargetProperty))
                        throw new ArgumentNullException("TargetProperty");
                    PropertyName = "Expr" + unnamedpropcount++;
                }
            }

            if (string.IsNullOrEmpty(TargetProperty))
                TargetProperty = PropertyName;
            props.Add(new DynamicDtoPropertyConfig()
            {
                PropertyName = PropertyName,
                PropertyPath = path,
                SourceExpression = sourceExpression,
                TargetProperty = TargetProperty,
                TargetType = OverrideType ?? sourceExpression.ReturnType,
                AsNullable = AsNullable
            });

            return this;
        }

        public DynamicDtoDescriptor AddProperty<T>(
            string PropertyName, string path = null, string TargetProperty = null,
            Type OverrideType = null, bool AsNullable = false)
        {
            return AddProperty(PropertyName, OverrideType ?? typeof(T), path, TargetProperty, AsNullable);
        }

        public DynamicDtoDescriptor AddProperty<T>(
            Expression<Func<T, object>> sourceExpression, string path = null, string TargetProperty = null
            , string PropertyName = null, Type OverrideType = null, bool AsNullable = false)
        {
            return AddProperty<T, object>(sourceExpression, path, TargetProperty, PropertyName, OverrideType, AsNullable);
        }

        public DynamicDtoDescriptor AddProperty<T, TProp>(
            Expression<Func<T, TProp>> sourceExpression, string path = null, string TargetProperty = null
            , string PropertyName = null, Type OverrideType = null, bool AsNullable = false)
        {
            return AddProperty((LambdaExpression)sourceExpression,
                path, TargetProperty, PropertyName,
                OverrideType ?? typeof(TProp), AsNullable);
        }

        public Type GetReturnType(string TypeName, Type BaseType = null)
        {
            Type returnType;

            var d = new DynamicTypeDescriptorBuilder()
                .HasName(TypeName)
                .HasBaseType(BaseType ?? typeof(DynamicType));

            foreach (var p in props)
                d.AddProperty(p.TargetProperty, p.TargetType, prop => prop.AsNullable(p.AsNullable));

            returnType = DynamicTypeBuilder.Instance.CreateType(d);

            return returnType;
        }

        public LambdaExpression CreateSelector(Type ModelType, Type returnType = null, IEnumerable<string> includedProperties = null)
        {
            var inParam = Expression.Parameter(ModelType, "model");
            if (returnType == null)
                returnType = GetReturnType("tmpDynamicDTO_" + random.Next().ToString());
            
            var memberAssignments = new List<MemberAssignment>();
            foreach (var p in (includedProperties == null ? props : props.Where(x => includedProperties.Any(ip => StringComparer.OrdinalIgnoreCase.Compare(ip, x.TargetProperty ?? x.PropertyName) == 0))))
            {
                Expression memberExpression;

                var exp = p.SourceExpression;
                if (exp == null)
                    memberExpression = Expression.Property(CreateMemberExpression(inParam, p.PropertyPath), p.PropertyName);
                else
                {
                    var memberlambda = (LambdaExpression)new SelectorToExpressionVisitor().Modify(
                        exp,
                        exp.Parameters[0],
                        CreateMemberExpression(inParam, p.PropertyPath));

                    memberExpression = memberlambda.Body;
                }
                var targetProperty = returnType.GetProperty(p.TargetProperty);

                memberAssignments.Add(
                Expression.Bind(
                    targetProperty,
                    ExpressionEx.ConvertIfNeeded(memberExpression, targetProperty.PropertyType)
                ));
            }

            var init = Expression.MemberInit(Expression.New(returnType), memberAssignments);
            return Expression.Lambda(init, inParam);
        }

        protected static Expression CreateMemberLambdaExpression(Expression inParam, string propertyFullPath)
        {
            Expression body = inParam;
            foreach (var member in propertyFullPath.Split('.'))
                body = Expression.PropertyOrField(body, member);
            return body;
        }

        protected static Expression CreateMemberExpression(Expression body, string propertyFullPath)
        {
            if (string.IsNullOrEmpty(propertyFullPath))
                return body;

            foreach (var member in propertyFullPath.Split('.'))
                body = Expression.PropertyOrField(body, member);
            return body;
        }

        public class SelectorToExpressionVisitor : ExpressionVisitor
        {
            ParameterExpression replace;
            Expression replaceWith;
            bool first = true;
            public Expression Modify(Expression expression,
                ParameterExpression replace,
                Expression replaceWith)
            {
                first = true;
                this.replace = replace;
                this.replaceWith = replaceWith;
                return Visit(expression);
            }

            public override Expression Visit(Expression node)
            {
                if (node == replace)
                {
                    return base.Visit(replaceWith);
                }
                else
                    return base.Visit(node);
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                if (first)
                {
                    first = false;
                    return Expression.Lambda(Visit(node.Body), Expression.Parameter(replaceWith.Type));
                }
                else
                    return base.VisitLambda<T>(node);


            }
        }
    }

    

}
