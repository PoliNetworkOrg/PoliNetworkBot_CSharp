#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects
{
    public class Language
    {
        private readonly Dictionary<string, string> _dict;

        public Language(Dictionary<string, string> dict)
        {
            _dict = dict;
        }

        public string Select(string lang)
        {
            if (_dict == null)
                return null;

            if (_dict.ContainsKey(lang))
                return _dict[lang];

            if (_dict.Keys.Count == 0)
                return null;

            if (_dict.ContainsKey("en"))
                return _dict["en"];

            var d = _dict.Keys.First();
            return _dict[d];
        }

        public static bool EqualsLang(string aValue, Language bLanguage, string languageOfB)
        {
            var bValue = bLanguage.Select(languageOfB);
            return aValue == bValue;
        }
    }
}