﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using JsonPolimi_Core_nf.Data;
using JsonPolimi_Core_nf.Tipi;
using JsonPolimi_Core_nf.Utils;
using PoliNetworkBot_CSharp.Code.Config;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using Groups = PoliNetworkBot_CSharp.Code.Data.Constants.Groups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

internal static class CommandDispatcher
{
    public static async Task CommandDispatcherMethod(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        string?[]? cmdLines = e?.Message?.Text?.Split(' ');
        var cmd = cmdLines?[0]?.Trim();

        if (string.IsNullOrEmpty(cmd))
        {
            await DefaultCommand(sender, e);
            return;
        }

        if (cmd.Contains('@'))
        {
            var cmd2 = cmd.Split("@");
            if (sender != null)
            {
                var botUsername = await sender.GetBotUsernameAsync();
                if (!string.Equals(cmd2[1], botUsername, StringComparison.CurrentCultureIgnoreCase)) return;
            }
        }

        switch (cmd)
        {
            case "/start":
            {
                await Start(sender, e);
                return;
            }

            case "/force_check_invite_links":
            {
                if (GlobalVariables.Creators != null &&
                    GlobalVariables.Creators.ToList().Any(x => x.Matches(e?.Message?.From)))
                    _ = ForceCheckInviteLinksAsync(sender, e);
                else
                    await DefaultCommand(sender, e);
                return;
            }

            case "/contact":
            {
                await ContactUs(sender, e);
                return;
            }

            case "/help":
            {
                await Help(sender, e);
                return;
            }

            case "/muteAll":
            {
                if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                if (e?.Message?.ReplyToMessage == null)
                {
                    await CommandNeedsAReplyToMessage(sender, e);
                    return;
                }

                if (GlobalVariables.AllowedMuteAll != null &&
                    GlobalVariables.AllowedMuteAll.ToList().Any(x => x.Matches(e.Message?.From)))
                    _ = MuteAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode,
                        e.Message.From?.Username, false);
                else
                    await DefaultCommand(sender, e);
                return;
            }
            case "/unmuteAll":
            {
                if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                if (e?.Message?.ReplyToMessage == null)
                {
                    await CommandNeedsAReplyToMessage(sender, e);
                    return;
                }

                if (GlobalVariables.AllowedMuteAll != null &&
                    GlobalVariables.AllowedMuteAll.ToList().Any(x => x.Matches(e.Message?.From)))
                    _ = UnMuteAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                        false);
                else
                    await DefaultCommand(sender, e);
                return;
            }

