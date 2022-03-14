#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class InviteLinks
    {
        internal static async Task<int> FillMissingLinksIntoDB_Async(TelegramBotAbstract sender, MessageEventArgs e)
        {
            const string q1 = "SELECT id FROM Groups WHERE link IS NULL OR link = ''";
            var dt = SqLite.ExecuteSelect(q1);

            var n = 0;
            if (dt == null || dt.Rows.Count == 0)
                return n;

            foreach (DataRow dr in dt.Rows)
            {
                var success = await CreateInviteLinkAsync((long)dr.ItemArray[0], sender, e);
                switch (success.isNuovo)
                {
                    case SuccessoGenerazioneLink.NUOVO_LINK:
                    case SuccessoGenerazioneLink.RICICLATO:
                        n++;
                        break;

                    case SuccessoGenerazioneLink.ERRORE:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return n;
        }

        internal static async Task<NuovoLink> CreateInviteLinkAsync(long chatId, TelegramBotAbstract sender,
            MessageEventArgs messageEventArgs)
        {
            var successoGenerazione = SuccessoGenerazioneLink.ERRORE;
            var r = await TryGetCurrentInviteLinkAsync(chatId, sender, messageEventArgs);
            if (string.IsNullOrEmpty(r))
                try
                {
                    r = await sender.ExportChatInviteLinkAsync(chatId, null);
                }
                catch
                {
                    // ignored
                }
            else
                successoGenerazione = SuccessoGenerazioneLink.RICICLATO;

            if (string.IsNullOrEmpty(r))
                return new NuovoLink(successoGenerazione);
            successoGenerazione = SuccessoGenerazioneLink.NUOVO_LINK;

            SalvaNuovoLink(r, chatId);

            return new NuovoLink(successoGenerazione, r);
        }

        private static async Task<string> TryGetCurrentInviteLinkAsync(long chatId, TelegramBotAbstract sender,
            MessageEventArgs messageEventArgs)
        {
            Chat chat = null;
            try
            {
                await Task.Delay(100);
                chat = (await sender.GetChat(chatId))?.Item1;
                return chat?.InviteLink;
            }
            catch (Exception ex1)
            {
                Logger.WriteLine(ex1);
                var ex3M = "5" +
                    "\n\n" + ex1.Message +
                    "\n\n" + chatId +
                    "\n\n" + chat == null ? "[null class]" :
                    string.IsNullOrEmpty(chat.Title) ? "[null or empty title]" : chat.Title;

                await NotifyUtil.NotifyOwners(ex3M, sender, messageEventArgs);
                return null;
            }
        }

        private static void SalvaNuovoLink(string nuovoLink, long chatId)
        {
            const string q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                { "@link", nuovoLink },
                { "@lul", DateTime.Now },
                { "@id", chatId }
            });
        }

        internal static async Task UpdateLinksFromJsonAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Chat.Type != ChatType.Private)
                    return;

                if (!Owners.CheckIfOwner(e.Message.From.Id))
                    return;

                if (e.Message.ReplyToMessage?.Document == null)
                    return;

                var d = e.Message.ReplyToMessage.Document;
                var (_, item2) = await sender.DownloadFileAsync(d);
                Logger.WriteLine(item2.Length);
                item2.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(item2);
                var text = await reader.ReadToEndAsync();

                var obj = JsonConvert.DeserializeObject(text);
                Logger.WriteLine(obj.GetType());
                var jArray = (JArray)obj;

                var L = new ListaGruppiTG_Update();

                var gruppoTGs = new List<GruppoTG>();
                foreach (var x in jArray)
                    try
                    {
                        var jObject = (JObject)x;
                        var gruppoTG = new GruppoTG(jObject["id_link"], jObject["class"], jObject["permanentId"],
                            jObject["LastUpdateInviteLinkTime"]);
                        gruppoTGs.Add(gruppoTG);
                    }
                    catch
                    {
                        ;
                    }

                gruppoTGs = RimuoviDuplicati(gruppoTGs);

                foreach (var gruppoTG in gruppoTGs)
                {
                    try
                    {
                        await Task.Delay(100);
                    }
                    catch
                    {
                        ;
                    }

                    try
                    {
                        await UpdateLinksFromJson2Async(gruppoTG, sender, e, L);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine(ex);
                        var ex4m = "4" + "\n\n" + ex.Message;
                        await sender.SendTextMessageAsync(e.Message.From.Id,
                            new Language(
                                new Dictionary<string, string>
                                {
                                    {
                                        "it",
                                        ex4m
                                    }
                                }),
                            ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);
                        return;
                    }
                }

                var s2 = "Link generati\n\n" +
                         "Totale: " + L.Count() + "\n" +
                         "Nuovi: " + L.GetCount_Filtered(SuccessoGenerazioneLink.NUOVO_LINK) + "\n" +
                         "Riciclati: " + L.GetCount_Filtered(SuccessoGenerazioneLink.RICICLATO) + "\n" +
                         "Errori: " + L.GetCount_Filtered(SuccessoGenerazioneLink.ERRORE) + "\n";

                await sender.SendTextMessageAsync(e.Message.From.Id,
                    new Language(
                        new Dictionary<string, string>
                        {
                            {
                                "it",
                                s2
                            }
                        }),
                    ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                var st = L.GetStringList();

                var dict = new Dictionary<string, string>
                {
                    { "it", "Gruppi con link rigenerati" }
                };
                var stream = UtilsFileText.GenerateStreamFromString(st);
                var tf = new TelegramFile(stream, "groups.txt", "Gruppi con link rigenerati", "text/plain");
                await sender.SendFileAsync(tf, new PeerAbstract(e.Message.From.Id, e.Message.Chat.Type),
                    new Language(dict),
                    TextAsCaption.AFTER_FILE, e.Message.From.Username, e.Message.From.LanguageCode, null, false);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
            }
        }

        private static async Task UpdateLinksFromJson2Async(GruppoTG gruppoTG, TelegramBotAbstract sender,
            MessageEventArgs e, ListaGruppiTG_Update L)
        {
            var result = new GruppoTG_Update(null, SuccessoGenerazioneLink.ERRORE);

            var group_id = gruppoTG.permanentId;
            var sql1 = "empty";
            if (!string.IsNullOrEmpty(gruppoTG.idLink))
            {
                sql1 = "SELECT id FROM Groups " +
                       "WHERE Groups.link LIKE '%" + gruppoTG.idLink + "%'";

                if (gruppoTG.idLink.Length < 3) gruppoTG.idLink = "";
            }

            if (group_id == null)
                try
                {
                    if (!string.IsNullOrEmpty(gruppoTG.idLink))
                    {
                        var r1 = SqLite.ExecuteSelect(sql1);
                        if (r1 is { Rows: { Count: > 0 } } && r1.Rows[0].ItemArray.Length > 0)
                        {
                            var r2 = r1.Rows[0];
                            var r3 = r2.ItemArray[0];
                            group_id = Convert.ToInt64(r3);
                        }
                        else
                        {
                            result.Query1Fallita = true;
                        }
                    }
                }
                catch (Exception ex1)
                {
                    Logger.WriteLine(ex1);
                    var ex1m = "1" + "\n\n" + ex1.Message + "\n\n" + sql1 + "\n\n" + gruppoTG.idLink + "\n\n" +
                               gruppoTG.nome + "\n\n" + gruppoTG.newLink + "\n\n" + gruppoTG.permanentId;
                    await sender.SendTextMessageAsync(e.Message.From.Id,
                        new Language(
                            new Dictionary<string, string>
                            {
                                {
                                    "it",
                                    ex1m
                                }
                            }),
                        ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                    result.gruppoTG = gruppoTG;
                    result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex1m;
                    result.ExceptionObject = ex1;
                    L.Add(result);

                    return;
                }

            const string sql2 = "SELECT id FROM Groups WHERE Groups.title LIKE '%' || @nome || '%'";

            if (group_id == null)
                try
                {
                    if (!string.IsNullOrEmpty(gruppoTG.nome))
                    {
                        var r1 = SqLite.ExecuteSelect(sql2,
                            new Dictionary<string, object> { { "@nome", gruppoTG.nome } });
                        if (r1 is { Rows: { Count: > 0 } } && r1.Rows[0].ItemArray.Length > 0)
                        {
                            var r2 = r1.Rows[0];
                            var r3 = r2.ItemArray[0];
                            group_id = Convert.ToInt64(r3);
                        }
                        else
                        {
                            result.Query2Fallita = true;
                        }
                    }
                }
                catch (Exception ex2)
                {
                    Logger.WriteLine(ex2);
                    var ex2m = "2" + "\n\n" + ex2.Message + "\n\n" + sql2 + "\n\n" + gruppoTG.nome;
                    await sender.SendTextMessageAsync(e.Message.From.Id,
                        new Language(
                            new Dictionary<string, string>
                            {
                                {
                                    "it",
                                    ex2m
                                }
                            }),
                        ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                    result.gruppoTG = gruppoTG;
                    result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex2m;
                    result.ExceptionObject = ex2;
                    L.Add(result);

                    return;
                }

            if (group_id == null)
            {
                result.gruppoTG = gruppoTG;
                result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                L.Add(result);
            }
            else
            {
                NuovoLink s3 = null;
                try
                {
                    if (group_id != null)
                    {
                        gruppoTG.UpdateID(group_id.Value);

                        s3 = await CreateInviteLinkAsync(group_id.Value, sender, e);
                        if (s3 != null) gruppoTG.UpdateNewLink(s3.link);
                    }
                }
                catch (Exception ex3)
                {
                    Logger.WriteLine(ex3);
                    var ex3m = "3" + "\n\n" + ex3.Message;
                    await sender.SendTextMessageAsync(e.Message.From.Id,
                        new Language(
                            new Dictionary<string, string>
                            {
                                {
                                    "it",
                                    ex3m
                                }
                            }),
                        ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                    result.gruppoTG = gruppoTG;
                    result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex3m;
                    result.ExceptionObject = ex3;
                    result.CreateInviteLinkFallita = true;
                    L.Add(result);

                    return;
                }

                var successoGenerazione = GetSuccessoGenerazione(s3);

                result.gruppoTG = gruppoTG;
                result.successoGenerazioneLink = successoGenerazione;
                L.Add(result);
            }
        }

        private static SuccessoGenerazioneLink GetSuccessoGenerazione(NuovoLink s3)
        {
            return s3?.isNuovo ?? SuccessoGenerazioneLink.ERRORE;
        }

        private static List<GruppoTG> RimuoviDuplicati(List<GruppoTG> gruppoTGs)
        {
            if (gruppoTGs == null)
                return new List<GruppoTG>();

            for (var i = 0; i < gruppoTGs.Count; i++)
            for (var j = i + 1; j < gruppoTGs.Count; j++)
                if (i != j)
                    if (gruppoTGs[i].permanentId != null && gruppoTGs[j].permanentId != null)
                        if (gruppoTGs[i].permanentId == gruppoTGs[j].permanentId)
                        {
                            gruppoTGs[i].oldLinks.AddRange(gruppoTGs[j].oldLinks);
                            gruppoTGs.RemoveAt(j);
                            j--;
                        }

            return gruppoTGs;
        }
    }
}