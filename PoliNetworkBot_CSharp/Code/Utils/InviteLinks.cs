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
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class InviteLinks
{
    internal static async Task<int> FillMissingLinksIntoDB_Async(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        const string? q1 = "SELECT id FROM GroupsTelegram WHERE link IS NULL OR link = ''";
        var dt = Database.ExecuteSelect(q1, sender?.DbConfig);

        var n = 0;
        if (dt == null || dt.Rows.Count == 0)
            return n;

        foreach (DataRow dr in dt.Rows)
        {
            var x1 = dr.ItemArray[0];
            if (x1 == null) continue;
            var success = await CreateInviteLinkAsync((long)x1, sender, e);
            if (success != null)
                switch (success.IsNuovo)
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

    internal static async Task<NuovoLink?> CreateInviteLinkAsync(long chatId, TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs)
    {
        var r = await TryGetCurrentInviteLinkAsync(chatId, sender, messageEventArgs);
        if (!string.IsNullOrEmpty(r)) return SalvaNuovoLink(r, chatId, sender, SuccessoGenerazioneLink.RICICLATO);

        try
        {
            if (sender != null) r = await sender.ExportChatInviteLinkAsync(chatId, null);
        }
        catch
        {
            // ignored
        }

        return string.IsNullOrEmpty(r)
            ? new NuovoLink(SuccessoGenerazioneLink.ERRORE)
            : SalvaNuovoLink(r, chatId, sender, SuccessoGenerazioneLink.NUOVO_LINK);
    }

    private static async Task<string?> TryGetCurrentInviteLinkAsync(long chatId, TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs)
    {
        Chat? chat = null;
        try
        {
            await Task.Delay(100);
            if (sender != null) chat = (await sender.GetChat(chatId))?.Item1;
            return chat?.InviteLink;
        }
        catch (Exception? ex1)
        {
            Logger.Logger.WriteLine(ex1);
            var ex3M = "5" +
                       "\n\n" + ex1.Message +
                       "\n\n" + chatId +
                       "\n\n" + (chat == null ? "[null class]" :
                           string.IsNullOrEmpty(chat.Title) ? "[null or empty title]" : chat.Title);

            await NotifyUtil.NotifyOwners_AnError_AndLog3(ex3M, sender, EventArgsContainer.Get(messageEventArgs),
                FileTypeJsonEnum.SIMPLE_STRING, SendActionEnum.SEND_TEXT);
            return null;
        }
    }

    private static NuovoLink SalvaNuovoLink(string? nuovoLink, long chatId, TelegramBotAbstract? sender,
        SuccessoGenerazioneLink successoGenerazioneLink)
    {
        const string? q1 = "UPDATE GroupsTelegram SET link = @link, last_update_link = @lul WHERE id = @id";
        Database.Execute(q1, sender?.DbConfig, new Dictionary<string, object?>
        {
            { "@link", nuovoLink },
            { "@lul", GetDateTimeLastUpdateLinkFormattedString(DateTime.Now) },
            { "@id", chatId }
        });

        return new NuovoLink(successoGenerazioneLink, nuovoLink);
    }

    public static string? GetDateTimeLastUpdateLinkFormattedString(DateTime? lastUpdateLinkTime)
    {
        return lastUpdateLinkTime?.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private static async Task UpdateLinksFromJsonAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        try
        {
            if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
                return;

            if (e?.Message?.From == null)
                return;

            if (!Owners.CheckIfOwner(e.Message.From.Id))
                return;

            if (e.Message.ReplyToMessage?.Document == null)
                return;

            var d = e.Message.ReplyToMessage.Document;
            if (sender != null)
            {
                var tuple1 = await sender.DownloadFileAsync(d);
                if (tuple1 != null)
                {
                    var item2 = tuple1.Item2;
                    Logger.Logger.WriteLine(item2.Length);
                    item2.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(item2);
                    var text = await reader.ReadToEndAsync();

                    var obj = JsonConvert.DeserializeObject(text);
                    Logger.Logger.WriteLine(obj?.GetType());
                    if (obj != null)
                    {
                        var jArray = (JArray)obj;

                        var l = new ListaGruppiTgUpdate();

                        var gruppoTGs = new List<GruppoTg?>();
                        foreach (var x in jArray)
                            try
                            {
                                var jObject = (JObject)x;
                                var gruppoTg = new GruppoTg(jObject["id_link"], jObject["class"],
                                    jObject["permanentId"],
                                    jObject["LastUpdateInviteLinkTime"]);
                                gruppoTGs.Add(gruppoTg);
                            }
                            catch
                            {
                                // ignored
                            }

                        gruppoTGs = RimuoviDuplicati(gruppoTGs);

                        foreach (var gruppoTg in gruppoTGs)
                        {
                            try
                            {
                                await Task.Delay(100);
                            }
                            catch
                            {
                                // ignored
                            }

                            try
                            {
                                await UpdateLinksFromJson2Async(gruppoTg, sender, e, l);
                            }
                            catch (Exception? ex)
                            {
                                Logger.Logger.WriteLine(ex);
                                var ex4M = "4" + "\n\n" + ex.Message;
                                await sender.SendTextMessageAsync(e.Message.From.Id,
                                    new Language(
                                        new Dictionary<string, string?>
                                        {
                                            {
                                                "it",
                                                ex4M
                                            }
                                        }),
                                    ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);
                                return;
                            }
                        }

                        var s2 = "Link generati\n\n" +
                                 "Totale: " + l.Count() + "\n" +
                                 "Nuovi: " + l.GetCount_Filtered(SuccessoGenerazioneLink.NUOVO_LINK) + "\n" +
                                 "Riciclati: " + l.GetCount_Filtered(SuccessoGenerazioneLink.RICICLATO) + "\n" +
                                 "Errori: " + l.GetCount_Filtered(SuccessoGenerazioneLink.ERRORE) + "\n";

                        await sender.SendTextMessageAsync(e.Message.From.Id,
                            new Language(
                                new Dictionary<string, string?>
                                {
                                    {
                                        "it",
                                        s2
                                    }
                                }),
                            ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                        var st = l.GetStringList();

                        var dict = new Dictionary<string, string?>
                        {
                            { "it", "Gruppi con link rigenerati" }
                        };
                        var stream = UtilsFileText.GenerateStreamFromString(st);
                        var tf = new TelegramFile(stream, "groups.txt", "Gruppi con link rigenerati", "text/plain");
                        await sender.SendFileAsync(tf, new PeerAbstract(e.Message.From.Id, e.Message.Chat.Type),
                            new Language(dict),
                            TextAsCaption.AFTER_FILE, e.Message.From.Username, e.Message.From.LanguageCode, null,
                            false);
                    }
                }
            }
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
        }
    }

    private static async Task UpdateLinksFromJson2Async(GruppoTg? gruppoTg, TelegramBotAbstract? sender,
        MessageEventArgs? e, ListaGruppiTgUpdate l)
    {
        var result = new GruppoTgUpdate(null, SuccessoGenerazioneLink.ERRORE);

        if (gruppoTg != null)
        {
            var groupId = gruppoTg.PermanentId;
            var sql1 = "empty";
            if (!string.IsNullOrEmpty(gruppoTg.IdLink))
            {
                sql1 = "SELECT id FROM GroupsTelegram " +
                       "WHERE GroupsTelegram.link LIKE '%" + gruppoTg.IdLink + "%'";

                if (gruppoTg.IdLink.Length < 3) gruppoTg.IdLink = "";
            }

            if (groupId == null)
                try
                {
                    if (!string.IsNullOrEmpty(gruppoTg.IdLink))
                    {
                        var r1 = Database.ExecuteSelect(sql1, sender?.DbConfig);
                        if (r1 is { Rows.Count: > 0 } && r1.Rows[0].ItemArray.Length > 0)
                        {
                            var r2 = r1.Rows[0];
                            var r3 = r2.ItemArray[0];
                            groupId = Convert.ToInt64(r3);
                        }
                        else
                        {
                            result.Query1Fallita = true;
                        }
                    }
                }
                catch (Exception? ex1)
                {
                    Logger.Logger.WriteLine(ex1);
                    var ex1M = "1" + "\n\n" + ex1.Message + "\n\n" + sql1 + "\n\n" + gruppoTg.IdLink + "\n\n" +
                               gruppoTg.Nome + "\n\n" + gruppoTg.NewLink + "\n\n" + gruppoTg.PermanentId;
                    if (sender != null)
                        if (e?.Message != null)
                            await sender.SendTextMessageAsync(e.Message.From?.Id,
                                new Language(
                                    new Dictionary<string, string?>
                                    {
                                        {
                                            "it",
                                            ex1M
                                        }
                                    }),
                                ChatType.Private, "it", ParseMode.Html, null, e.Message.From?.Username);

                    result.GruppoTg = gruppoTg;
                    result.SuccessoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex1M;
                    l.Add(result);

                    return;
                }

            const string? sql2 = "SELECT id FROM GroupsTelegram WHERE GroupsTelegram.title LIKE '%' || @nome || '%'";

            if (groupId == null)
                try
                {
                    if (!string.IsNullOrEmpty(gruppoTg.Nome))
                    {
                        var r1 = Database.ExecuteSelect(sql2, sender?.DbConfig,
                            new Dictionary<string, object?> { { "@nome", gruppoTg.Nome } });
                        if (r1 is { Rows.Count: > 0 } && r1.Rows[0].ItemArray.Length > 0)
                        {
                            var r2 = r1.Rows[0];
                            var r3 = r2.ItemArray[0];
                            groupId = Convert.ToInt64(r3);
                        }
                        else
                        {
                            result.Query2Fallita = true;
                        }
                    }
                }
                catch (Exception? ex2)
                {
                    Logger.Logger.WriteLine(ex2);
                    var ex2M = "2" + "\n\n" + ex2.Message + "\n\n" + sql2 + "\n\n" + gruppoTg.Nome;
                    if (sender != null)
                        if (e?.Message != null)
                            await sender.SendTextMessageAsync(e.Message.From?.Id,
                                new Language(
                                    new Dictionary<string, string?>
                                    {
                                        {
                                            "it",
                                            ex2M
                                        }
                                    }),
                                ChatType.Private, "it", ParseMode.Html, null, e.Message.From?.Username);

                    result.GruppoTg = gruppoTg;
                    result.SuccessoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex2M;
                    l.Add(result);

                    return;
                }

            if (groupId == null)
            {
                result.GruppoTg = gruppoTg;
                result.SuccessoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                l.Add(result);
            }
            else
            {
                NuovoLink? s3;
                try
                {
                    gruppoTg.UpdateId(groupId.Value);

                    s3 = await CreateInviteLinkAsync(groupId.Value, sender, e);
                    if (s3 != null) gruppoTg.UpdateNewLink(s3.Link);
                }
                catch (Exception? ex3)
                {
                    Logger.Logger.WriteLine(ex3);
                    var ex3M = "3" + "\n\n" + ex3.Message;
                    if (sender != null)
                        if (e?.Message?.From != null)
                            await sender.SendTextMessageAsync(e.Message.From.Id,
                                new Language(
                                    new Dictionary<string, string?>
                                    {
                                        {
                                            "it",
                                            ex3M
                                        }
                                    }),
                                ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                    result.GruppoTg = gruppoTg;
                    result.SuccessoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                    result.ExceptionMessage = ex3M;
                    result.CreateInviteLinkFallita = true;
                    l.Add(result);

                    return;
                }

                var successoGenerazione = GetSuccessoGenerazione(s3);

                result.GruppoTg = gruppoTg;
                result.SuccessoGenerazioneLink = successoGenerazione;
                l.Add(result);
            }
        }
    }

    private static SuccessoGenerazioneLink GetSuccessoGenerazione(NuovoLink? s3)
    {
        return s3?.IsNuovo ?? SuccessoGenerazioneLink.ERRORE;
    }

    private static List<GruppoTg?> RimuoviDuplicati(List<GruppoTg?>? gruppoTGs)
    {
        if (gruppoTGs == null)
            return new List<GruppoTg?>();

        for (var i = 0; i < gruppoTGs.Count; i++)
        for (var j = i + 1; j < gruppoTGs.Count; j++)
            if (i != j)
            {
                var gruppoTg1 = gruppoTGs[i];
                var gruppoTg2 = gruppoTGs[j];
                if (gruppoTg2 == null || gruppoTg1 is not { PermanentId: { } } ||
                    gruppoTg2.PermanentId == null) continue;

                if (gruppoTg1.PermanentId != gruppoTg2.PermanentId) continue;

                gruppoTg1.OldLinks.AddRange(gruppoTg2.OldLinks);
                gruppoTGs.RemoveAt(j);
                j--;
            }

        return gruppoTGs;
    }

    public static async Task<CommandExecutionState> UpdateLinksFromJsonAsync2(MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        await UpdateLinksFromJsonAsync(sender, e);
        return CommandExecutionState.SUCCESSFUL;
    }
}