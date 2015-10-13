using System.Linq;

namespace Axe.Engines
{
    public class DefaultAxeEngine : IAxeEngine
    {
        public AxeProfile Profile { get; set; }


        public DefaultAxeEngine(AxeProfile profile)
        {
            Profile = profile;
        }


        /// <summary>
        /// Generates a select statement against the query to only include the specified fields.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public IQueryable<TEntity> AxeFields<TEntity>(IQueryable<TEntity> query, string fields)
            where TEntity : class, new()
        {
            FieldRing fieldRing;
            if (!parseFieldsOrGetDefaults<TEntity>(fields, out fieldRing))
            {
                return query;
            }

            var selector = Profile.ExpressionBuilder.BuildExpression<TEntity>(fieldRing, Profile);
            return query.Select(selector);
        }

        /// <summary>
        /// Eliminates unwanted fields from the object.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public TEntity AxeFields<TEntity>(TEntity entity, string fields)
            where TEntity : class, new()
        {
            FieldRing fieldRing;
            if (!parseFieldsOrGetDefaults<TEntity>(fields, out fieldRing))
            {
                return entity;
            }

            var selector = Profile.ExpressionBuilder.BuildExpression<TEntity>(fieldRing, Profile);
            return selector.Compile()(entity);
        }


        /// <summary>
        /// Parses the field list if any are specified then checks for defaults. Returns true if fields are specified or defaults are found.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="fields"></param>
        /// <param name="fieldRing"></param>
        /// <returns></returns>
        private bool parseFieldsOrGetDefaults<TEntity>(string fields, out FieldRing fieldRing)
        {
            if (string.IsNullOrEmpty(fields))
            {
                if (!Profile.TryGetDefaults<TEntity>(out fieldRing))
                {
                    return false;
                }
            }
            else
            {
                fieldRing = Profile.FieldParser.ParseFields(fields);
            }
            return true;
        }
    }
}
