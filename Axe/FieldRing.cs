using System.Collections.Generic;

namespace Axe
{
    /// <summary>
    /// Represents a set of fields and nested properties that need to be expanded.
    /// </summary>
    public class FieldRing
    {
        /// <summary>
        /// Gets or sets the fields to expand.
        /// </summary>
        public ICollection<string> Fields { get; set; }

        /// <summary>
        /// Gets or sets the nested properties to expand.
        /// </summary>
        public IDictionary<string, FieldRing> NestedRings { get; set; }

        /// <summary>
        /// Initializes a new, empty FieldRing.
        /// </summary>
        public FieldRing()
        {
            Fields = new List<string>();
            NestedRings = new Dictionary<string, FieldRing>();
        }
    }
}
