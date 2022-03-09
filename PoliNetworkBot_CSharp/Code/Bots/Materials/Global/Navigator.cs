using System.Collections.Generic;
using Bot;
using Bot.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Global
{
    static public class Navigator
    {
        public static Dictionary<string, string[]> ScuoleCorso = new Dictionary<string, string[]>()
        {
            ["3I"] = new[]
            {
                "MatNano",
                "Info",
                "MobilityMD",
                "AES",
                "Electronics",
                "Automazione",
                "Chimica",
                "Elettrica"
            },
            ["AUIC"] = null,
            ["ICAT"] = null,
            ["Design"] = null
        };
        
        public static bool CorsoHandler(Conversation conversation, string messageText)
        {
            foreach (var scuola in ScuoleCorso.Values)
            {
                foreach (var corso in scuola)
                {
                    if (messageText == corso)
                    {
                        conversation.setCorso(corso);
                        conversation.setStato(stati.Cartella);
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool ScuolaHandler(Conversation conversation, string messageText)
        {
            foreach (var scuola in ScuoleCorso.Keys)
            {
                if (messageText != scuola) continue;
                conversation.setCorso(scuola);
                conversation.setStato(stati.Scuola);
                return true;
            }

            return false;
        }
    }
}