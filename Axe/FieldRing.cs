using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Axe
{
    /// <summary>
    /// Represents a set of fields and nested properties that need to be expanded.
    /// </summary>
    public class FieldRing
    {
        /// <summary>
        /// Gets or sets the fields to expand.
        /// </summary>
        public ICollection<string> Fields { get; set; }

        /// <summary>
        /// Gets or sets the nested properties to expand.
        /// </summary>
        public IDictionary<string, FieldRing> NestedRings { get; set; }

        /// <summary>
        /// Initializes a new, empty FieldRing.
        /// </summary>
        public FieldRing()
        {
            Fields = new List<string>();
            NestedRings = new Dictionary<string, FieldRing>();
        }
    }

    /// <summary>
    /// Represents a set of fields and nested properties to be expanded. Extends the base FieldRing with a type to allow easily adding of fields by expression.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class FieldRing<TEntity> : FieldRing
            where TEntity : class, new()
    {
        /// <summary>
        /// Adds the collection from the given expression to the list of nested rings.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <typeparam name="TCollection"></typeparam>
        /// <param name="locator"></param>
        /// <param name="subLocators"></param>
        /// <returns></returns>
        public FieldRing<TEntity> AddCollection<TProperty, TCollection>(Expression<Func<TEntity, TProperty>> locator, Action<FieldRing<TCollection>> subLocators)
            where TCollection : class, new() where TProperty : IEnumerable<TCollection>
        {
            var fieldRing = new FieldRing<TCollection>();
            subLocators(fieldRing);

            var propertyName = getPropertyInfo(locator).Name;
            NestedRings.Add(propertyName, fieldRing);
            return this;
        }

        /// <summary>
        /// Adds the fields from the given expression to the list of fields to include.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="locator"></param>
        /// <returns></returns>
        public FieldRing<TEntity> AddField<TProperty>(Expression<Func<TEntity, TProperty>> locator)
        {
            Fields.Add(getPropertyInfo(locator).Name);
            return this;
        }

        /// <summary>
        /// Adds the nested object from the given expression to the list of nested rings.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="locator"></param>
        /// <param name="subLocators"></param>
        /// <returns></returns>
        public FieldRing<TEntity> AddNestedObject<TProperty>(Expression<Func<TEntity, TProperty>> locator, Action<FieldRing<TProperty>> subLocators)
            where TProperty : class, new()
        {
            var fieldRing = new FieldRing<TProperty>();
            subLocators(fieldRing);

            var propertyName = getPropertyInfo(locator).Name;
            NestedRings.Add(propertyName, fieldRing);
            return this;
        }

        /// <summary>
        /// Retrieves the property info for the specified expression. Useful for attaining the name of the property.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyLambda"></param>
        /// <returns></returns>
        private PropertyInfo getPropertyInfo<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda)
        {
            Type type = typeof(TEntity);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.", nameof(propertyLambda));
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.", nameof(propertyLambda));
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.", nameof(propertyLambda));
            }

            return propInfo;
        }
    }
}
