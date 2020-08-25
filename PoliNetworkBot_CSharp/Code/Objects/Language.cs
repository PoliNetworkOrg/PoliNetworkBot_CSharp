using System.Collections.Generic;
using System.Linq;

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class Language
    {
        public readonly Dictionary<string, string> Dict;

        public Language(Dictionary<string, string> dict)
        {
            this.Dict = dict;
        }

        public string Select(string lang)
        {
            if (Dict == null)
                return null;

            if (Dict.ContainsKey(lang))
                return Dict[lang];

            if (Dict.Keys.Count == 0)
                return null;

            var d = Dict.Keys.First();
            return Dict[d];
        }

        public static bool EqualsLang(string Avalue, Language BLanguage, string languageOfB)
        {
            var Bvalue = BLanguage.Select(languageOfB);
            return Avalue == Bvalue;
        }
    }
}