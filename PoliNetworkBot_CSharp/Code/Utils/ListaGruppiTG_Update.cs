using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class ListaGruppiTG_Update
    {
        public List<Tuple<GruppoTG, Enums.SuccessoGenerazioneLink>> L = new List<Tuple<GruppoTG, Enums.SuccessoGenerazioneLink>>();

        internal int Count()
        {
            if (L == null)
                L = new List<Tuple<GruppoTG, Enums.SuccessoGenerazioneLink>>();

            return L.Count;
        }

        internal void Add(Tuple<GruppoTG, Enums.SuccessoGenerazioneLink> tuple)
        {
            if (L == null)
                L = new List<Tuple<GruppoTG, Enums.SuccessoGenerazioneLink>>();

            L.Add(tuple);
        }

        internal string GetStringList()
        {
            string st = "";

            foreach (var l2 in L)
            {
                try
                {
                    string s3 = "Success: " + (l2.Item2 != Enums.SuccessoGenerazioneLink.ERRORE ? "S" : "N") + "\n" +
                        "IdLink: " + StringNotNull(l2.Item1.idLink) + "\n" +
                        "NewLink: " + StringNotNull(l2.Item1.newLink) + "\n" +
                        "PermanentId: " + StringNotNull(l2.Item1.permanentId) + "\n" +
                        "Nome: " + StringNotNull(l2.Item1.nome);
                    st += s3 + "\n\n";

                    /*await sender.SendTextMessageAsync(e.Message.From.Id,
                        new Language(
                            new Dictionary<string, string>() { { "it",
                                    s3 } }),
                        ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                    */
                }
                catch (Exception ex2)
                {
                    Console.WriteLine(ex2);
                }

                //Thread.Sleep(500);
            }

            return st;
        }

        private static string StringNotNull(long? permanentId)
        {
            if (permanentId == null)
                return "null";

            return permanentId.Value.ToString();
        }

        private static string StringNotNull(string idLink)
        {
            if (idLink == null)
                return "[null]";

            if (idLink == "")
                return "[EMPTY]";

            return idLink;
        }

        internal int GetCount_Filtered(Enums.SuccessoGenerazioneLink successoGenerazione)
        {
            if (L == null)
                L = new List<Tuple<GruppoTG, Enums.SuccessoGenerazioneLink>>();

            return L.Where(x => x.Item2 == successoGenerazione).ToList().Count;
        }
    }
}