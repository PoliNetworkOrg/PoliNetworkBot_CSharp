#region

using System.Collections.Generic;
using Bot.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Global
{
    public static class Navigator
    {
        public static Dictionary<string, string[]> ScuoleCorso = new()
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
                if (scuola == null) continue;
                foreach (var corso in scuola)
                    if (messageText == corso)
                    {
                        conversation.setCorso(corso);
                        conversation.setStato(stati.Cartella);
                        return true;
                    }
            }

            return false;
        }

        public static bool ScuolaHandler(Conversation conversation, string messageText)
        {
            foreach (var scuola in ScuoleCorso.Keys)
            {
                if (messageText != scuola) continue;
                conversation.setScuola(scuola);
                conversation.setStato(stati.Corso);
                return true;
            }

            return false;
        }
    }
}