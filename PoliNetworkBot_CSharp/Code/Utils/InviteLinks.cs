﻿#region

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
        var dt = Database.ExecuteSelect(q1, sender.DbConfig);

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

    internal static async Task<NuovoLink?> CreateInviteLinkAsync(long chatId, TelegramBotAbstract? sender,
        MessageEventArgs? messageEventArgs)
    {
        var r = await TryGetCurrentInviteLinkAsync(chatId, sender, messageEventArgs);
        if (!string.IsNullOrEmpty(r)) return SalvaNuovoLink(r, chatId, sender, SuccessoGenerazioneLink.RICICLATO);

        try
        {
            r = await sender.ExportChatInviteLinkAsync(chatId, null);
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
        Chat chat = null;
        try
        {
            await Task.Delay(100);
            chat = (await sender.GetChat(chatId))?.Item1;
            return chat?.InviteLink;
        }
        catch (Exception? ex1)
        {
            Logger.Logger.WriteLine(ex1);
            var ex3M = "5" +
                "\n\n" + ex1.Message +
                "\n\n" + chatId +
                "\n\n" + chat == null ? "[null class]" :
                string.IsNullOrEmpty(chat.Title) ? "[null or empty title]" : chat.Title;

            await NotifyUtil.NotifyOwners(ex3M, sender, messageEventArgs);
            return null;
        }
    }

    private static NuovoLink? SalvaNuovoLink(string? nuovoLink, long chatId, TelegramBotAbstract? sender,
        SuccessoGenerazioneLink successoGenerazioneLink)
    {
        const string? q1 = "UPDATE GroupsTelegram SET link = @link, last_update_link = @lul WHERE id = @id";
        Database.Execute(q1, sender.DbConfig, new Dictionary<string, object?>
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

    internal static async Task UpdateLinksFromJsonAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        try
        {
            if (e.Message.Chat.Type != ChatType.Private)
                return;

            if (e.Message.From == null)
                return;
            
            if (!Owners.CheckIfOwner(e.Message.From.Id))
                return;

            if (e.Message.ReplyToMessage?.Document == null)
                return;

            var d = e.Message.ReplyToMessage.Document;
            var (_, item2) = await sender.DownloadFileAsync(d);
            Logger.Logger.WriteLine(item2.Length);
            item2.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(item2);
            var text = await reader.ReadToEndAsync();

            var obj = JsonConvert.DeserializeObject(text);
            Logger.Logger.WriteLine(obj?.GetType());
            var jArray = (JArray)obj;

            var l = new ListaGruppiTG_Update();

            var gruppoTGs = new List<GruppoTG>();
            if (jArray != null)
                foreach (var x in jArray)
                    try
                    {
                        var jObject = (JObject)x;
                        var gruppoTg = new GruppoTG(jObject["id_link"], jObject["class"], jObject["permanentId"],
                            jObject["LastUpdateInviteLinkTime"]);
                        gruppoTGs.Add(gruppoTg);
                    }
                    catch
                    {
                        ;
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
                    ;
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
                            new Dictionary<string?, string>
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
                    new Dictionary<string?, string>
                    {
                        {
                            "it",
                            s2
                        }
                    }),
                ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

            var st = l.GetStringList();

            var dict = new Dictionary<string?, string>
            {
                { "it", "Gruppi con link rigenerati" }
            };
            var stream = UtilsFileText.GenerateStreamFromString(st);
            var tf = new TelegramFile(stream, "groups.txt", "Gruppi con link rigenerati", "text/plain");
            await sender.SendFileAsync(tf, new PeerAbstract(e.Message.From.Id, e.Message.Chat.Type),
                new Language(dict),
                TextAsCaption.AFTER_FILE, e.Message.From.Username, e.Message.From.LanguageCode, null, false);
        }
        catch (Exception? ex)
        {
            Logger.Logger.WriteLine(ex);
        }
    }

    private static async Task UpdateLinksFromJson2Async(GruppoTG gruppoTg, TelegramBotAbstract? sender,
        MessageEventArgs? e, ListaGruppiTG_Update L)
    {
        var result = new GruppoTG_Update(null, SuccessoGenerazioneLink.ERRORE);

        var groupId = gruppoTg.permanentId;
        var sql1 = "empty";
        if (!string.IsNullOrEmpty(gruppoTg.idLink))
        {
            sql1 = "SELECT id FROM GroupsTelegram " +
                   "WHERE GroupsTelegram.link LIKE '%" + gruppoTg.idLink + "%'";

            if (gruppoTg.idLink.Length < 3) gruppoTg.idLink = "";
        }

        if (groupId == null)
            try
            {
                if (!string.IsNullOrEmpty(gruppoTg.idLink))
                {
                    var r1 = Database.ExecuteSelect(sql1, sender.DbConfig);
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
                var ex1m = "1" + "\n\n" + ex1.Message + "\n\n" + sql1 + "\n\n" + gruppoTg.idLink + "\n\n" +
                           gruppoTg.nome + "\n\n" + gruppoTg.newLink + "\n\n" + gruppoTg.permanentId;
                await sender.SendTextMessageAsync(e.Message.From?.Id,
                    new Language(
                        new Dictionary<string?, string>
                        {
                            {
                                "it",
                                ex1m
                            }
                        }),
                    ChatType.Private, "it", ParseMode.Html, null, e.Message.From?.Username);

                result.gruppoTG = gruppoTg;
                result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                result.ExceptionMessage = ex1m;
                result.ExceptionObject = ex1;
                L.Add(result);

                return;
            }

        const string? sql2 = "SELECT id FROM GroupsTelegram WHERE GroupsTelegram.title LIKE '%' || @nome || '%'";

        if (groupId == null)
            try
            {
                if (!string.IsNullOrEmpty(gruppoTg.nome))
                {
                    var r1 = Database.ExecuteSelect(sql2, sender.DbConfig,
                        new Dictionary<string, object?> { { "@nome", gruppoTg.nome } });
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
                var ex2m = "2" + "\n\n" + ex2.Message + "\n\n" + sql2 + "\n\n" + gruppoTg.nome;
                await sender.SendTextMessageAsync(e.Message.From?.Id,
                    new Language(
                        new Dictionary<string?, string>
                        {
                            {
                                "it",
                                ex2m
                            }
                        }),
                    ChatType.Private, "it", ParseMode.Html, null, e.Message.From?.Username);

                result.gruppoTG = gruppoTg;
                result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                result.ExceptionMessage = ex2m;
                result.ExceptionObject = ex2;
                L.Add(result);

                return;
            }

        if (groupId == null)
        {
            result.gruppoTG = gruppoTg;
            result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
            L.Add(result);
        }
        else
        {
            NuovoLink? s3 = null;
            try
            {
                if (groupId != null)
                {
                    gruppoTg.UpdateId(groupId.Value);

                    s3 = await CreateInviteLinkAsync(groupId.Value, sender, e);
                    if (s3 != null) gruppoTg.UpdateNewLink(s3.link);
                }
            }
            catch (Exception? ex3)
            {
                Logger.Logger.WriteLine(ex3);
                var ex3M = "3" + "\n\n" + ex3.Message;
                await sender.SendTextMessageAsync(e.Message.From.Id,
                    new Language(
                        new Dictionary<string?, string>
                        {
                            {
                                "it",
                                ex3M
                            }
                        }),
                    ChatType.Private, "it", ParseMode.Html, null, e.Message.From.Username);

                result.gruppoTG = gruppoTg;
                result.successoGenerazioneLink = SuccessoGenerazioneLink.ERRORE;
                result.ExceptionMessage = ex3M;
                result.ExceptionObject = ex3;
                result.CreateInviteLinkFallita = true;
                L.Add(result);

                return;
            }

            var successoGenerazione = GetSuccessoGenerazione(s3);

            result.gruppoTG = gruppoTg;
            result.successoGenerazioneLink = successoGenerazione;
            L.Add(result);
        }
    }

    private static SuccessoGenerazioneLink GetSuccessoGenerazione(NuovoLink? s3)
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