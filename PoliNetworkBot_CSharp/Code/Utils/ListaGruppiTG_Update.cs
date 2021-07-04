using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class ListaGruppiTG_Update
    {
        public List<Tuple<GruppoTG, bool>> L = new List<Tuple<GruppoTG, bool>>();

        internal int? Count()
        {
            if (L == null)
                L = new List<Tuple<GruppoTG, bool>>();

            return L.Count;
        }

        internal void Add(Tuple<GruppoTG, bool> tuple)
        {
            if (L == null)
                L = new List<Tuple<GruppoTG, bool>>();

            L.Add(tuple);
        }

      

        internal string GetStringList()
        {
            string st = "";

            


            foreach (var l2 in L)
            {
                try
                {
                    string s3 = "Success: " + (l2.Item2 ? "S" : "N") + "\n" +
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

    }
}