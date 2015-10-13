using System.Linq;
using Axe.Engines;

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
            return AxeFields(query, fields, DefaultAxe.Engine);
        }

        /// <summary>
        /// Generates a select statement against the query to only include the specified fields.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <param name="engine"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AxeFields<TEntity>(this IQueryable<TEntity> query, string fields, IAxeEngine engine)
            where TEntity : class, new()
        {
            return engine.AxeFields(query, fields);
        }
    }
}
