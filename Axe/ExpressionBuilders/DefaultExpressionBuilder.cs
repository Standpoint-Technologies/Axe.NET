using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Axe.ExpressionBuilders
{
    public class DefaultExpressionBuilder : IExpressionBuilder
    {
        protected static Lazy<ModuleBuilder> ModuleBuilder = new Lazy<ModuleBuilder>(() =>
        {
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "AxeTempAssembly";
            AssemblyBuilder assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule("AxeTempModule");
        });

        protected static IDictionary<string, Type> FakeTypes = new Dictionary<string, Type>();


        /// <summary>
        /// Builds a Select compatible expression for the entity from the specified FieldRing and AxeProfile.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fieldRing"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public Expression<Func<TEntity, TEntity>> BuildExpression<TEntity>(FieldRing fieldRing, AxeProfile profile)
            where TEntity : class, new()
        {
            var inputParameter = Expression.Parameter(typeof(TEntity), "input");

            var newType = profile.ExtendTypesDynamically ? GetOrCreateExtendingType(typeof(TEntity)) : typeof(TEntity);

            var newExpression = Expression.New(newType);

            var fullExpression = RecursiveExpressionBuilder<TEntity>(fieldRing, profile, inputParameter);

            return Expression.Lambda<Func<TEntity, TEntity>>(fullExpression, inputParameter);
        }


        /// <summary>
        /// Generates a dynamic type that extends the specified type. Useful in situations where the original type cannot be reused.
        /// </summary>
        /// <param name="type">The type to replicate</param>
        /// <returns>The new type</returns>
        protected Type GetOrCreateExtendingType(Type type)
        {
            string typeName = type.Name + "_LimitFields";
            Type newType;
            if (!FakeTypes.TryGetValue(typeName, out newType))
            {
                TypeBuilder typeBuilder = ModuleBuilder.Value.DefineType(typeName, TypeAttributes.Public | TypeAttributes.Class, type);
                newType = typeBuilder.CreateType();
                FakeTypes.Add(typeName, newType);
            }
            return newType;
        }


        /// <summary>
        /// Recursively traverses the FieldRing and expands only the specified properties, returning the resulting expression.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fieldRing"></param>
        /// <param name="parentParameter"></param>
        /// <returns></returns>
        protected MemberInitExpression RecursiveExpressionBuilder<TEntity>(FieldRing fieldRing, AxeProfile profile, Expression parentParameter)
        {
            var newType = profile.ExtendTypesDynamically ? GetOrCreateExtendingType(typeof(TEntity)) : typeof(TEntity);
            var propertyBindingFlags = profile.IgnoreCase ? BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance;

            var newExpression = Expression.New(newType);

            var bindings = fieldRing.Fields.Select(field =>
            {
                var propertyInfo = typeof(TEntity).GetProperty(field, propertyBindingFlags);
                if (propertyInfo != null)
                {
                    var propertyExpression = Expression.Property(parentParameter, propertyInfo);
                    return Expression.Bind(propertyInfo, propertyExpression);
                }
                return null;
            }).Union(fieldRing.NestedRings.Select(field =>
            {
                var propertyInfo = typeof(TEntity).GetProperty(field.Key, propertyBindingFlags);
                if (propertyInfo != null)
                {
                    var nestedParameter = Expression.Property(parentParameter, field.Key);
                    var propertyExpression = (MemberInitExpression)typeof(DefaultExpressionBuilder)
                        .GetMethod("RecursiveExpressionBuilder", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(propertyInfo.PropertyType)
                        .Invoke(this, new object[] { field.Value, profile, nestedParameter });
                    return Expression.Bind(propertyInfo, propertyExpression);
                }
                return null;
            })).Where(x => x != null);

            return Expression.MemberInit(newExpression, bindings);
        }
    }
}
