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
        protected Expression RecursiveExpressionBuilder<TEntity>(FieldRing fieldRing, AxeProfile profile, Expression parentParameter, int counter = 0)
        {
            Type oldType = typeof(TEntity);
            Type collectionType = oldType;
            var inputParameter = parentParameter;

            // If type is a collection, we need the inner type
            bool isCollection;
            if (isCollection = tryGetEnumerableType(oldType, out oldType))
            {
                // Switch the input parameter to reference a new paremeter for the Select
                inputParameter = Expression.Parameter(oldType, $"input{counter}");
            }

            var newType = profile.ExtendTypesDynamically ? GetOrCreateExtendingType(oldType) : oldType;
            var propertyBindingFlags = profile.IgnoreCase ? BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance;

            var newExpression = Expression.New(newType);

            var bindings = fieldRing.Fields.Select(field => getFieldBinding(field, oldType, propertyBindingFlags, inputParameter))
                .Union(fieldRing.NestedRings.Select(field => getNestedObjectBinding(field.Key, field.Value, oldType, propertyBindingFlags, inputParameter, profile, counter)))
                .Where(x => x != null);

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

        private MemberAssignment getFieldBinding(string field, Type type, BindingFlags bindingFlags, Expression source)
        {
            var propertyInfo = type.GetProperty(field, bindingFlags);

            if (propertyInfo == null)
            {
                // Property was not found on type
                return null;
            }

            var propertyExpression = Expression.Property(source, propertyInfo);
            return Expression.Bind(propertyInfo, propertyExpression);
        }

        private MemberAssignment getNestedObjectBinding(string field, FieldRing fields, Type type, BindingFlags bindingFlags, Expression source, AxeProfile profile, int counter)
        {
            var propertyInfo = type.GetProperty(field, bindingFlags);
            if (propertyInfo == null)
            {
                // Property was not found on type
                return null;
            }

            var nestedParameter = Expression.Property(source, field);
            var propertyExpression = (Expression)typeof(DefaultExpressionBuilder)
                .GetMethod("RecursiveExpressionBuilder", BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(propertyInfo.PropertyType)
                .Invoke(this, new object[] { fields, profile, nestedParameter, counter + 1 });
            var bindingExpression = profile.EnableNullReferenceCheck ? nullCheck(source, nestedParameter, propertyExpression) : propertyExpression;
            return Expression.Bind(propertyInfo, bindingExpression);
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

        private static bool tryGetEnumerableType(Type type, out Type entityType)
        {
            var ienumerable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? type : type.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .FirstOrDefault();
            if (ienumerable != null)
            {
                entityType = ienumerable.GetGenericArguments().First();
                return true;
            }

            entityType = type;
            return false;
        }

        private static Expression nullCheck(Expression source, Expression property, Expression propertyExpression)
        {
            var nullSourceExpression = Expression.Constant(null, property.Type);
            var nullDestinationExpression = Expression.Constant(null, propertyExpression.Type);
            var equalityExpression = Expression.Equal(property, nullSourceExpression);
            return Expression.Condition(equalityExpression, nullDestinationExpression, propertyExpression);
        }
    }
}
