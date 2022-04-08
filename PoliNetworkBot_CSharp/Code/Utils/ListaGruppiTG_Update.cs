#region

using PoliNetworkBot_CSharp.Code.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class ListaGruppiTG_Update
{
    private List<GruppoTG_Update> L = new();

    internal int Count()
    {
        L ??= new List<GruppoTG_Update>();

        return L.Count;
    }

    internal void Add(GruppoTG_Update tuple)
    {
        L ??= new List<GruppoTG_Update>();

        L.Add(tuple);
    }

    internal string GetStringList()
    {
        var st = "";

        foreach (var l2 in L)
            try
            {
                var s3 = "Success: " + (l2.successoGenerazioneLink != SuccessoGenerazioneLink.ERRORE ? "S" : "N") +
                         "\n" +
                         "IdLink: " + StringNotNull(l2.gruppoTG.idLink) + "\n" +
                         "NewLink: " + StringNotNull(l2.gruppoTG.newLink) + "\n" +
                         "PermanentId: " + StringNotNullFromLong(l2.gruppoTG.permanentId) + "\n" +
                         "OldLink: " + "[";

                if (l2.gruppoTG.oldLinks == null || l2.gruppoTG.oldLinks.Count == 0)
                {
                    ;
                }
                else
                {
                    var s4 = l2.gruppoTG.oldLinks.Aggregate("", (current, l3) => current + "'" + l3 + "',");
                    s4 = s4.Remove(s4.Length - 1);
                    s3 += s4;
                }

                s3 += "]" + "\n" +
                      "ExceptionMessage: " + StringNotNull(HttpUtility.HtmlEncode(l2.ExceptionMessage)) + "\n" +
                      "q1: " + StringNotNullFromBool(l2.Query1Fallita) + "\n" +
                      "q2: " + StringNotNullFromBool(l2.Query2Fallita) + "\n" +
                      "q3: " + StringNotNullFromBool(l2.CreateInviteLinkFallita) + "\n" +
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
                Logger.Logger.WriteLine(ex2);
            }

        //Thread.Sleep(500);

        return st;
    }

    private static string StringNotNullFromBool(bool? query2Fallita)
    {
        if (query2Fallita == null) return StringNotNull(null);

        return query2Fallita.Value ? "Y" : "N";
    }

    private static string StringNotNullFromLong(long? permanentId)
    {
        return permanentId == null ? "null" : permanentId.Value.ToString();
    }

    private static string StringNotNull(string idLink)
    {
        if (idLink == null)
            return "[null]";

        return idLink == "" ? "[EMPTY]" : idLink;
    }

    internal int GetCount_Filtered(SuccessoGenerazioneLink successoGenerazione)
    {
        L ??= new List<GruppoTG_Update>();

        return L.Where(x => x.successoGenerazioneLink == successoGenerazione).ToList().Count;
    }
}