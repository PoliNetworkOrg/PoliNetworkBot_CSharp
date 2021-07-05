using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class ListaGruppiTG_Update
    {
        public List<GruppoTG_Update> L = new List<GruppoTG_Update>();

        internal int Count()
        {
            if (L == null)
                L = new List<GruppoTG_Update>();

            return L.Count;
        }

        internal void Add(GruppoTG_Update tuple)
        {
            if (L == null)
                L = new List<GruppoTG_Update>();

            L.Add(tuple);
        }

        internal string GetStringList()
        {
            string st = "";

            foreach (var l2 in L)
            {
                try
                {
                    string s3 = "Success: " + (l2.successoGenerazioneLink != Enums.SuccessoGenerazioneLink.ERRORE ? "S" : "N") + "\n" +
                        "IdLink: " + StringNotNull(l2.gruppoTG.idLink) + "\n" +
                        "NewLink: " + StringNotNull(l2.gruppoTG.newLink) + "\n" +
                        "PermanentId: " + StringNotNullFromLong(l2.gruppoTG.permanentId) + "\n" +
                        "OldLink: " + "[";

                    if (l2.gruppoTG.oldLinks == null || l2.gruppoTG.oldLinks.Count == 0)
                        ;
                    else
                    {
                        string s4 = "";
                        foreach (var l3 in l2.gruppoTG.oldLinks)
                        {
                            s4 += "'" + l3 + "',";
                        }
                        s4 = s4.Remove(s4.Length - 1);
                        s3 += s4;
                    }

                    s3 += "]" + "\n" +
                            "ExceptionMessage: " + StringNotNull(System.Web.HttpUtility.HtmlEncode(l2.ExceptionMessage)) + "\n" +

                            "q1: " + StringNotNullFromBool(l2.query1Fallita) + "\n" +

                            "q2: " + StringNotNullFromBool(l2.query2Fallita) + "\n" +

                            "q3: " + StringNotNullFromBool(l2.createInviteLinkFallita) + "\n" +
                        "Nome: " + StringNotNull(l2.gruppoTG.nome);
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

        private string StringNotNullFromBool(bool? query2Fallita)
        {
            if (query2Fallita == null)
            {
                return StringNotNull(null);
            }

            return query2Fallita.Value ? "Y" : "N";

        }

        private static string StringNotNullFromLong(long? permanentId)
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
                L = new List<GruppoTG_Update>();

            return L.Where(x => x.successoGenerazioneLink == successoGenerazione).ToList().Count;
        }
    }
}