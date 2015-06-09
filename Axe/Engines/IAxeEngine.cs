using System.Linq;

namespace Axe.Engines
{
    public interface IAxeEngine
    {
        /// <summary>
        /// Gets or sets the AxeProfile to use when axing fields.
        /// </summary>
        AxeProfile Profile { get; set; }

        /// <summary>
        /// Eliminates unwanted fields from the query by appending a select statement that includes only the specified fields.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        IQueryable<TEntity> AxeFields<TEntity>(IQueryable<TEntity> query, string fields)
            where TEntity : class, new();

        /// <summary>
        /// Eliminates unwanted fields from the object and includes only the specified fields in the result.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        TEntity AxeFields<TEntity>(TEntity entity, string fields)
            where TEntity : class, new();
    }
}
