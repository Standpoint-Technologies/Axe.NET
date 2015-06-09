using Axe.ExpressionBuilders;
using Axe.FieldParsers;

namespace Axe
{
    public class AxeProfile
    {
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
    }
}
