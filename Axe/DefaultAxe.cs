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
        /// The default AxeProfile.
        /// </summary>
        public static AxeProfile Profile { get; set; }

        static DefaultAxe()
        {
            Profile = new AxeProfile()
            {
                ExpressionBuilder = new DefaultExpressionBuilder(),
                FieldParser = new GoogleParser()
            };
        }
    }
}
