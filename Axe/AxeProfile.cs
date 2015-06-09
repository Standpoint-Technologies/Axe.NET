using System.Collections.Generic;
using Axe.ExpressionBuilders;
using Axe.FieldParsers;

namespace Axe
{
    public class AxeProfile
    {
        private IDictionary<string, FieldRing> _defaultFields = new Dictionary<string, FieldRing>();

        /// <summary>
        /// Gets or sets the expression builder to use when generating the Select expression.
        /// </summary>
        public IExpressionBuilder ExpressionBuilder { get; set; }

        /// <summary>
        /// Enables or disables dynamic type extension. This adds overhead, but will fix Entity Framework's error of multiple constructors of the same type.
        /// </summary>
        public bool ExtendTypesDynamically { get; set; }

        /// <summary>
        /// Gets or sets the parser to use when parsing field strings.
        /// </summary>
        public IFieldParser FieldParser { get; set; }

        /// <summary>
        /// Enables or disables case sensitivity when matching field names.
        /// </summary>
        public bool IgnoreCase { get; set; }


        /// <summary>
        /// Creates a default set of fields for the entity. Must be followed by specifying fields otherwise an empty object will be returned.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public FieldRing<TEntity> CreateDefaultFields<TEntity>()
            where TEntity : class, new()
        {
            var fieldRing = new FieldRing<TEntity>();
            _defaultFields.Add(typeof(TEntity).FullName, fieldRing);
            return fieldRing;
        }

        /// <summary>
        /// Gets the defaults for the specified type if any are registered. Returns a boolean indicating if defaults were found.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="defaults"></param>
        /// <returns></returns>
        public bool TryGetDefaults<TEntity>(out FieldRing defaults)
        {
            return _defaultFields.TryGetValue(typeof(TEntity).FullName, out defaults);
        }
    }
}
