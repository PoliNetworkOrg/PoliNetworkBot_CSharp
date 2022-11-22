#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using JsonPolimi_Core_nf.Tipi;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Enums.Log;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class Groups
{
    private static readonly Dictionary<long, InfoChat?> GroupsInRam = new();

    public static async Task<GetGroupsAndFixNamesResult> GetGroupsAndFixNames(TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        var s1 = await FixAllGroupsName(telegramBotAbstract, messageEventArgs);
        var s2 = GetAllGroups(telegramBotAbstract);
        return new GetGroupsAndFixNamesResult(s1, s2);
    }

    internal static DataTable? GetAllGroups(TelegramBotAbstract? sender, bool onlyValids = false)
    {
        var q1 = onlyValids
            ? "SELECT * FROM GroupsTelegram WHERE ( valid = 'Y' or valid = 1 )"
            : "SELECT * FROM GroupsTelegram";

        return Database.ExecuteSelect(q1, sender?.DbConfig);
    }

    internal static DataTable? GetGroupsByTitle(string query, int limit, TelegramBotAbstract? sender)
    {
        const string? q1 = "SELECT id,title,link " +
                           "FROM GroupsTelegram " +
                           "WHERE title LIKE @title " +
                           "AND ( valid = 'Y' or valid = 1 ) LIMIT @limit";
        var seo = query.Split(" ");
        var query2 = seo.Aggregate("", (current, word) => current + ('%' + word));
        query2 += "%";
        return Database.ExecuteSelect(q1, sender?.DbConfig,
            new Dictionary<string, object?> { { "@title", query2 }, { "@limit", limit } });
    }

    internal static async Task<SuccessWithException?> CheckIfAdminAsync(long userId, string? username, long chatId,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (GlobalVariables.Creators != null && GlobalVariables.Creators.ToList().Any(x => x.Matches(userId, username)))
            return new SuccessWithException(true);

        if (telegramBotAbstract != null)
        {
            var s1 = await telegramBotAbstract.IsAdminAsync(userId, chatId);
            if (s1 != null && s1.IsSuccess())
                return s1;
        }

        if (GlobalVariables.Owners != null && GlobalVariables.Owners.ToList().Any(x => x.Matches(userId, username)))
            return new SuccessWithException(true);

        return null;
    }

    public static async Task<List<ResultFixGroupsName>> FixAllGroupsName(TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        var r = new List<ResultFixGroupsName>();
        try
        {
            Logger.Logger.WriteLine("Starting fix of groups name");
            const string? q1 = "SELECT * FROM GroupsTelegram";
            var groups = Database.ExecuteSelect(q1, telegramBotAbstract?.DbConfig);
            if (groups != null)
            {
                var indexTitle = groups.Columns.IndexOf("title");
                var indexId = groups.Columns.IndexOf("id");
                for (var i = 0; i < groups.Rows.Count; i++)
                {
                    var groupsRow = groups.Rows[i];
                    var o = groupsRow[indexId];
                    var indexIdInTable = (long)o;
                    var o1 = groupsRow[indexTitle];
                    var oldTitle = (string)o1;
                    var newTitle = "";
                    try
                    {
                        await Task.Delay(300);
                        Tuple<Chat?, Exception?>? newTitleWithException = null;
                        var e = 0;
                        while ((newTitleWithException == null ||
                                newTitleWithException.Item2 is ApiRequestException)
                               && e < 3)
                        {
                            if (telegramBotAbstract != null)
                                newTitleWithException = await telegramBotAbstract.GetChat(indexIdInTable);
                            if (newTitleWithException is { Item2: ApiRequestException })
                                await Task.Delay(1000 * 60 * 5);
                            e++;
                        }

                        newTitle = newTitleWithException?.Item1?.Title;
                        var s1 = GroupCheckAndUpdate(indexIdInTable, oldTitle, newTitleWithException,
                            telegramBotAbstract);
                        r.Add(new ResultFixGroupsName(s1, groupsRow));
                    }
                    catch (Exception e2)
                    {
                        var e3 = new Exception("Unexpected exception in FixAllGroupsName \n\noldTitle: " +
                                               oldTitle +
                                               "\n NewTitle: " + newTitle + "\n\n" + e2);
                        await NotifyUtil.NotifyOwnerWithLog2(e3, telegramBotAbstract,
                            EventArgsContainer.Get(messageEventArgs));
                    }
                }
            }

            GroupsFixLog.SendLog(telegramBotAbstract, EventArgsContainer.Get(messageEventArgs));
        }
        catch (Exception? e)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e, telegramBotAbstract, EventArgsContainer.Get(messageEventArgs));
        }

        return r;
    }

    internal static void CheckForGroupUpdate(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        try
        {
            if (e != null && e.Message.Chat.Title == null)
                return;

            if (e?.Message != null)
            {
                var groupsFixLogUpdatedEnum = CheckForGroupUpdateAsync2(e.Message.Chat, telegramBotClient);

                if (groupsFixLogUpdatedEnum == GroupsFixLogUpdatedEnum.NEW_NAME)
                    GroupsFixLog.SendLog(telegramBotClient, EventArgsContainer.Get(e),
                        GroupsFixLogUpdatedEnum.NEW_NAME);
            }

            _ = CheckIfInviteIsWorking(e, telegramBotClient);
        }
        catch (Exception? ex)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(ex, telegramBotClient, EventArgsContainer.Get(e));
        }
    }

    private static async Task CheckIfInviteIsWorking(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient)
    {
        try
        {
            if (e != null && e.Message.Chat.Title == null)
                return;
            InfoChat? infoChat = null;
            bool? getDone;

            lock (GroupsInRam)
            {
                getDone = e?.Message != null && GroupsInRam.TryGetValue(e.Message.Chat.Id, out infoChat);
            }

            if (infoChat != null && getDone != null && getDone.Value)
            {
                if (infoChat.IsInhibited()) return;
            }
            else
            {
                if (e?.Message != null)
                {
                    infoChat = new InfoChat(e.Message.Chat, DateTime.Now);
                    lock (GroupsInRam)
                    {
                        GroupsInRam[e.Message.Chat.Id] = infoChat;
                    }
                }
            }


            if (telegramBotClient != null)
                if (e?.Message != null)
                {
                    var groups = Database.ExecuteSelect(Query.SelectGroupsTelegramWhereId, telegramBotClient.DbConfig,
                        new Dictionary<string, object?> { { "@id", e.Message.Chat.Id } }, ToLog.NO);
                    if (groups != null && groups.Rows.Count == 0)
                        throw new Exception("No group found with id: " + e.Message.Chat.Id +
                                            " while running CheckForGroupTitleUpdateAsync");
                    if (groups != null)
                    {
                        var indexId = groups.Columns.IndexOf("id");
                        var indexIdInTable = (long)groups.Rows[0][indexId];

                        var g = ListaGruppo.CreaGruppo(groups.Rows[0]);

                        var linkFunzionante = g.CheckSeIlLinkVa(false, null);

                        if (linkFunzionante != null && !linkFunzionante.Value)
                        {
                            var nuovoLink =
                                await InviteLinks.CreateInviteLinkAsync(indexIdInTable, telegramBotClient, e);
                            if (nuovoLink != null && nuovoLink.IsNuovo != SuccessoGenerazioneLink.ERRORE)
                                await NotifyUtil.NotifyOwners_AnError_AndLog3(
                                    "Fixed link for group " + e.Message.Chat.Title + " id: " + e.Message.Chat.Id,
                                    telegramBotClient, EventArgsContainer.Get(e), FileTypeJsonEnum.SIMPLE_STRING,
                                    SendActionEnum.SEND_TEXT);
                        }
                    }
                }

            infoChat?.UpdateTimeOfLastLinkCheck();
        }
        catch (Exception? ex)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(ex, telegramBotClient, EventArgsContainer.Get(e));
        }
    }

    private static GroupsFixLogUpdatedEnum CheckForGroupUpdateAsync2(Chat group, TelegramBotAbstract? sender)
    {
        InfoChat? telegramGroup;
        bool? groupInRamGetDone;
        lock (GroupsInRam)
        {
            groupInRamGetDone = GroupsInRam.TryGetValue(group.Id, out telegramGroup);
        }

        if (telegramGroup != null && groupInRamGetDone.Value)
        {
            if (group.Title == telegramGroup.Chat.Title) return GroupsFixLogUpdatedEnum.DID_NOTHING;
            lock (GroupsInRam)
            {
                GroupsInRam.Remove(group.Id);
                GroupsInRam.Add(group.Id, new InfoChat(group, DateTime.Now));
            }

            return GroupCheckAndUpdate2(group.Id, group.Title, telegramGroup.Chat.Title, sender);
        }


        var groups = Database.ExecuteSelect(Query.SelectGroupsTelegramWhereId, sender?.DbConfig,
            new Dictionary<string, object?> { { "@id", group.Id } }, ToLog.NO);
        if (groups != null && groups.Rows.Count == 0)
            throw new Exception("No group found with id: " + group.Id +
                                " while running CheckForGroupTitleUpdateAsync");

        var row = groups?.Rows[0];

        lock (GroupsInRam)
        {
            if (!GroupsInRam.ContainsKey(group.Id))
            {
                GroupsInRam[group.Id] = new InfoChat(group, DateTime.Now);
            }
            else
            {
                var infoChat = GroupsInRam[group.Id];
                if (infoChat != null) infoChat.Chat = group;
            }
        }

        if (groups == null) return GroupsFixLogUpdatedEnum.DID_NOTHING;
        var indexTitle = groups.Columns.IndexOf("title");
        if (row == null) return GroupsFixLogUpdatedEnum.DID_NOTHING;
        var oldTitle = (string)row[indexTitle];

        return oldTitle != group.Title
            ? GroupCheckAndUpdate2(group.Id, group.Title, oldTitle, sender)
            : GroupsFixLogUpdatedEnum.DID_NOTHING;
    }

    private static GroupsFixLogUpdatedEnum GroupCheckAndUpdate(long indexIdInTable,
        string? oldTitle,
        Tuple<Chat?, Exception?>? newTitleWithException, TelegramBotAbstract? sender)
    {
        var newTitle = newTitleWithException?.Item1?.Title;
        if (string.IsNullOrEmpty(oldTitle) && string.IsNullOrEmpty(newTitle))
        {
            GroupsFixLog.OldNullNewNull(newTitleWithException?.Item1?.Id, indexIdInTable);
            GroupsFixLog.CountIgnored();
            return GroupsFixLogUpdatedEnum.DID_NOTHING;
        }

        if (string.IsNullOrEmpty(newTitle))
        {
            GroupsFixLog.NewNull(indexIdInTable, oldTitle, newTitleWithException?.Item2);
            GroupsFixLog.CountIgnored();
            return GroupsFixLogUpdatedEnum.DID_NOTHING;
        }

        if (string.IsNullOrEmpty(oldTitle))
        {
            GroupsFixLog.OldNull(newTitle);
            oldTitle = "[previously null]";
        }

        if (oldTitle != newTitle) return GroupCheckAndUpdate2(indexIdInTable, newTitle, oldTitle, sender);
        GroupsFixLog.CountIgnored();
        return GroupsFixLogUpdatedEnum.DID_NOTHING;
    }

    private static GroupsFixLogUpdatedEnum GroupCheckAndUpdate2(long id, string? newTitle, string? oldTitle,
        TelegramBotAbstract? sender)
    {
        const string? q = "UPDATE GroupsTelegram SET title = @title WHERE id = @id";

        var d = new Dictionary<string, object?>
        {
            { "@title", newTitle },
            { "@id", id }
        };
        Database.Execute(q, sender?.DbConfig, d);
        GroupsFixLog.NameChange(oldTitle, newTitle);
        return GroupsFixLogUpdatedEnum.NEW_NAME;
    }

    internal static async Task SendMessageExitingAndThenExit(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e)
    {
        try
        {
            if (e?.Message != null)
                switch (e.Message.Chat.Type)
                {
                    case ChatType.Group:
                    case ChatType.Supergroup:
                    {
                        try
                        {
                            Dictionary<string, string?> dict = new()
                            {
                                {
                                    "it",
                                    "Il bot non è autorizzato in questo gruppo. Contattare gli amministratori di PoliNetwork."
                                },
                                {
                                    "en",
                                    "The bot is not authorized in this group. Contact the PoliNetwork administrators."
                                }
                            };
                            Language lang = new(dict);

                            await SendMessage.SendMessageInAGroup(
                                telegramBotClient, e.Message.From?.LanguageCode, lang, EventArgsContainer.Get(e),
                                e.Message.Chat.Id, e.Message.Chat.Type,
                                ParseMode.Html, null, true
                            );
                        }
                        catch
                        {
                            // ignored
                        }

                        if (telegramBotClient != null) await telegramBotClient.ExitGroupAsync(e);
                    }
                        break;

                    case ChatType.Private:
                        break;

                    case ChatType.Channel:
                        break;

                    case ChatType.Sender:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
        }
        catch
        {
            // ignored
        }
    }


    public static async Task<MessageSentResult?> SendGroupsByTitle(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? args)
    {
        if (args is { Length: < 1 }) return null;
        return args != null ? await SendGroupsByTitle(string.Join(" ", args), sender, e, 6) : null;
    }

    private static async Task<MessageSentResult?> SendGroupsByTitle(string query, TelegramBotAbstract? sender,
        MessageEventArgs? e, int limit)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
                return null;

            var groups = GetGroupsByTitle(query, limit, sender);

            if (groups == null)
                return null;

            Logger.Logger.WriteLine("Groups search (1): " + groups.Rows.Count);

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

            Logger.Logger.WriteLine("Groups search (2): " + buttonsMatrix?.Count);

            var text2 = GetTextSearchResult(limit, buttonsMatrix);

            var inline = buttonsMatrix == null ? null : new InlineKeyboardMarkup(buttonsMatrix);

            if (e is { Message.From: { } })
                return e.Message.Chat.Type switch
                {
                    ChatType.Sender or ChatType.Private => await SendMessage.SendMessageInPrivate(sender,
                        e.Message.From.Id,
                        e.Message.ReplyToMessage?.From?.LanguageCode ?? e.Message.From?.LanguageCode,
                        "", text2, ParseMode.Html, e.Message.ReplyToMessage?.MessageId, inline,
                        EventArgsContainer.Get(e)),
                    ChatType.Group or ChatType.Channel or ChatType.Supergroup => await SendMessage
                        .SendMessageInAGroup(
                            sender,
                            e.Message.ReplyToMessage?.From?.LanguageCode ?? e.Message.From?.LanguageCode,
                            text2, EventArgsContainer.Get(e),
                            e.Message.Chat.Id, e.Message.Chat.Type,
                            ParseMode.Html, e.Message.ReplyToMessage?.MessageId, true, inline),
                    _ => throw new ArgumentOutOfRangeException()
                };

            return null;
        }
        catch (Exception? exception)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(exception, sender, EventArgsContainer.Get(e));
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

    public static async Task<bool> GetGroups(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        return e != null && await CommandDispatcher.GetAllGroups(e.Message.From?.Id, e.Message.From?.Username, sender,
            e.Message.From?.LanguageCode,
            e.Message.Chat.Type);
    }

    public static async Task<bool> UpdateGroupsDry(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var text = await CommandDispatcher.UpdateGroups(sender, true, true, false, e);

        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
        return false;
    }

    public static async Task<bool> UpdateGroups(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var text = await CommandDispatcher.UpdateGroups(sender, false, true, false, e);

        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        return false;
    }

    public static async Task<bool> UpdateGroupsAndFixNames(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var text = await CommandDispatcher.UpdateGroups(sender, false, true, true, e);

        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        return false;
    }

    public static async Task<bool> UpdateGroupsAndFixNamesDry(MessageEventArgs? e, TelegramBotAbstract? sender)
    {
        var text = await CommandDispatcher.UpdateGroups(sender, true, true, true, e);

        if (e != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From?.Id,
                e.Message.From?.LanguageCode, e.Message.From?.Username, text.Language,
                ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        return false;
    }
}