            case "/banAll":
            {
                if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                if (e?.Message?.ReplyToMessage == null)
                {
                    await CommandNeedsAReplyToMessage(sender, e);
                    return;
                }

                if (GlobalVariables.AllowedBanAll != null &&
                    GlobalVariables.AllowedBanAll.ToList().Any(x => x.Matches(e.Message?.From)))
                    _ = BanAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                        false);
                else
                    await DefaultCommand(sender, e);
                return;
            }

            case "/banDeleteAll":
            {
                if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                if (e?.Message?.ReplyToMessage == null)
                {
                    await CommandNeedsAReplyToMessage(sender, e);
                    return;
                }

                if ((GlobalVariables.AllowedBanAll == null ||
                     !GlobalVariables.AllowedBanAll.ToList().Any(x => x.Matches(e.Message?.From))) &&
                    (GlobalVariables.Owners == null ||
                     !GlobalVariables.Owners.ToList().Any(x => x.Matches(e.Message?.From))))
                {
                    await DefaultCommand(sender, e);
                    return;
                }

                _ = BanAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                    true);
                return;
            }

            case "/del":
            {
                var reply = e?.Message?.ReplyToMessage;
                if (reply == null || sender == null || ((GlobalVariables.AllowedBanAll == null || !GlobalVariables
                        .AllowedBanAll.ToList()
                        .Any(x => e != null && x.Matches(e.Message?.From))) && (GlobalVariables.Owners == null ||
                        !GlobalVariables.Owners.ToList().Any(x => e != null && x.Matches(e.Message?.From)))))
                {
                    await DefaultCommand(sender, e);
                    return;
                }

                await sender.DeleteMessageAsync(reply.Chat.Id, reply.MessageId, null);
                return;
            }

            /*
        case "/massiveSend":
            {
                if (e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                try
                {
                    if (GlobalVariables.AllowedBanAll.Contains(e.Message.From?.Username?.ToLower()))
                        _ = MassiveSendAsync(sender, e, cmdLines, e.Message.From.LanguageCode, e.Message.From.Username);
                    else
                        await DefaultCommand(sender, e);
                }
                catch
                {
                    ;
                }

                return;
            }
            */

            case "/ban":
            {
                _ = BanUserAsync(sender, e, cmdLines, false);
                return;
            }
            /*case "/banAllHistory":
                {
                    // _ = BanUserAsync(sender, e, cmdLines);
                    _ = BanUserHistoryAsync(sender, e, false);
                    return;
                }*/

            case "/unbanAll":
            {
                if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
                {
                    await CommandNotSentInPrivateAsync(sender, e);
                    return;
                }

                if (e?.Message?.ReplyToMessage == null)
                {
                    await CommandNeedsAReplyToMessage(sender, e);
                    return;
                }

                if (GlobalVariables.AllowedBanAll != null &&
                    GlobalVariables.AllowedBanAll.ToList().Any(x => x.Matches(e.Message?.From)))
                    _ = UnbanAllAsync(sender, e, cmdLines, e.Message.From?.LanguageCode, e.Message.From?.Username,
                        false);
                else
                    await DefaultCommand(sender, e);
                return;
            }

            case "/test_spam":
            {
                if (e?.Message?.ReplyToMessage == null)
                    return;

                await TestSpamAsync(e.Message.ReplyToMessage, sender, e);
                return;
            }

            case "/groups":
            {
                await SendRecommendedGroupsAsync(sender, e);
                return;
            }

            case "/search":
            {
                var query = "";
                if (cmdLines != null)
                    for (var i = 1; i < cmdLines.Length; i++)
                        query += cmdLines[i] + " ";

                if (!string.IsNullOrEmpty(query))
                    query = query[..^1];

                _ = SendGroupsByTitle(query, sender, e, 6);

                return;
            }

            case "/reboot":
            {
                if (e is { Message: { } })
                    if (e.Message is { } && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        Reboot();
                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }

            case "/sendmessageinchannel":
            {
                var message = e?.Message;
                if (message != null && Owners.CheckIfOwner(e?.Message?.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    if (cmdLines != null && (e?.Message?.ReplyToMessage == null || cmdLines.Length != 2))
                        return;
                    var text = new Language(new Dictionary<string, string?>
                    {
                        { "it", e?.Message?.ReplyToMessage?.Text ?? e?.Message?.ReplyToMessage?.Caption }
                    });
                    var c2 = cmdLines?[1];
                    if (cmdLines == null)
                        return;

                    if (c2 != null)
                        _ = await SendMessage.SendMessageInAGroup(sender, e?.Message?.From?.LanguageCode,
                            text, e,
                            long.Parse(c2),
                            ChatType.Channel, ParseMode.Html, null, false);

                    return;
                }

                await DefaultCommand(sender, e);

                return;
            }

            case "/getGroups":
            {
                if (e != null)
                {
                    var message = e.Message;
                    if (GlobalVariables.Creators != null && message != null &&
                        (GlobalVariables.Creators.ToList().Any(x => x.Matches(e.Message?.From)) ||
                         Owners.CheckIfOwner(e.Message?.From?.Id)) && message.Chat.Type == ChatType.Private)
                    {
                        string? username = null;
                        if (!string.IsNullOrEmpty(e.Message?.From?.Username))
                            username = e.Message.From.Username;

                        if (e.Message?.From != null)
                            _ = GetAllGroups(e.Message.From.Id, username, sender, e.Message.From.LanguageCode,
                                e.Message.Chat.Type);
                        return;
                    }
                }

                await DefaultCommand(sender, e);

                return;
            }

            case "/allowmessage":
            {
                var message = e?.Message;
                if (message != null && Permissions.CheckPermissions(Permission.HEAD_ADMIN, e?.Message?.From) &&
                    message.Chat.Type == ChatType.Private)
                {
                    await AllowMessageAsync(e, sender);
                    return;
                }

                await DefaultCommand(sender, e);

                return;
            }

            case "/allowmessageowner":
            {
                var message = e?.Message;
                if (message != null && Owners.CheckIfOwner(e?.Message?.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    await AllowMessageOwnerAsync(e, sender);
                    return;
                }

                await DefaultCommand(sender, e);

                return;
            }

            case "/allowedmessages":
            {
                var o = e?.Message;
                if (o != null && Owners.CheckIfOwner(e?.Message?.From?.Id) && o.Chat.Type == ChatType.Private)
                {
                    var text = new Language(new Dictionary<string, string?>
                    {
                        { "it", "List of messages: " },
                        { "en", "List of messages: " }
                    });
                    if (sender == null) return;


                    var message1 = e?.Message;
                    if (message1 == null)
                        return;

                    await sender.SendTextMessageAsync(e?.Message?.From?.Id, text,
                        ChatType.Private,
                        e?.Message?.From?.LanguageCode, ParseMode.Html, null,
                        e?.Message?.From?.Username,
                        message1.MessageId);
                    var messages = MessagesStore.GetAllMessages(x =>
                        x != null && x.AllowedStatus.GetStatus() == MessageAllowedStatusEnum.ALLOWED);
                    if (messages == null)
                        return;

                    foreach (var m2 in messages.Select(message => message?.message)
                                 .Where(m2 => m2 != null))
                    {
                        text = new Language(new Dictionary<string, string?>
                        {
                            { "uni", m2 }
                        });
                        await sender.SendTextMessageAsync(e?.Message?.From?.Id, text,
                            ChatType.Private,
                            "uni", ParseMode.Html, null, e?.Message?.From?.Username);
                    }


                    return;
                }

                await DefaultCommand(sender, e);

                return;
            }

            case "/unallowmessage":
            {
                var message = e?.Message;
                if (message != null && Owners.CheckIfOwner(e?.Message?.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    if (e?.Message?.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
                    {
                        var text = new Language(new Dictionary<string, string?>
                        {
                            { "en", "You have to reply to a message containing the message" }
                        });
                        if (sender == null) return;
                        var o = e?.Message;
                        if (o != null)
                            await sender.SendTextMessageAsync(e?.Message?.From?.Id, text,
                                ChatType.Private,
                                e?.Message?.From?.LanguageCode, ParseMode.Html, null,
                                e?.Message?.From?.Username,
                                o.MessageId);

                        return;
                    }

                    MessagesStore.RemoveMessage(e.Message.ReplyToMessage.Text);
                    return;
                }

                await DefaultCommand(sender, e);

                return;
            }
            case "/updategroups_dry":
            {
                if (e is { Message: { } })
                    if (e.Message is { } && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, true, true, false, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/updategroups":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message?.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, false, true, false, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/updategroupsandfixnames":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, false, true, true, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/updategroupsandfixnames_dry":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        var text = await UpdateGroups(sender, true, true, true, e);

                        await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                            e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                            ParseMode.Html, null);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/backup":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        if (e.Message.From != null)
                            await BackupHandler(e.Message.From.Id, sender, e.Message.From.Username,
                                e.Message.Chat.Type);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/getrunningtime":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        try
                        {
                            var lang = new Language(new Dictionary<string, string?>
                            {
                                { "", await GetRunningTime() }
                            });
                            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                                e.Message.From?.LanguageCode,
                                e.Message.From?.Username, lang, ParseMode.Html,
                                null);
                            return;
                        }
                        catch (Exception? ex)
                        {
                            _ = NotifyUtil.NotifyOwners(ex, sender, e);
                        }

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/subscribe_log":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        await Logger.Subscribe(e.Message.From?.Id, sender, e);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/unsubscribe_log":
            {
                if (e is { Message: { } })
                    if (e.Message != null && Owners.CheckIfOwner(e.Message?.From?.Id) &&
                        e.Message!.Chat.Type == ChatType.Private)
                    {
                        Logger.Unsubscribe(e.Message.From?.Id);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/getlog":
            {
                var message = e?.Message;
                if (message != null && Owners.CheckIfOwner(e?.Message?.From?.Id) &&
                    message.Chat.Type == ChatType.Private)
                {
                    Logger.PrintLog(sender, new List<long?> { e?.Message?.From?.Id, Groups.BackupGroup }, e);

                    return;
                }

                await DefaultCommand(sender, e);

                return;
            }
            case "/getmessagesent":
            {
                if (e is { Message: { } })
                    if (Owners.CheckIfOwner(e.Message?.From?.Id)
                        && e.Message!.Chat.Type == ChatType.Private && e.Message.ReplyToMessage != null)
                    {
                        await MessagesStore.SendMessageDetailsAsync(sender, e);

                        return;
                    }

                await DefaultCommand(sender, e);

                return;
            }
            case "/testtime":
            {
                if (e?.Message == null || e.Message.Chat.Type != ChatType.Private)
                    return;

                var time = await TestTime(sender, e);
                Console.WriteLine(time);

                return;
            }

            case "/time":
            {
                if (e == null) return;
                var lang = new Language(new Dictionary<string, string?>
                {
                    { "", DateTimeClass.NowAsStringAmericanFormat() }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message?.From?.Id,
                    e.Message?.From?.LanguageCode,
                    e.Message?.From?.Username, lang, ParseMode.Html,
                    null);

                return;
            }

            case "/assoc_write":
            case "/assoc_send":
            {
                _ = await Assoc.Assoc_SendAsync(sender, e);
                return;
            }

            case "/assoc_publish":
            {
                if (Owners.CheckIfOwner(e?.Message?.From?.Id))
                    _ = await Assoc.Assoc_Publish(sender, e);
                else
                    _ = await DefaultCommand(sender, e);
                return;
            }

            case "/assoc_read":
            {
                _ = await Assoc.Assoc_Read(sender, e, false);
                return;
            }

            case "/assoc_read_all":
            {
                if (Owners.CheckIfOwner(e?.Message?.From?.Id))
                    _ = await Assoc.Assoc_ReadAll(sender, e);
                else
                    _ = await DefaultCommand(sender, e);
                return;
            }

            case "/assoc_delete":
            case "/assoc_remove":
            {
                _ = await Assoc.Assoc_Delete(sender, e);
                return;
            }

            case "/rooms":
            {
                await Rooms.RoomsMainAsync(sender, e);
                return;
            }

            case "/rules":
            {
                _ = await Rules(sender, e);
                return;
            }

            case "/qe":
            {
                _ = await QueryBot(true, e, sender);
                return;
            }

            case "/qs":
            {
                _ = await QueryBot(false, e, sender);
                return;
            }

            case "/update_links_from_json":
            {
                await InviteLinks.UpdateLinksFromJsonAsync(sender, e);
                return;
            }

            default:
            {
                await DefaultCommand(sender, e);
                return;
            }
        }
    }

    private static async Task AllowMessageOwnerAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        if (e != null)
        {
            if (e.Message?.ReplyToMessage == null || (string.IsNullOrEmpty(e.Message.ReplyToMessage.Text) &&
                                                      string.IsNullOrEmpty(e.Message.ReplyToMessage.Caption)))
            {
                var text = new Language(new Dictionary<string, string?>
                {
                    { "en", "You have to reply to a message containing the message" },
                    { "it", "You have to reply to a message containing the message" }
                });

                if (sender != null)
                    if (e.Message != null)
                        await sender.SendTextMessageAsync(e.Message?.From?.Id, text, ChatType.Private,
                            e.Message?.From?.LanguageCode, ParseMode.Html, null, e.Message?.From?.Username,
                            e.Message!.MessageId);
            }
            else
            {
                MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Text);
                MessagesStore.AllowMessageOwner(e.Message.ReplyToMessage.Caption);
                Logger.WriteLine(
                    e.Message.ReplyToMessage.Text ?? e.Message.ReplyToMessage.Caption ??
                    "Error in allowmessage, both caption and text are null");
            }
        }
    }

    private static async Task AllowMessageAsync(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        await Assoc.AllowMessage(e, sender);
    }

    public static async Task<string?> GetRunningTime()
    {
        try
        {
            using var powershell = PowerShell.Create();
            const string path = "./static/build-date.txt";
            return await File.ReadAllTextAsync(path);
        }
        catch
        {
            // ignored
        }

        return null;
    }

    private static async Task CommandNeedsAReplyToMessage(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "it", "E' necessario rispondere ad un messaggio per usare questo comando" },
            { "en", "This command only works with a reply to message" }
        });
        if (e != null)
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message?.From?.LanguageCode, e.Message?.From?.Username, e.Message?.From?.Id,
                e.Message?.From?.FirstName, e.Message?.From?.LastName, e.Message?.Chat.Id, e.Message?.Chat.Type);
    }

    private static void Reboot()
    {
        try
        {
            BackupUtil.BackupBeforeReboot();
        }
        catch
        {
            // ignored
        }

        using var powershell = PowerShell.Create();
        if (DoScript(powershell, "screen -ls", true).Aggregate("", (current, a) => current + a)
            .Contains("rebooter")) return;
        DoScript(powershell, "screen -d -m -S rebooter ./static/rebooter.sh", true);
    }

    private static async Task<object?> SendGroupsByTitle(string query, TelegramBotAbstract? sender,
        MessageEventArgs? e, int limit)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
                return null;

            var groups = Utils.Groups.GetGroupsByTitle(query, limit, sender);

            if (groups == null)
                return null;

            Logger.WriteLine("Groups search (1): " + groups.Rows.Count);

            var indexTitle = groups.Columns.IndexOf("title");
            var indexLink = groups.Columns.IndexOf("link");
            var buttons = (from DataRow row in groups.Rows
                where !string.IsNullOrEmpty(row?[indexLink].ToString()) &&
                      !string.IsNullOrEmpty(row?[indexTitle]?.ToString())
                select new InlineKeyboardButton(row[indexTitle].ToString() ?? "Error!")
                    { Url = row[indexLink].ToString() }).ToList();

            var buttonsMatrix = buttons is { Count: > 0 }
                ? KeyboardMarkup.ArrayToMatrixString(buttons)
                : null;

            Logger.WriteLine("Groups search (2): " + buttonsMatrix?.Count);

            var text2 = GetTextSearchResult(limit, buttonsMatrix);

            var inline = buttonsMatrix == null ? null : new InlineKeyboardMarkup(buttonsMatrix);

            if (e is { Message.From: { } })
                return e.Message.Chat.Type switch
                {
                    ChatType.Sender or ChatType.Private => await SendMessage.SendMessageInPrivate(sender,
                        e.Message.From.Id,
                        e.Message?.ReplyToMessage?.From?.LanguageCode ?? e.Message?.From?.LanguageCode,
                        "", text2, ParseMode.Html, e.Message?.ReplyToMessage?.MessageId, inline),
                    ChatType.Group or ChatType.Channel or ChatType.Supergroup => await SendMessage
                        .SendMessageInAGroup(
                            sender,
                            e.Message?.ReplyToMessage?.From?.LanguageCode ?? e.Message?.From?.LanguageCode,
                            text2, e,
                            e.Message!.Chat.Id, e.Message.Chat.Type,
                            ParseMode.Html, e.Message?.ReplyToMessage?.MessageId, true, 0, inline),
                    _ => throw new ArgumentOutOfRangeException()
                };

            return null;
        }
        catch (Exception? exception)
        {
            _ = NotifyUtil.NotifyOwners(exception, sender, e);
            return null;
        }
    }

    private static Language GetTextSearchResult(int limit, List<List<InlineKeyboardButton>>? buttonsMatrix)
    {
        return buttonsMatrix switch
        {
            null => new Language(new Dictionary<string, string?>
                {
                    { "en", "<b>No results</b>." },
                    { "it", "<b>Nessun risultato</b>." }
                }
            ),
            _ => limit switch
            {
                <= 0 => new Language(new Dictionary<string, string?>
                    {
                        { "en", "<b>Here are the groups </b>:" },
                        { "it", "<b>Ecco i gruppi</b>:" }
                    }
                ),
                _ => new Language(new Dictionary<string, string?>
                    {
                        { "en", "<b>Here are the groups </b> (max " + limit + "):" },
                        { "it", "<b>Ecco i gruppi</b> (max " + limit + "):" }
                    }
                )
            }
        };
    }

    public static async Task<UpdateGroupsResult> UpdateGroups(TelegramBotAbstract? sender, bool dry,
        bool debug,
        bool updateDb, MessageEventArgs? messageEventArgs)
    {
        Logger.WriteLine(
            "UpdateGroups started (dry: " + dry + ", debug: " + debug + ", updateDB: " + updateDb + ")",
            LogSeverityLevel.ALERT);
        List<ResultFixGroupsName>? x1 = null;
        if (updateDb) x1 = await Utils.Groups.FixAllGroupsName(sender, messageEventArgs);

        var groups = Utils.Groups.GetAllGroups(sender, true);

        Variabili.L = new ListaGruppo();

        Variabili.L.HandleSerializedObject(groups);

        CheckSeILinkVanno2(5, true, 10);

        var json =
            JsonBuilder.GetJson(new CheckGruppo(CheckGruppo.E.RICERCA_SITO_V3),
                false);

        if (!Directory.Exists(Paths.Data.PoliNetworkWebsiteData))
        {
            Directory.CreateDirectory(Paths.Data.PoliNetworkWebsiteData);
            InitGithubRepo();
        }

        const string path = Paths.Data.PoliNetworkWebsiteData + "/groupsGenerated.json";
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        if (dry)
        {
            Logger.WriteLine(await File.ReadAllTextAsync(path));
            var l = new Language(new Dictionary<string, string?>
            {
                { "it", "Dry run completata" },
                { "en", "Dry run completed" }
            });
            return new UpdateGroupsResult(l, x1);
        }

        using var powershell = PowerShell.Create();
        const string cd = Paths.Data.PoliNetworkWebsiteData;
        DoScript(powershell, "cd " + cd, debug);
        DoScript(powershell, "git fetch org", debug);
        DoScript(powershell, "git pull --force", debug);
        DoScript(powershell, "git add . --ignore-errors", debug);

        var commit = @"git commit -m ""[Automatic Commit] Updated Group List""" +
                     @" --author=""" + GitHubConfig.GetUser() + "<" + GitHubConfig.GetEmail() +
                     @">""";
        DoScript(powershell, commit, debug);

        var push = @"git push https://" + GitHubConfig.GetUser() + ":" +
                   GitHubConfig.GetPassword() + "@" +
                   GitHubConfig.GetRepo() + @" --all -f";
        DoScript(powershell, push, debug);

        const string hubPr =
            @"hub pull-request -m ""[AutoCommit] Groups Update"" -b PoliNetworkOrg:main -h PoliNetworkDev:main -l bot -f";

        var result = DoScript(powershell, hubPr, debug);

        powershell.Stop();

        var toBeSent = result.Aggregate("", (current, s) => current + s + "\n");

        var text = result.Count > 0
            ? new Dictionary<string, string?>
            {
                { "it", "Done \n" + toBeSent },
                { "en", "Done \n" + toBeSent }
            }
            : new Dictionary<string, string?>
            {
                { "it", "Error in execution" },
                { "en", "Error in execution" }
            };

        _ = NotifyUtil.NotifyOwners(
            "UpdateGroup result: \n" + (string.IsNullOrEmpty(toBeSent) ? "No PR created" : toBeSent), sender, null);

        var l1 = new Language(text);
        return new UpdateGroupsResult(l1, x1);
    }

    private static void CheckSeILinkVanno2(int volteCheCiRiprova, bool laPrimaVoltaControllaDaCapo,
        int waitOgniVoltaCheCiRiprova)
    {
        ParametriFunzione parametriFunzione = new();
        parametriFunzione.AddParam(volteCheCiRiprova, "volteCheCiRiprova");
        parametriFunzione.AddParam(laPrimaVoltaControllaDaCapo, "laPrimaVoltaControllaDaCapo");
        parametriFunzione.AddParam(waitOgniVoltaCheCiRiprova, "waitOgniVoltaCheCiRiprova");
        RunEventoLogged(Variabili.L.CheckSeILinkVanno, parametriFunzione);
    }

    private static void RunEventoLogged(Func<ParametriFunzione, EventoConLog> funcEvent,
        ParametriFunzione parametriFunzione)
    {
        var eventoLog = funcEvent.Invoke(parametriFunzione);
        eventoLog.RunAction();
        Logger.Log(eventoLog);
    }

    private static void InitGithubRepo()
    {
        Logger.WriteLine("Init websitedata repository");
        using var powershell = PowerShell.Create();
        DoScript(powershell, "cd ../data/", true);
        DoScript(powershell, "git clone https://" + GitHubConfig.GetRepo(), true);
        DoScript(powershell, "cd ./polinetworkWebsiteData", true);
        DoScript(powershell, "git remote add org https://" + GitHubConfig.GetRemote(), true);
    }

    public static List<string?> DoScript(PowerShell powershell, string script, bool debug)
    {
        powershell.AddScript(script);
        List<string?> listString = new();
        if (debug)
        {
            var x = "Executing command: " + script;
            Logger.WriteLine(x);
            listString.Add(x);
        }

        var results = powershell.Invoke().ToList();
        if (debug)
            foreach (var t in results)
            {
                Logger.WriteLine(t.ToString());
                listString.Add(t.ToString());
            }

        powershell.Commands.Clear();
        return listString;
    }

    public static async Task BackupHandler(long sendTo, TelegramBotAbstract? botAbstract, string? username,
        ChatType chatType)
    {
        try
        {
            var jsonDb = BackupUtil.GetDB_AsJson(botAbstract);

            if (string.IsNullOrEmpty(jsonDb)) return;

            var bytes = Encoding.UTF8.GetBytes(jsonDb);
            var stream = new MemoryStream(bytes);

            var text2 = new Language(new Dictionary<string, string?>
            {
                { "it", "Backup:" }
            });

            var peer = new PeerAbstract(sendTo, chatType);

            await SendMessage.SendFileAsync(new TelegramFile(stream, "db.json",
                    null, "application/json"), peer,
                text2, TextAsCaption.BEFORE_FILE,
                botAbstract, username, "it", null, true);
        }
        catch (Exception? ex)
        {
            await NotifyUtil.NotifyOwners(ex, botAbstract, null);
        }
    }

    private static async Task TestSpamAsync(Message message, TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message != null)
        {
            var r2 = MessagesStore.StoreAndCheck(e.Message.ReplyToMessage);

            if (r2 is not (SpamType.SPAM_PERMITTED or SpamType.SPAM_LINK))
                r2 = await Blacklist.IsSpam(message.Text, message.Chat.Id, sender, true, e);

            var dict = new Dictionary<string, string?>
            {
                { "en", r2.ToString() }
            };
            var text = new Language(dict);
            try
            {
                if (e.Message.From != null)
                    if (sender != null)
                        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, "en",
                            ParseMode.Html,
                            null, null);
            }
            catch
            {
                // ignored
            }
        }
    }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    private static async Task<object> MassiveSendAsync(TelegramBotAbstract? sender, MessageEventArgs? e,
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        string textToSend)
    {
        /*

        textToSend =        "Buonasera a tutti, vi ricordiamo che lunedì 24 fino al 27 verranno aperti i seggi online per le elezioni, fate sentire la vostra voce mi raccomando. <b>Votate!</b>\nPotete informarvi su modalità di voto e candidati al sito\npolinetworkelezioni.github.io/it" +
                "\n\n\n" +
                "Good evening everyone, we remind you that on Monday 24th to 27th the online polling stations will be open for the elections, please let your voice be heard. <b>Vote!</b>\nYou can find out about voting procedures and candidates in the website\npolinetworkelezioni.github.io/en"

         */

        var groups = Database.ExecuteSelect("Select id FROM GroupsTelegram", sender?.DbConfig);

        if (groups?.Rows == null || groups.Rows.Count == 0)
        {
            var dict = new Dictionary<string, string?> { { "en", "No groups!" } };
            if (e?.Message?.From != null)
                if (sender != null)
                    await sender.SendTextMessageAsync(e.Message.From.Id, new Language(dict), ChatType.Private,
                        e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                        e.Message.MessageId);
        }

        var counter = 0;

        var dict2 = new Dictionary<string, string?>
        {
            {
                "en",
                textToSend
            }
        };

        try
        {
            var g1 = groups?.Rows;
            if (g1 != null)
                foreach (DataRow element in g1)
                {
                    try
                    {
                        var groupId = Convert.ToInt64(element.ItemArray[0]);

                        try
                        {
                            await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), e, groupId,
                                ChatType.Supergroup, ParseMode.Html, null, default);
                            counter++;
                        }
                        catch
                        {
                            try
                            {
                                await SendMessage.SendMessageInAGroup(sender, "en", new Language(dict2), e,
                                    groupId,
                                    ChatType.Group, ParseMode.Html, null, default);
                                counter++;
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    await Task.Delay(500);
                }
        }
        catch
        {
            // ignored
        }

        var text = new Language(new Dictionary<string, string?>
        {
            { "en", "Sent in  " + counter + " groups" }
        });

        await Task.Delay(500);

        if (e?.Message?.From == null) return true;
        if (sender != null)
            await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                e.Message.From.LanguageCode,
                ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
        return true;
    }

#pragma warning disable CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    private static async Task<List<long>> BanUserHistoryAsync(TelegramBotAbstract sender, long idGroup)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
#pragma warning restore CS1998 // Il metodo asincrono non contiene operatori 'await', pertanto verrà eseguito in modo sincrono
    {
        const string? queryForBannedUsers =
            "SELECT * from Banned as B1 WHERE when_banned >= (SELECT MAX(B2.when_banned) from Banned as B2 where B1.target = B2.target) and banned_true_unbanned_false = 83";
        var bannedUsers = Database.ExecuteSelect(queryForBannedUsers, sender.DbConfig);
        if (bannedUsers == null)
            return new List<long>();
        var bannedUsersId = bannedUsers.Rows[bannedUsers.Columns.IndexOf("target")].ItemArray;
        var bannedUsersIdArray = bannedUsersId.Select(user =>
        {
            var r1 = user?.ToString();
            return r1 != null ? long.Parse(r1) : 0;
        }).ToList();

        return bannedUsersIdArray;
    }

    /*
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati
    private static async Task<object> BanUserHistoryAsync(TelegramBotAbstract sender, MessageEventArgs e,
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
        bool? revokeMessage)
    {
        var r = Owners.CheckIfOwner(e.Message.From.Id);
        if (!r) return r;

        var query = "SELECT DISTINCT T1.target FROM " +
                    "(SELECT * FROM Banned WHERE banned_true_unbanned_false = 83 ORDER BY when_banned DESC) AS T1, " +
                    "(SELECT * FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC) AS T2 " +
                    "WHERE (T1.target = T2.target AND T1.when_banned >= T2.when_banned AND T1.target IN (SELECT target FROM(SELECT target FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC))) OR (T1.target NOT IN (SELECT target FROM (SELECT target FROM Banned WHERE banned_true_unbanned_false != 83 ORDER BY when_banned DESC)))";
        var x = SqLite.ExecuteSelect(query);

        if (x == null || x.Rows == null || x.Rows.Count == 0)
        {
            var text3 = new Language(new Dictionary<string, string?>
            {
                {"en", "There are no users to ban!"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text3, ChatType.Private,
                e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            return false;
        }

        var groups = SqLite.ExecuteSelect("Select id FROM GroupsTelegram");
        /*
        if(e.Message.Text.Length !=10)
        {
            Language text2 = new Language(new Dictionary<string, string?>() {
                {"en", "Group not found (1)!"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private, e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId, false);
            return false;
        }

        var counter = 0;
        var channel = Regex.Match(e.Message.Text, @"\d+").Value;
        if (groups.Select("id = " + "'" + channel + "'").Length != 1)
        {
            var text2 = new Language(new Dictionary<string, string?>
            {
                {"en", "Group not found! (2)"}
            });
            await sender.SendTextMessageAsync(e.Message.From.Id, text2, ChatType.Private,
                e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
            return false;
        }

        foreach (DataRow element in x.Rows)
        {
            var userToBeBanned = Convert.ToInt64(element.ItemArray[0]);
            await RestrictUser.BanUserFromGroup(sender, userToBeBanned, Convert.ToInt64(channel), null,
                revokeMessage);
            counter++;
        }

        var text = new Language(new Dictionary<string, string?>
        {
            {"en", "Banned " + counter + " in group: " + groups.Select(channel)}
        });
        await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private, e.Message.From.LanguageCode,
            ParseMode.Html, null, e.Message.From.Username, e.Message.MessageId);
        return true;
    }
    */

    private static async Task<long?> QueryBot(bool executeTrueSelectFalse, MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        if (e?.Message?.ForwardFrom != null)
            return null;

        if (e?.Message?.From == null)
            return null;

        if (GlobalVariables.IsOwner(e.Message.From.Id))
            return await QueryBot2(executeTrueSelectFalse, e, sender);

        return null;
    }

    private static async Task<long?> QueryBot2(bool executeTrueSelectFalse, MessageEventArgs? e,
        TelegramBotAbstract? sender)
    {
        if (e == null) return null;
        if (e.Message?.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
        {
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "You have to reply to a message containing the query" }
            });
            if (e.Message?.From == null) return null;
            if (sender != null)
                await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                    e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                    e.Message.MessageId);
            return null;
        }

        var query = e.Message.ReplyToMessage.Text;
        if (executeTrueSelectFalse)
            if (sender != null)
            {
                var i = Database.Execute(query, sender.DbConfig);

                var text = new Language(new Dictionary<string, string?>
                {
                    { "en", "Query execution. Result: " + i }
                });
                if (e.Message.From != null)
                    await sender.SendTextMessageAsync(e.Message.From.Id, text, ChatType.Private,
                        e.Message.From.LanguageCode, ParseMode.Html, null, e.Message.From.Username,
                        e.Message.MessageId);
                return i;
            }


        if (sender == null) return null;
        var x = Database.ExecuteSelect(query, sender.DbConfig);
        var x2 = StreamSerialization.SerializeToStream(x);
        var documentInput =
            new TelegramFile(x2, "table.bin", "Query result", "application/octet-stream");

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "Query result" }
        });
        if (e.Message.From == null)
            return -1;

        PeerAbstract peer = new(e.Message.From.Id, e.Message.Chat.Type);
        var v = await sender.SendFileAsync(documentInput, peer, text2, TextAsCaption.AS_CAPTION,
            e.Message.From.Username, e.Message.From.LanguageCode, e.Message.MessageId, false);
        return v ? 1 : 0;
    }

    private static async Task<MessageSentResult?> TestTime(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message?.From == null)
            return null;

        if (e.Message.Text == null) return null;
        var tuple1 = await AskUser.AskDateAsync(e.Message.From.Id,
            e.Message.Text,
            e.Message.From.LanguageCode, sender, e.Message.From.Username);

        var exception = tuple1?.Item2;
        if (exception != null)
        {
            var s = tuple1?.Item3;
            await NotifyUtil.NotifyOwners(new ExceptionNumbered(exception), sender, e, 0, s);

            return null;
        }

        var dateTimeSchedule = tuple1?.Item1;
        if (dateTimeSchedule == null) return null;
        var sentDate2 = dateTimeSchedule.GetDate();

        var dict = new Dictionary<string, string?>
        {
            { "en", DateTimeClass.DateTimeToItalianFormat(sentDate2) }
        };
        var text = new Language(dict);
        return await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
            e.Message.From.LanguageCode, e.Message.From.Username,
            text, ParseMode.Html, e.Message.MessageId);
    }

    private static async Task<MessageSentResult?> Rules(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        const string text = "Ecco le regole!\n" +
                            "https://polinetwork.org/it/rules";

        const string textEng = "Here are the rules!\n" +
                               "https://polinetwork.org/en/rules";

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng },
            { "it", text }
        });

        return await SendMessage.SendMessageInPrivate(sender, e?.Message?.From?.Id,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, text2, ParseMode.Html, null);
    }

    private static async Task SendRecommendedGroupsAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        const string text = "<i>Lista di gruppi consigliati</i>:\n" +
                            "\n👥 Gruppo di tutti gli studenti @PoliGruppo 👈\n" +
                            "\n📖 Libri @PoliBook\n" +
                            "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                            "\n🥳 Eventi @PoliEventi\n" +
                            "\nRicordiamo che sul nostro sito vi sono tutti i link ai gruppi con tanto ricerca, facci un salto!\n" +
                            "https://polinetwork.github.io/";

        const string textEng = "<i>List of recommended groups</i>:\n" +
                               "\n👥 Group with all students @PoliGruppo 👈\n" +
                               "\n📖 Books @PoliBook\n" +
                               "\n🤪 Spotted & Memes @PolimiSpotted @PolimiMemes\n" +
                               "\n🥳 Events @PoliEventi\n" +
                               "\nWe remind you that on our website there are all link to the groups, and they are searchable, have a look!\n" +
                               "https://polinetwork.github.io/";

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng },
            { "it", text }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message?.From?.Id,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, text2, ParseMode.Html, null);
    }

    private static async Task<bool> GetAllGroups(long chatId, string? username, TelegramBotAbstract? sender,
        string? lang, ChatType chatType)
    {
        var groups = Utils.Groups.GetAllGroups(sender);
        Stream? stream = new MemoryStream();
        FileSerialization.SerializeFile(groups, ref stream);

        var peer = new PeerAbstract(chatId, chatType);

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "Here are all groups:" },
            { "it", "Ecco tutti i gruppi:" }
        });
        return await SendMessage.SendFileAsync(new TelegramFile(stream, "groups.bin",
                null, "application/octet-stream"), peer,
            text2, TextAsCaption.BEFORE_FILE,
            sender, username, lang, null, true);
    }

    private static async Task<SuccessWithException?> BanUserAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e,
        string?[]? stringInfo, bool? revokeMessage)
    {
        if (e?.Message?.From != null)
        {
            var r =
                await Utils.Groups.CheckIfAdminAsync(e.Message.From.Id, e.Message.From.Username, e.Message.Chat.Id,
                    sender);
            if (r != null && !r.IsSuccess()) return r;
        }

        if (e?.Message?.ReplyToMessage == null)
        {
            var userIdFound = await Info.GetTargetUserIdAsync(stringInfo?[1], sender);
            if (userIdFound == null)
            {
                var e2 = new Exception("Can't find userid (1)");
                await NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender, e);
                return new SuccessWithException(false, e2);
            }

            var targetId = userIdFound.GetId();
            if (targetId != null && e?.Message != null)
                return await RestrictUser.BanUserFromGroup(sender, targetId.Value, e.Message.Chat.Id, null,
                    revokeMessage);

            var e3 = new Exception("Can't find userid (2)");
            await NotifyUtil.NotifyOwners(new ExceptionNumbered(e3), sender, e);
            return new SuccessWithException(false, e3);
        }

        var targetInt = e.Message.ReplyToMessage.From?.Id;

        NotifyUtil.NotifyOwnersBanAction(sender, e, targetInt, e.Message.ReplyToMessage.From?.Username);

        return await RestrictUser.BanUserFromGroup(sender, targetInt, e.Message.Chat.Id, stringInfo,
            revokeMessage);
    }

    private static async Task<SuccessWithException> UnbanAllAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e, IReadOnlyList<string?>? target, string? lang,
        string? username,
        bool? revokeMessage)
    {
        return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username,
            RestrictAction.UNBAN, revokeMessage);
    }

    private static async Task<SuccessWithException> BanAllAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e,
        IReadOnlyList<string?>? target, string? lang, string? username, bool? revokeMessage)
    {
        return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, RestrictAction.BAN,
            revokeMessage);
    }

    private static async Task<SuccessWithException> MuteAllAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e, IReadOnlyList<string?>? target, string? lang,
        string? username,
        bool? revokeMessage)
    {
        return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, RestrictAction.MUTE,
            revokeMessage);
    }

    private static async Task<SuccessWithException> UnMuteAllAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e, IReadOnlyList<string?>? target, string? lang,
        string? username,
        bool? revokeMessage)
    {
        return await BanAllUnbanAllMethod1Async2Async(sender, e, target, lang, username, RestrictAction.UNMUTE,
            revokeMessage);
    }

    private static async Task<SuccessWithException> BanAllUnbanAllMethod1Async2Async(TelegramBotAbstract? sender,
        MessageEventArgs? e,
        IReadOnlyList<string?>? target, string? lang, string? username, RestrictAction ban,
        bool? revokeMessage)
    {
        var d1 = GetDateTime(target);
        try
        {
            await BanAllUnbanAllMethod1Async(ban, GetFinalTargetForRestrictAll(target), e, sender, lang,
                username,
                d1?.GetValue(), revokeMessage);
            return new SuccessWithException(true, d1?.GetExceptions());
        }
        catch (Exception? ex)
        {
            var ex2 = Concat(ex, d1);
            return new SuccessWithException(false, ex2);
        }
    }

    private static List<Exception?> Concat(Exception? ex, ValueWithException<DateTime?>? d1)
    {
        var r = new List<Exception?>
        {
            ex
        };
        var d2 = d1?.GetExceptions();
        if (d1 == null || !d1.ContainsExceptions())
            return r;
        if (d2 != null)
            r.AddRange(d2);

        return r;
    }

    private static ValueWithException<DateTime?>? GetDateTime(IReadOnlyList<string?>? target)
    {
        if (target == null)
            return null;
        if (target.Count < 3)
            return null;

        var s = "";
        for (var i = 2; i < target.Count; i++) s += target[i] + " ";

        s = s.Trim();
        return DateTimeClass.GetFromString(s);
    }

    private static async Task BanAllUnbanAllMethod1Async(RestrictAction restrictAction,
        string? finalTarget,
        MessageEventArgs? e, TelegramBotAbstract? sender, string? lang, string? username, DateTime? until,
        bool? revokeMessage)
    {
        if (string.IsNullOrEmpty(finalTarget))
        {
            var lang2 = new Language(new Dictionary<string, string?>
            {
                { "en", "We can't find the target." },
                { "it", "Non riusciamo a trovare il bersaglio" }
            });
            if (sender != null)
                await sender.SendTextMessageAsync(e?.Message?.From?.Id, lang2, ChatType.Private,
                    lang, ParseMode.Html, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

            return;
        }

        if (string.IsNullOrEmpty(e?.Message?.ReplyToMessage?.Text))
        {
            var lang2 = new Language(new Dictionary<string, string?>
            {
                { "en", "The replied message cannot be empty!" },
                { "it", "Il messaggio a cui rispondi non può essere vuoto" }
            });
            if (e?.Message?.From == null) return;
            if (sender != null)
                await sender.SendTextMessageAsync(e.Message.From.Id, lang2, ChatType.Private,
                    lang, ParseMode.Html, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

            return;
        }

        var done =
            await RestrictUser.BanAllAsync(sender, e, finalTarget, restrictAction, until, revokeMessage);
        var text2 = done?.Item1.GetLanguage(restrictAction, finalTarget, done.Item3);

        NotifyUtil.NotifyOwnersBanAction(sender, e, restrictAction, done, finalTarget,
            e.Message.ReplyToMessage.Text);

        if (e.Message.From != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                ParseMode.Html,
                e.Message.MessageId);

        await SendReportOfSuccessAndFailures(sender, e, done);
    }

    private static async Task SendReportOfSuccessAndFailures(TelegramBotAbstract? sender, MessageEventArgs? e,
        Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long>? done)
    {
        try
        {
            if (done != null)
            {
                var (banUnbanAllResult, _, _) = done;
                await SendReportOfSuccessAndFailures2(
                    StreamSerialization.SerializeToStream(banUnbanAllResult.GetSuccess()),
                    "success.bin", sender, e);
                await SendReportOfSuccessAndFailures2(
                    StreamSerialization.SerializeToStream(banUnbanAllResult.GetFailed()),
                    "failed.bin", sender, e);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static async Task SendReportOfSuccessAndFailures2(Stream? stream, string filename,
        TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var file = new TelegramFile(stream, filename, "", "application/octet-stream");
        var message = e?.Message;
        if (message != null)
        {
            var peer = new PeerAbstract(e?.Message?.From?.Id, message.Chat.Type);
            var text = new Language(new Dictionary<string, string?>
            {
                { "en", "" }
            });
            await SendMessage.SendFileAsync(file, peer, text, TextAsCaption.AS_CAPTION,
                sender, e?.Message?.From?.Username, e?.Message?.From?.LanguageCode,
                null, true);
        }
    }

    private static string? GetFinalTargetForRestrictAll(IReadOnlyList<string?>? target)
    {
        return target?[1];
    }

    private static async Task<bool> DefaultCommand(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var text2 = new Language(new Dictionary<string, string?>
        {
            {
                "en",
                "I'm sorry, but I don't know this command. Try to ask the administrators (/contact)"
            },
            {
                "it",
                "Mi dispiace, ma non conosco questo comando. Prova a contattare gli amministratori (/contact)"
            }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message?.From?.Id,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, text2,
            ParseMode.Html,
            null);

        return true;
    }

    private static async Task Help(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message != null && e.Message.Chat.Type == ChatType.Private)
            await HelpPrivate(sender, e);
        else
            await CommandNotSentInPrivateAsync(sender, e);
    }

    private static async Task CommandNotSentInPrivateAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var lang = new Language(new Dictionary<string, string?>
        {
            { "it", "Questo messaggio funziona solo in chat privata" },
            { "en", "This command only works in private chat with me" }
        });
        if (e != null)
        {
            await SendMessage.SendMessageInPrivateOrAGroup(sender,
                lang, e.Message?.From?.LanguageCode, e.Message?.From?.Username, e.Message?.From?.Id,
                e.Message?.From?.FirstName, e.Message?.From?.LastName, e.Message?.Chat.Id, e.Message?.Chat.Type);

            if (e.Message != null)
                if (sender != null)
                    await sender.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
        }
    }

    private static async Task HelpPrivate(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        const string text = "<i>Lista di funzioni</i>:\n" +
                            //"\n📑 Sistema di recensioni dei corsi (per maggiori info /help_review)\n" +
                            //"\n🔖 Link ai materiali nei gruppi (per maggiori info /help_material)\n" +
                            "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                            "FAQ (domande frequenti)</a>\n" +
                            "\n🏫 Ricerca aule libere /rooms\n" +
                            //"\n🕶️ Sistema di pubblicazione anonima (per maggiori info /help_anon)\n" +
                            //"\n🎙️ Registrazione delle lezioni (per maggiori info /help_record)\n" +
                            "\n👥 Gruppo consigliati e utili /groups\n" +
                            "\n⚠ Hai già letto le regole del network? /rules\n" +
                            "\n✍ Per contattarci /contact";

        const string textEng = "<i>List of features</i>:\n" +
                               //"\n📑 Review system of courses (for more info /help_review)\n" +
                               //"\n🔖 Link to notes (for more info /help_material)\n" +
                               "\n🙋 <a href='https://polinetwork.org/it/faq/index.html'>" +
                               "FAQ (frequently asked questions)</a>\n" +
                               "\n🏫 Find free rooms /rooms\n" +
                               //"\n🕶️ Anonymous posting system (for more info /help_anon)\n" +
                               //"\n🎙️ Record of lessons (for more info /help_record)\n" +
                               "\n👥 Recommended groups /groups\n" +
                               "\n⚠ Have you already read our network rules? /rules\n" +
                               "\n✍ To contact us /contact";

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", textEng },
            { "it", text }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message?.From?.Id,
            e?.Message?.From?.LanguageCode,
            e?.Message?.From?.Username, text2, ParseMode.Html, null);
    }

    private static async Task ContactUs(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e?.Message);
        if (telegramBotClient != null)
        {
            var lang2 = new Language(new Dictionary<string, string?>
            {
                { "it", telegramBotClient.GetContactString() },
                { "en", telegramBotClient.GetContactString() }
            });
            await telegramBotClient.SendTextMessageAsync(e?.Message?.Chat.Id,
                lang2, e?.Message?.Chat.Type, e?.Message?.From?.LanguageCode,
                ParseMode.Html,
                new ReplyMarkupObject(ReplyMarkupEnum.REMOVE), e?.Message?.From?.Username
            );
        }
    }

    private static async Task ForceCheckInviteLinksAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        long? n = null;
        try
        {
            n = await InviteLinks.FillMissingLinksIntoDB_Async(sender, e);
        }
        catch (Exception? e2)
        {
            await NotifyUtil.NotifyOwners(new ExceptionNumbered(e2), sender, e);
        }

        if (n == null)
            return;

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", "I have updated n=" + n + " links" },
            { "it", "Ho aggiornato n=" + n + " link" }
        });
        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message?.From?.Id,
                e.Message?.From?.LanguageCode,
                e.Message?.From?.Username, text2,
                ParseMode.Html,
                e.Message?.MessageId);
    }

    private static async Task Start(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        await DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, e?.Message);
        var lang2 = new Language(new Dictionary<string, string?>
        {
            {
                "it", "Ciao! 👋\n" +
                      "\nScrivi /help per la lista completa delle mie funzioni 👀\n" +
                      "\nVisita anche il nostro sito " + telegramBotClient?.GetWebSite()
            },
            {
                "en", "Hi! 👋\n" +
                      "\nWrite /help for the complete list of my functions👀\n" +
                      "\nAlso visit our site " + telegramBotClient?.GetWebSite()
            }
        });
        if (telegramBotClient != null)
            await telegramBotClient.SendTextMessageAsync(e?.Message?.Chat.Id,
                lang2,
                e?.Message?.Chat.Type, replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE),
                lang: e?.Message?.From?.LanguageCode, username: e?.Message?.From?.Username, parseMode: ParseMode.Html
            );
    }

    public static void BanMessageActions(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        NotifyUtil.NotifyOwnersBanAction(telegramBotClient, e, e?.Message?.LeftChatMember?.Id,
            e?.Message?.LeftChatMember?.Username);
    }
}