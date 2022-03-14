#region

using System.Collections.Generic;
using System.Linq;
using Bot.Enums;
using PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Global
{
    public static class Navigator
    {
        public static readonly Dictionary<string, string[]> ScuoleCorso = new()
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
            foreach (var corso in from scuola in ScuoleCorso.Values
                     where scuola != null
                     from corso in scuola
                     where messageText == corso
                     select corso)
            {
                conversation.setCorso(corso);
                conversation.setStato(stati.Cartella);
                return true;
            }

            return false;
        }

        public static bool ScuolaHandler(Conversation conversation, string messageText)
        {
            foreach (var scuola in ScuoleCorso.Keys.Where(scuola => messageText == scuola))
            {
                conversation.setScuola(scuola);
                conversation.setStato(stati.Corso);
                return true;
            }

            return false;
        }
    }
}