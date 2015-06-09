using System.Linq;

namespace Axe.QueryableExtensions
{
    public static class QueryableExtensions
    {
        /// <summary>
        /// Generates a select statement against the query to only include the specified fields.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AxeFields<TEntity>(this IQueryable<TEntity> query, string fields)
            where TEntity : class, new()
        {
            return AxeFields<TEntity>(query, fields, DefaultAxe.Profile);
        }

        /// <summary>
        /// Generates a select statement against the query to only include the specified fields.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AxeFields<TEntity>(this IQueryable<TEntity> query, string fields, AxeProfile profile)
            where TEntity : class, new()
        {
            if (string.IsNullOrEmpty(fields))
            {
                return query;
            }

            var fieldRing = profile.FieldParser.ParseFields(fields);
            var selector = profile.ExpressionBuilder.BuildExpression<TEntity>(fieldRing, profile);
            return query.Select(selector);
        }
    }
}
