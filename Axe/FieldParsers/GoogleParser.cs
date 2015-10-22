using System.Linq;
using System.Text.RegularExpressions;

namespace Axe.FieldParsers
{
    public class GoogleParser : IFieldParser
    {
        private readonly Regex _patternRegex = new Regex(@"(?<field>[^,\(]+)(\((?<subfields>.*?((?<open>\().+(?<-open>\))(?(open)(?!)))?)\))?");


        public FieldRing ParseFields(string fields)
        {
            var cleanedFields = new string(fields.Where(x => !char.IsWhiteSpace(x)).ToArray());
            var ret = new FieldRing();
            var match = _patternRegex.Match(cleanedFields);
            while (match.Success)
            {
                var fieldName = match.Groups["field"].Value;
                var subfieldsGroup = match.Groups["subfields"];

                if (subfieldsGroup.Success)
                {
                    var subFields = subfieldsGroup.Value;
                    ret.NestedRings.Add(fieldName, ParseFields(subFields));
                }
                else
                {
                    ret.Fields.Add(fieldName);
                }

                match = match.NextMatch();
            }
            return ret;
        }
    }
}
