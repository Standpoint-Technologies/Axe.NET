using System.Linq;
using System.Text.RegularExpressions;

namespace Axe.FieldParsers
{
    public class GoogleParser : IFieldParser
    {
        private Regex _nestRegex = new Regex(@"(?<field>[^\(]+)\((?<subFields>.*)\)");

        private Regex _splitRegex = new Regex(@"([^,]*\x28[^\x29]*\x29|[^,]+)");


        public FieldRing ParseFields(string fields)
        {
            var ret = new FieldRing();
            foreach (var field in _splitRegex.Split(fields).Where(x => x != string.Empty && x != ",").Select(x => x.Trim()))
            {
                var nestedMatch = _nestRegex.Match(field);
                if (nestedMatch.Success)
                {
                    var fieldName = nestedMatch.Groups["field"].Value;
                    var subFields = nestedMatch.Groups["subFields"].Value;

                    ret.NestedRings.Add(fieldName, ParseFields(subFields));
                }
                else
                {
                    ret.Fields.Add(field);
                }
            }
            return ret;
        }
    }
}
