using Dynamix;
using Dynamix.Expressions;
using Dynamix.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamix.DynamicProjection
{
    class DynamicDTOProperty
    {
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }
        public bool AsNullable { get; set; }
    }

    class UnmappedDynamicDTOProperty : DynamicDTOProperty
    {

    }

    class StaticValueDynamicDTOProperty : DynamicDTOProperty
    {
        public object Value { get; set; }
    }

    class DirectDynamicDTOProperty : DynamicDTOProperty
    {
        public string SourcePropertyPath { get; set; }
    }

    class ExpressionProperty : DynamicDTOProperty
    {
        public string SourceExpression { get; set; }
    }

    class KeyDescriptionProperty : DynamicDTOProperty
    {
        public string SourceValuePropertyPath { get; set; }
        public string SourceDescriptionPropertyPath { get; set; }

    }

    class MappedValueProperty : DynamicDTOProperty
    {
        public string SourcePropertyPath { get; set; }
        public Dictionary<object, object> MappedValues { get; set; }
    }

    class LookupValueProperty : DynamicDTOProperty
    {
        public string SourcePropertyPath { get; set; }
        public string LookupDictionaryName { get; set; }
    }

    class ConvertibleLazy<T> : Lazy<T>
    {
        public ConvertibleLazy()
        {
        }

        public ConvertibleLazy(Func<T> valueFactory) : base(valueFactory)
        {
        }

        public ConvertibleLazy(bool isThreadSafe) : base(isThreadSafe)
        {
        }

        public ConvertibleLazy(LazyThreadSafetyMode mode) : base(mode)
        {
        }

        public ConvertibleLazy(Func<T> valueFactory, bool isThreadSafe) : base(valueFactory, isThreadSafe)
        {
        }

        public ConvertibleLazy(Func<T> valueFactory, LazyThreadSafetyMode mode) : base(valueFactory, mode)
        {
        }

        public static implicit operator T(ConvertibleLazy<T> lazy)
        {
            return lazy.Value;
        }
    }

    public class DynamicProjectionProperty
    {
        public PropertyInfo TargetProperty { get; set; }
        public Expression SourceExpression { get; set; }
    }

    class DynamicProjectionDirectProperty : DynamicProjectionProperty
    {

    }

    class DynamicProjectionConstantProperty : DynamicProjectionProperty
    {
        public object ConstantValue { get; set; }
    }

    class DynamicProjectionKeyedDescriptionProperty : DynamicProjectionProperty
    {
        public Expression KeySourceExpression { get; set; }
        //public DynamicProjectionProperty KeyProperty { get; set; }
    }

    class DynamicProjectionMappedValueProperty : DynamicProjectionProperty
    {
        public Dictionary<object, object> MappedValues { get; set; }
    }

    class DynamicProjectionProperties
    {
        public ParameterExpression SourceParameter { get; set; }
        public IEnumerable<DynamicProjectionProperty> Properties { get; set; }
    }

    public class DynamicProjection
    {
        readonly ParameterExpression sourceParameter;
        readonly ConvertibleLazy<LambdaExpression> defaultSelector;
        public Type SourceType => sourceParameter.Type;
        public Type ProjectedType { get; }
        public IReadOnlyList<DynamicProjectionProperty> Properties { get; }

        

        internal DynamicProjection(Type projectedType, DynamicProjectionProperties properties)
        {
            ProjectedType = projectedType;
            Properties = properties.Properties.ToList();
            sourceParameter = properties.SourceParameter;

            defaultSelector = new ConvertibleLazy<LambdaExpression>(() => BuildSelector());
        }

        private LambdaExpression BuildSelector(IEnumerable<string> selectedProperties = null)
        {
            var memberAssignments =
                (selectedProperties == null ? Properties : Properties.Where(x => selectedProperties.Contains(x.TargetProperty.Name)))
                    .Select(p => Expression.Bind(
                            p.TargetProperty,
                            ExpressionEx.ConvertIfNeeded(p.SourceExpression, p.TargetProperty.PropertyType)));

            return Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(ProjectedType),
                        memberAssignments),
                    sourceParameter);
        }

        private LambdaExpression BuildPredicate()
        {
            return null;
        }

        public DynamicQueryable BuildQuery(
            IQueryable queryable, 
            IEnumerable<string> selectedColumns = null
            )
        {
            var d = new DynamicQueryable(queryable)
                .Select(selectedColumns == null ? defaultSelector : BuildSelector(selectedColumns));

            return d;
        }
    }

    public class DynamicProjectionBuilder
    {
        readonly Type sourceType;
        readonly string dtoTypeName;

        readonly List<DynamicDTOProperty> properties = new List<DynamicDTOProperty>();


        public DynamicProjectionBuilder(Type sourceType, string projectedTypeName)
        {
            this.sourceType = sourceType;
            this.dtoTypeName = projectedTypeName;
        }

        public DynamicProjectionBuilder AddUnmappedProperty(
            string propertyName, Type propertyType,
            bool asNullable = false)
        {
            properties.Add(new UnmappedDynamicDTOProperty()
            {
                PropertyName = propertyName,
                PropertyType = propertyType,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddUnmappedProperty<T>(
            string propertyName)
        {
            properties.Add(new UnmappedDynamicDTOProperty()
            {
                PropertyName = propertyName,
                PropertyType = typeof(T),
            });

            return this;
        }

        public DynamicProjectionBuilder AddStaticValueProperty(
            string propertyName, Type propertyType,
            object value, bool asNullable = false)
        {
            properties.Add(new StaticValueDynamicDTOProperty()
            {
                PropertyName = propertyName,
                PropertyType = propertyType,
                Value = value,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddStaticValueProperty<T>(
            string propertyName, T value)
        {
            properties.Add(new StaticValueDynamicDTOProperty()
            {
                PropertyName = propertyName,
                PropertyType = typeof(T),
                Value = value,
            });

            return this;
        }

        public DynamicProjectionBuilder AddMappedProperty(
            string targetPropertyName, Type targetPropertyType,
            string sourcePropertyPath,
            bool asNullable = false)
        {
            properties.Add(new DirectDynamicDTOProperty()
            {
                PropertyName = targetPropertyName,
                PropertyType = targetPropertyType,
                SourcePropertyPath = sourcePropertyPath,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddMappedProperty(
            string targetPropertyName,
            string sourcePropertyPath,
            bool asNullable = false)
        {
            properties.Add(new DirectDynamicDTOProperty()
            {
                PropertyName = targetPropertyName,
                //recursive?
                PropertyType = sourceType.GetProperty(sourcePropertyPath).PropertyType,
                SourcePropertyPath = sourcePropertyPath,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddMappedProperty(
            string sourcePropertyPath,
            bool asNullable = false)
        {
            properties.Add(new DirectDynamicDTOProperty()
            {
                PropertyName = sourcePropertyPath,
                //recursive?
                PropertyType = sourceType.GetProperty(sourcePropertyPath).PropertyType,
                SourcePropertyPath = sourcePropertyPath,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddMappedProperty<T>(
            string sourcePropertyPath)
        {
            properties.Add(new DirectDynamicDTOProperty()
            {
                PropertyName = sourcePropertyPath,
                PropertyType = typeof(T),
                SourcePropertyPath = sourcePropertyPath,
            });

            return this;
        }

        public DynamicProjectionBuilder AddMappedProperty<T>(
            string targetPropertyName,
            string sourcePropertyPath)
        {
            properties.Add(new DirectDynamicDTOProperty()
            {
                PropertyName = targetPropertyName,
                PropertyType = typeof(T),
                SourcePropertyPath = sourcePropertyPath,
            });

            return this;
        }



        public DynamicProjectionBuilder AddΕxpressionProperty(
            string targetPropertyName, Type targetPropertyType,
            string sourceExpresion,
            bool asNullable = false)
        {
            properties.Add(new ExpressionProperty()
            {
                PropertyName = targetPropertyName,
                PropertyType = targetPropertyType,
                SourceExpression = sourceExpresion,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddΕxpressionProperty<T>(
            string targetPropertyName,
            string sourceExpresion)
        {
            properties.Add(new ExpressionProperty()
            {
                PropertyName = targetPropertyName,
                PropertyType = typeof(T),
                SourceExpression = sourceExpresion,
            });

            return this;
        }

        public DynamicProjectionBuilder AddKeyedDescriptionProperty(
            string targetPropertyName, Type targetPropertyType,
            string sourceValuePropertyPath, string sourceDescriptionPropertyPath,
            bool asNullable = false)
        {
            properties.Add(new KeyDescriptionProperty()
            {
                PropertyName = targetPropertyName,
                PropertyType = targetPropertyType,
                SourceValuePropertyPath = sourceValuePropertyPath,
                SourceDescriptionPropertyPath = sourceDescriptionPropertyPath,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjectionBuilder AddStaticValueMapProperty(
            string targetPropertyName, Type targetPropertyType,
            string sourceValuePropertyPath, Dictionary<object, object> valueMap,
            bool asNullable = false)
        {
            properties.Add(new MappedValueProperty()
            {
                PropertyName = targetPropertyName,
                PropertyType = targetPropertyType,
                SourcePropertyPath = sourceValuePropertyPath,
                MappedValues = valueMap,
                AsNullable = asNullable
            });

            return this;
        }

        public DynamicProjection Build(Type sourceType = null)
        {
            sourceType = sourceType ?? this.sourceType;
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));

            var projectedType = BuildProjectedType();
            var sourceParameter = Expression.Parameter(sourceType, sourceParameterName);

            var projectionProperties = BuildProjectionProperties(sourceParameter, projectedType).ToList();


            return new DynamicProjection(projectedType, new DynamicProjectionProperties() { SourceParameter = sourceParameter, Properties = projectionProperties });
        }

        static readonly string sourceParameterName = "source";
        private IEnumerable<DynamicProjectionProperty> BuildProjectionProperties(ParameterExpression sourceParameter, Type projectedType)
        {
            foreach (var p in properties)
            {
                var targetProperty = projectedType.GetProperty(p.PropertyName);

                if (p is DirectDynamicDTOProperty direct)
                {
                    yield return new DynamicProjectionDirectProperty()
                    {
                        SourceExpression = CreateMemberExpression(sourceParameter, direct.SourcePropertyPath),
                        TargetProperty = targetProperty
                    };
                }
                else if (p is StaticValueDynamicDTOProperty staticValueProperty)
                {
                    yield return new DynamicProjectionConstantProperty()
                    {
                        SourceExpression = Expression.Constant(staticValueProperty.Value),
                        TargetProperty = targetProperty,
                        ConstantValue = staticValueProperty.Value
                    };
                }
                else if (p is ExpressionProperty expressionProperty)
                {
                    yield return new DynamicProjectionDirectProperty()
                    {
                        SourceExpression = System.Linq.Dynamic.DynamicExpression.Parse(
                                new[] { sourceParameter }, p.PropertyType, sourceParameterName + "." + expressionProperty.SourceExpression),
                        TargetProperty = targetProperty
                    };
                }
                else if (p is KeyDescriptionProperty keyDescriptionProperty)
                {
                    yield return new DynamicProjectionKeyedDescriptionProperty()
                    {
                        SourceExpression = CreateMemberExpression(sourceParameter, keyDescriptionProperty.SourceDescriptionPropertyPath),
                        TargetProperty = targetProperty,
                        KeySourceExpression = CreateMemberExpression(sourceParameter, keyDescriptionProperty.SourceValuePropertyPath)
                    };
                }
                else if (p is MappedValueProperty mappedValueProperty)
                {
                    var memberExpression = CreateMemberExpression(sourceParameter, mappedValueProperty.SourcePropertyPath);

                    var defaultValue =
                        mappedValueProperty.AsNullable || Nullable.GetUnderlyingType(mappedValueProperty.PropertyType) != null
                        ? null : mappedValueProperty.PropertyType.DefaultOf();

                    memberExpression = mappedValueProperty.MappedValues.Reverse().Aggregate(
                        ExpressionEx.ConvertIfNeeded(Expression.Constant(defaultValue), mappedValueProperty.PropertyType),
                        (current, next) => Expression.Condition(
                                                Expression.Equal(memberExpression, ExpressionEx.ConvertIfNeeded(Expression.Constant(next.Key), memberExpression.Type)),
                                                ExpressionEx.ConvertIfNeeded(Expression.Constant(next.Value), mappedValueProperty.PropertyType),
                                                current));

                    yield return new DynamicProjectionMappedValueProperty()
                    {
                        SourceExpression = memberExpression,
                        TargetProperty = targetProperty,
                        MappedValues = mappedValueProperty.MappedValues
                    };
                }
                else if (p is LookupValueProperty lookupValueProperty)
                {
                    //memberExpression = CreateMemberExpression(inParam, keyDescriptionProperty.SourceDescriptionPropertyPath);
                }
            };
        }

        protected static Expression CreateMemberExpression(Expression body, string propertyFullPath)
        {
            if (string.IsNullOrEmpty(propertyFullPath))
                return body;

            foreach (var member in propertyFullPath.Split('.'))
                body = Expression.PropertyOrField(body, member);
            return body;
        }

        private Type BuildProjectedType()
        {
            var typeDescriptor = new DynamicTypeDescriptor()
                .HasName(dtoTypeName);

            foreach (var p in properties)
            {
                typeDescriptor.AddProperty(p.PropertyName, p.PropertyType, p.AsNullable);
            }

            return DynamicTypeBuilder.Instance.CreateType(typeDescriptor);
        }


    }

    
}
