using System.Linq;
using Axe.Engines;
using Axe.ExpressionBuilders;
using Axe.FieldParsers;

namespace Axe
{
    /// <summary>
    /// The default Axe configuration to use if no AxeProfile is specified. Defaults utilize the GoogleParser and DefaultExpressionBuilder.
    /// </summary>
    public static class DefaultAxe
    {
        /// <summary>
        /// The default AxeEngine.
        /// </summary>
        public static IAxeEngine Engine { get; set; }

        /// <summary>
        /// The default AxeProfile.
        /// </summary>
        public static AxeProfile Profile
        {
            get
            {
                return Engine.Profile;
            }
            set
            {
                Engine.Profile = value;
            }
        }


        static DefaultAxe()
        {
            var profile = new AxeProfile
            {
                ExpressionBuilder = new DefaultExpressionBuilder(),
                FieldParser = new GoogleParser()
            };
            Engine = new DefaultAxeEngine(profile);
        }


        /// <summary>
        /// Eliminates unwanted fields from the query by appending a select statement that includes only the specified fields.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static IQueryable<TEntity> AxeFields<TEntity>(IQueryable<TEntity> query, string fields)
            where TEntity : class, new()
        {
            return Engine.AxeFields(query, fields);
        }

        /// <summary>
        /// Eliminates unwanted fields from the object and includes only the specified fields in the result.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static TEntity AxeFields<TEntity>(TEntity entity, string fields)
            where TEntity : class, new()
        {
            return Engine.AxeFields(entity, fields);
        }
    }
}
