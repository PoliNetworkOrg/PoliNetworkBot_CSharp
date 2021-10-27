#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

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
                var success = await CreateInviteLinkAsync((long) dr.ItemArray[0], sender, e);
                switch (success.isNuovo)
                {
                    case SuccessoGenerazioneLink.NUOVO_LINK:
                    case SuccessoGenerazioneLink.RICICLATO:
                        n++;
                        break;
                }
            }

            return n;
        }

        internal static async Task<NuovoLink> CreateInviteLinkAsync(long chatId, TelegramBotAbstract sender,
            MessageEventArgs e)
        {
            var successoGenerazione = SuccessoGenerazioneLink.ERRORE;
            var r = await TryGetCurrentInviteLinkAsync(chatId, sender, e);
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
            MessageEventArgs e)
        {
            Chat chat = null;
            try
            {
                await Task.Delay(100);
                chat = await sender.GetChat(chatId);
                if (chat == null)
                    return null;

                return chat.InviteLink;
            }
            catch (Exception ex1)
            {
                Console.WriteLine(ex1);
                var ex3m = "5" +
                    "\n\n" + ex1.Message +
                    "\n\n" + chatId +
                    "\n\n" + chat == null ? "[null class]" :
                    string.IsNullOrEmpty(chat.Title) ? "[null or empty title]" : chat.Title;

                await sender.SendTextMessageAsync(e.Message.From.Id,
                    new Language(
                        new Dictionary<string, string>
                        {
                            {
                                "it",
                                ex3m
                            }
                        }),
                    ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                return null;
            }

            return null;
        }

        private static void SalvaNuovoLink(string nuovoLink, long chatId)
        {
            const string q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                {"@link", nuovoLink},
                {"@lul", DateTime.Now},
                {"@id", chatId}
            });
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        internal static async Task UpdateLinksFromJsonAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Chat.Type != ChatType.Private)
                    return;

                if (!Owners.CheckIfOwner(e.Message.From.Id))
                    return;

                if (e.Message.ReplyToMessage == null)
                    return;

                if (e.Message.ReplyToMessage.Document == null)
                    return;

                var d = e.Message.ReplyToMessage.Document;
                var f = await sender.DownloadFileAsync(d);
                Console.WriteLine(f.Item2.Length);
                f.Item2.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(f.Item2);
                var text = reader.ReadToEnd();

                var obj = JsonConvert.DeserializeObject(text);
                Console.WriteLine(obj.GetType());
                var jArray = (JArray) obj;

                var L = new ListaGruppiTG_Update();

                var gruppoTGs = new List<GruppoTG>();
                foreach (var x in jArray)
                    try
                    {
                        var jObject = (JObject) x;
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
                        Console.WriteLine(ex);
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
                            ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
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
                    ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);

                var st = L.GetStringList();

                var dict = new Dictionary<string, string>
                {
                    {"it", "Gruppi con link rigenerati"}
                };
                var stream = GenerateStreamFromString(st);
                var tf = new TelegramFile(stream, "groups.txt", "Gruuppi con link rigenerati", "text/plain");
                await sender.SendFileAsync(tf, new Tuple<TLAbsInputPeer, long>(null, e.Message.From.Id),
                    new Language(dict),
                    TextAsCaption.AFTER_FILE, e.Message.From.Username, e.Message.From.LanguageCode, null, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                        if (r1 != null && r1.Rows != null && r1.Rows.Count > 0 && r1.Rows[0] != null &&
                            r1.Rows[0].ItemArray != null && r1.Rows[0].ItemArray.Length > 0)
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
                    Console.WriteLine(ex1);
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
                        ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);

                    result.gruppoTG = gruppoTG;
                    result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex1m;
                    result.ExceptionObject = ex1;
                    L.Add(result);

                    return;
                }

            var sql2 = "SELECT id FROM Groups WHERE Groups.title LIKE '%' || @nome || '%'";

            if (group_id == null)
                try
                {
                    if (group_id == null && !string.IsNullOrEmpty(gruppoTG.nome))
                    {
                        var r1 = SqLite.ExecuteSelect(sql2, new Dictionary<string, object> {{"@nome", gruppoTG.nome}});
                        if (r1 != null && r1.Rows != null && r1.Rows.Count > 0 && r1.Rows[0] != null &&
                            r1.Rows[0].ItemArray != null && r1.Rows[0].ItemArray.Length > 0)
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
                    Console.WriteLine(ex2);
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
                        ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);

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
                    Console.WriteLine(ex3);
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
                        ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);

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
            if (s3 == null)
                return SuccessoGenerazioneLink.ERRORE;

            return s3.isNuovo;
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