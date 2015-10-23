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
            AssemblyName assemblyName = new AssemblyName { Name = "AxeTempAssembly" };
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
        /// <param name="profile"></param>
        /// <param name="parentParameter"></param>
        /// <returns></returns>
        protected Expression RecursiveExpressionBuilder<TEntity>(FieldRing fieldRing, AxeProfile profile, Expression parentParameter, int counter = 1)
        {
            Type oldType = typeof(TEntity);
            Type collectionType = null;

            // If type is a collection, we need the inner type
            var isCollection = false;
            var ienumerable = oldType.IsGenericType && oldType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? oldType : oldType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault();
            var inputParameter = parentParameter;
            if (ienumerable != null)
            {
                isCollection = true;
                collectionType = oldType;
                oldType = ienumerable.GetGenericArguments().First();
                inputParameter = Expression.Parameter(oldType, $"input{counter}");
            }

            var newType = profile.ExtendTypesDynamically ? GetOrCreateExtendingType(oldType) : oldType;
            var propertyBindingFlags = profile.IgnoreCase ? BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance;

            var newExpression = Expression.New(newType);

            var bindings = fieldRing.Fields.Select(field =>
            {
                var propertyInfo = oldType.GetProperty(field, propertyBindingFlags);
                if (propertyInfo != null)
                {
                    var propertyExpression = Expression.Property(inputParameter, propertyInfo);
                    return Expression.Bind(propertyInfo, propertyExpression);
                }
                return null;
            }).Union(fieldRing.NestedRings.Select(field =>
            {
                var propertyInfo = oldType.GetProperty(field.Key, propertyBindingFlags);
                if (propertyInfo != null)
                {
                    var nestedParameter = Expression.Property(inputParameter, field.Key);
                    var propertyExpression = (Expression)typeof(DefaultExpressionBuilder)
                        .GetMethod("RecursiveExpressionBuilder", BindingFlags.NonPublic | BindingFlags.Instance)
                        .MakeGenericMethod(propertyInfo.PropertyType)
                        .Invoke(this, new object[] { field.Value, profile, nestedParameter, counter + 1 });
                    return Expression.Bind(propertyInfo, propertyExpression);
                }
                return null;
            })).Where(x => x != null);

            if (isCollection)
            {
                var initExpression = Expression.MemberInit(newExpression, bindings);

                var selectExpression = getSelectExpression(oldType, newType, parentParameter, initExpression, (ParameterExpression)inputParameter, $"input{counter}");

                if (isList(collectionType, oldType))
                {
                    return getToListCallExpression(parentParameter.Type, oldType, selectExpression);
                }
                else if (collectionType.IsArray)
                {
                    return getToArrayCallExpression(oldType, selectExpression);
                }

                return selectExpression;
            }
            else
            {
                return Expression.MemberInit(newExpression, bindings);
            }
        }

        private static Expression getSelectExpression(Type sourceType, Type destinationType, Expression source, MemberInitExpression initExpression, ParameterExpression instanceParameter, string parameterName)
        {
            var delegateType = typeof(Func<,>).MakeGenericType(sourceType, destinationType);
            var newLambdaExpression = Expression.Lambda(delegateType, initExpression, instanceParameter);

            return Expression.Call(
                typeof(Enumerable),
                "Select",
                new Type[] { sourceType, destinationType },
                source,
                newLambdaExpression);
        }

        private static MethodCallExpression getToArrayCallExpression(Type destinationType, Expression selectExpression)
        {
            return Expression.Call(
                typeof(Enumerable),
                "ToArray",
                new[] { destinationType },
                selectExpression);
        }

        private static MethodCallExpression getToListCallExpression(Type collectionType, Type destinationType, Expression selectExpression)
        {
            return Expression.Call(
                typeof(Enumerable),
                collectionType.IsArray ? "ToArray" : "ToList",
                new[] { destinationType },
                selectExpression);
        }

        private static bool isList(Type collectionType, Type sourceType)
        {
            return typeof(IList<>).MakeGenericType(sourceType).IsAssignableFrom(collectionType)
                || typeof(ICollection<>).MakeGenericType(sourceType).IsAssignableFrom(collectionType);
        }
    }
}
