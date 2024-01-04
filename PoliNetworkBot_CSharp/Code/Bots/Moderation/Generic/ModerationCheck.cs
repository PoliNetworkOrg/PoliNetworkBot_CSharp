﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Generic;

internal static class ModerationCheck
{
    private static readonly List<long> WhitelistForeignGroups = new()
    {
        -1001394018284 //japan group
    };

    private static readonly object Lock = new();

    public static async Task<Tuple<ToExit?, ChatMember[]?, List<int>?, string?>?> CheckIfToExitAndUpdateGroupList(
        TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e == null) return null;
        switch (e.Message.Chat.Type)
        {
            case ChatType.Private:
                return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(ToExit.STAY, null,
                    new List<int> { 13 },
                    "private");

            case ChatType.Group:
                break;

            case ChatType.Channel:
                break;

            case ChatType.Supergroup:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        //start | exclude groups, bot will not operate in them
        if (e.Message.Chat.Id == ConfigAnon.ModAnonCheckGroup)
            return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(ToExit.STAY, null,
                new List<int> { 30 },
                null);
        //end | exclude groups
        lock (Lock)
        {
            const string? q1 = "SELECT id, valid FROM GroupsTelegram WHERE id = @id";
            var dt = Database.ExecuteSelectUnlogged(q1, GlobalVariables.DbConfig,
                new Dictionary<string, object?> { { "@id", e.Message.Chat.Id } });
            if (dt is { Rows.Count: > 0 })
            {
                var r1 = CheckIfToExit(sender, e, dt.Rows[0].ItemArray[1]).Result;
                r1?.Item3?.Insert(0, 11);
                return r1;
            }

            InsertGroup(sender, e);
        }

        var (item1, item2, list2, item4) = await CheckIfToExit(sender, e, null) ??
                                           new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(null, null, null,
                                               null);


        if (list2 == null) return null;
        list2.Insert(0, 12);
        return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(item1, item2, list2, item4);
    }

    internal static async Task<List<long>?> CheckIfNotAuthorizedBotHasBeenAdded(MessageEventArgs? e,
        TelegramBotAbstract? telegramBotClient)
    {
        if (e == null || telegramBotClient == null)
            return null;

        if (e.Message.NewChatMembers == null || e.Message.NewChatMembers.Length == 0)
            return null;

        var notAuthorizedBot =
            (from newMember in e.Message.NewChatMembers where newMember.IsBot select newMember.Id).ToList();

        if (notAuthorizedBot.Count == 0) return null;

        if (e.Message.From == null)
            return null;

        var userThatAddedBots = e.Message.From.Id;
        var isAdmin = await telegramBotClient.IsAdminAsync(userThatAddedBots, e.Message.Chat.Id);
        return isAdmin != null && isAdmin.IsSuccess() ? null : notAuthorizedBot;
    }

    private static async Task<Tuple<ToExit?, ChatMember[]?, List<int>?, string?>?> CheckIfToExit(
        TelegramBotAbstract? telegramBotClient, MessageEventArgs? e,
        object? v)
    {
        switch (v)
        {
            case null:
            case DBNull:
            {
                var (toExit, chatMembers, ints) = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e) ??
                                                  new Tuple<ToExit?, ChatMember[]?, List<int>?>(null, null, null);
                if (ints != null)
                {
                    ints.Insert(0, 1);
                    return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(toExit, chatMembers, ints, null);
                }

                break;
            }
            case char b:
            {
                if (b != 'Y') return await PreExitChecks(b.ToString(), e, telegramBotClient);
                return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(ToExit.STAY, null, new List<int> { 7 },
                    b.ToString());
            }
            case string s when string.IsNullOrEmpty(s):
            {
                var (toExit, chatMembers, ints) = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e) ??
                                                  new Tuple<ToExit?, ChatMember[]?, List<int>?>(null, null, null);
                if (ints != null)
                {
                    ints.Insert(0, 14);
                    return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(toExit, chatMembers, ints, s);
                }

                break;
            }
            case int i2:
            {
                if (i2 != 1)
                    return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(ToExit.EXIT, null,
                        new List<int> { 41 },
                        i2.ToString());
                return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(ToExit.STAY, null, new List<int> { 42 },
                    i2.ToString());
            }
            case string s:
            {
                s = s.Trim();

                if (s is not ("Y" or "1"))
                    return await PreExitChecks(s, e, telegramBotClient);
                return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(ToExit.STAY, null, new List<int> { 9 },
                    s);
            }
            default:
            {
                var (toExit, chatMembers, ints) = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e) ??
                                                  new Tuple<ToExit?, ChatMember[]?, List<int>?>(null, null, null);
                if (ints != null)
                {
                    ints.Insert(0, 10);
                    return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(toExit, chatMembers, ints,
                        v.ToString());
                }

                break;
            }
        }

        return null;
    }

    private static async Task<Tuple<ToExit?, ChatMember[]?, List<int>?, string?>?> PreExitChecks(
        string? oldValid,
        MessageEventArgs? messageEventArgs,
        TelegramBotAbstract? telegramBotAbstract)
    {
        var (item1, item2, item3) = await CheckIfToExit_NullValue2Async(telegramBotAbstract, messageEventArgs) ??
                                    new Tuple<ToExit?, ChatMember[]?, List<int>?>(null, null, null);
        if (item1 == ToExit.EXIT)
            return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(item1, item2, item3, oldValid);
        try
        {
            const string? q = "UPDATE GroupsTelegram SET valid = @valid WHERE id = @id";
            const string? valid = "Y";
            if (messageEventArgs?.Message != null)
            {
                var d = new Dictionary<string, object?>
                {
                    { "@valid", valid },
                    { "@id", messageEventArgs.Message.Chat.Id }
                };
                Database.Execute(q, GlobalVariables.DbConfig, d);
            }

            var name = "";
            if (messageEventArgs is { Message.Chat.Title: not null })
                name = messageEventArgs.Message.Chat.Title;

            Logger.WriteLine("Changed group with ID: " + messageEventArgs?.Message.Chat.Id + ", name:" + name +
                             " to valid");
        }
        catch (Exception? e)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e, telegramBotAbstract, EventArgsContainer.Get(messageEventArgs));
        }

        return new Tuple<ToExit?, ChatMember[]?, List<int>?, string?>(item1, item2, item3, oldValid);
    }

    private static async Task<Tuple<ToExit?, ChatMember[]?, List<int>?>?> CheckIfToExit_NullValueAndUpdateIt(
        TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? e)
    {
        var (toExit, chatMembers, ints) = await CheckIfToExit_NullValue2Async(telegramBotClient, e) ??
                                          new Tuple<ToExit?, ChatMember[]?, List<int>?>(null, null, null);
        var valid = toExit == ToExit.STAY ? "Y" : "N";

        const string q = "UPDATE GroupsTelegram SET valid = @valid WHERE id = @id";
        if (e?.Message != null)
        {
            var d = new Dictionary<string, object?>
            {
                { "@valid", valid },
                { "@id", e.Message.Chat.Id }
            };
            Database.Execute(q, GlobalVariables.DbConfig, d);
        }

        if (ints == null) return null;
        ints.Insert(0, 2);
        return new Tuple<ToExit?, ChatMember[]?, List<int>?>(toExit, chatMembers, ints);
    }

    private static async Task<Tuple<ToExit?, ChatMember[]?, List<int>?>?> CheckIfToExit_NullValue2Async(
        TelegramBotAbstract? telegramBotClient, MessageEventArgs? e)
    {
        var idChat = e?.Message.Chat.Id;
        if (idChat == null) return null;
        if (telegramBotClient == null) return null;
        var r = await telegramBotClient.GetChatAdministratorsAsync(idChat);
        if (r == null)
            return new Tuple<ToExit?, ChatMember[]?, List<int>?>(ToExit.STAY, null,
                new List<int> { 3 });

        return r.Select(Creators.CheckIfIsCreatorOrSubCreator)
            .Any(isCreator => isCreator != null && isCreator.Value)
            ? new Tuple<ToExit?, ChatMember[]?, List<int>?>(ToExit.STAY, r, new List<int> { 4 })
            : new Tuple<ToExit?, ChatMember[]?, List<int>?>(ToExit.EXIT, r, new List<int> { 5 });
    }

    private static void InsertGroup(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        try
        {
            const string? q1 =
                "INSERT INTO GroupsTelegram (id, bot_id, type, title) VALUES (@id, @botid, @type, @title)";

            if (e == null) return;
            if (sender != null)
                Database.Execute(q1, GlobalVariables.DbConfig, new Dictionary<string, object?>
                {
                    { "@id", e.Message.Chat.Id },
                    { "@botid", sender.GetId() },
                    { "@type", e.Message.Chat.Type.ToString() },
                    { "@title", e.Message.Chat.Title }
                });
            _ = CreateInviteLinkAsync(sender, e);
        }
        catch (Exception? ex)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(ex, sender, EventArgsContainer.Get(e));
        }
    }

    private static async Task<NuovoLink?> CreateInviteLinkAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        return e?.Message != null ? await InviteLinks.CreateInviteLinkAsync(e.Message.Chat.Id, sender, e) : null;
    }

    private static List<UsernameAndNameCheckResult>? CheckUsername(MessageEventArgs? e)
    {
        var message1 = e?.Message;
        var id = message1?.Chat.Id;
        if (GlobalVariables.NoUsernameCheckInThisChats != null && e?.Message != null && id != null &&
            GlobalVariables.NoUsernameCheckInThisChats.Contains(id.Value)) return null;

        var from = message1?.From;
        if (e != null && GlobalVariables.AllowedNoUsernameFromThisUserId != null && e.Message is { From: not null } &&
            from != null && GlobalVariables.AllowedNoUsernameFromThisUserId.Contains(from.Id))
            return null;

        var message = e?.Message;
        if (message == null) return null;
        var r = new List<UsernameAndNameCheckResult>
        {
            CheckUsername2(e?.Message.From?.Username, e?.Message.From?.FirstName,
                lastName: e?.Message.From?.LastName, language: e?.Message.From?.LanguageCode,
                userId: e?.Message.From?.Id, messageId: message.MessageId)
        };

        var newChatMembers = message1?.NewChatMembers;
        if (e?.Message.NewChatMembers == null || newChatMembers == null || newChatMembers.Length == 0)
            return r;

        r.AddRange(from user in newChatMembers
            where user.Id != r[0].GetUserId()
            select CheckUsername2(user.Username, user.FirstName, user.Id,
                user.LastName, user.LanguageCode, message1?.MessageId));

        return r;
    }

    private static UsernameAndNameCheckResult CheckUsername2(string? fromUsername, string? fromFirstName,
        long? userId,
        string? lastName, string? language, long? messageId)
    {
        var username = GlobalVariables.AllowedNoUsernameFromThisUserId != null && string.IsNullOrEmpty(fromUsername) &&
                       (userId == null || !GlobalVariables.AllowedNoUsernameFromThisUserId.Contains(userId.Value));
        var name = fromFirstName is { Length: < 2 };

        return new UsernameAndNameCheckResult(username, name, language,
            fromUsername, userId, fromFirstName, lastName, messageId);
    }

    internal static bool CheckIfHeIsAllowedSpam(MessageEventArgs? e)
    {
        var message = e?.Message;
        if (message == null)
            return false;

        var from1 = message.From;
        var chat = message.Chat;

        var b = chat.Type == ChatType.Private;
        return b || (from1 != null && Innocuo(from1, chat));
    }

    private static bool Innocuo(User from1, Chat? chat)
    {
        return from1.Id == 777000 || from1.Id == chat?.Id ||
               CheckIfIsInList(GlobalVariables.AllowedSpam, from1) ||
               CheckIfIsInList(GlobalVariables.Creators, from1) ||
               CheckIfIsInList(GlobalVariables.SubCreators, from1) ||
               CheckIfIsInList(GlobalVariables.Owners, from1);
    }

    internal static SpamType? CheckIfSpamStored(MessageEventArgs? e,
        TelegramBotAbstract? telegramBotAbstract)
    {
        var eMessage = e?.Message;
        if (eMessage == null)
            return null;

        var storedMessageResult = MessagesStore.StoreAndCheck(eMessage);
        switch (storedMessageResult)
        {
            case SpamType.SPAM_LINK:
            {
                if (eMessage.Text == null)
                    return SpamType.SPAM_LINK;

                var messages = MessagesStore.GetMessages(eMessage.Text);
                DeleteMessage.TryDeleteMessagesAsync(messages, telegramBotAbstract);

                return SpamType.SPAM_LINK;
            }
            case SpamType.NOT_ALLOWED_WORDS:
                break;

            case SpamType.ALL_GOOD:
                break;

            case SpamType.FOREIGN:
                break;

            case SpamType.FORMAT_INCORRECT:
                break;

            case SpamType.SPAM_PERMITTED:
                return SpamType.SPAM_PERMITTED;

            case SpamType.UNDEFINED:
                break;
        }

        return null;
    }

    private static bool CheckIfIsInList(IEnumerable<TelegramUser>? a, User from)
    {
        return a != null && a.Any(x => x.Matches(from));
    }

    internal static bool DetectForeignLanguage(MessageEventArgs? e)
    {
        if (e?.Message != null && WhitelistForeignGroups.Contains(e.Message.Chat.Id))
            return false;

        if (e?.Message.Text == null)
            return false;

        var koreanCharactersCount = Regex.Matches(e.Message.Text, @"[\uac00-\ud7a3]").Count;
        var japaneseCharactersCount = Regex.Matches(e.Message.Text, @"[\u3040-\u30ff]").Count;
        var chineseCharactersCount = Regex.Matches(e.Message.Text, @"[\u4e00-\u9FFF]").Count;

        return koreanCharactersCount + japaneseCharactersCount + chineseCharactersCount >= 3;
    }

    private static async Task SendUsernameWarning(TelegramBotAbstract? telegramBotClient,
        bool username, bool name, string? lang, string? usernameOfUser,
        long chatId, long? userId, long? messageId, ChatType messageChatType,
        string? firstName, string? lastName, IReadOnlyCollection<User>? newChatMembers,
        MessageEventArgs? messageEventArgs)
    {
        var s1I = username switch
        {
            true when !name =>
                "Imposta un username dalle impostazioni di telegram per poter scrivere in questo gruppo\n",
            false when name => "Imposta un nome più lungo " + "dalle impostazioni di telegram\n",
            _ =>
                "Imposta un username e un nome più lungo dalle impostazioni di telegram per poter scrivere in questo gruppo\n"
        };

        var s1E = username switch
        {
            true when !name => "Set an username from telegram settings to write in this group\n",
            false when name => "Set a longer first name from telegram settings to write in this group\n",
            _ => "Set an username and a longer first name from telegram settings to write in this group\n"
        };

        var s2 = new Language(new Dictionary<string, string?>
        {
            { "it", s1I },
            { "en", s1E }
        });

        GlobalVariables.UsernameWarningDictSent ??= new Dictionary<long, DateTime>();
        if (userId != null)
        {
            var doIt = !GlobalVariables.UsernameWarningDictSent.ContainsKey(userId.Value) ||
                       GlobalVariables.UsernameWarningDictSent[userId.Value].AddMinutes(5) <= DateTime.Now;
            if (doIt)
            {
                await SendUsernameWarning2Async(telegramBotClient, s2, lang, usernameOfUser, userId,
                    firstName, lastName, chatId, messageChatType, messageEventArgs);
                GlobalVariables.UsernameWarningDictSent[userId.Value] = DateTime.Now;
            }
        }

        if (newChatMembers == null || newChatMembers.Count == 0)
            await RestrictUser.Mute(60 * 5, telegramBotClient, chatId, userId, messageChatType,
                RestrictAction.MUTE);

        if (messageId != null)
            if (telegramBotClient != null)
                await telegramBotClient.DeleteMessageAsync(chatId, messageId.Value, null);
    }

    private static async Task SendUsernameWarning2Async(TelegramBotAbstract? telegramBotClient, Language s2,
        string? lang, string? usernameOfUser, long? userId, string? firstName, string? lastName,
        long chatId, ChatType messageChatType, MessageEventArgs? messageEventArgs)
    {
        if (telegramBotClient == null)
            return;

        var r1 = await SendMessage.SendMessageInPrivateOrAGroup(telegramBotClient, s2, lang,
            usernameOfUser, userId, firstName, lastName, chatId, messageChatType);

        var botid = telegramBotClient.GetId();
        if (botid == null)
            return;

        const int minutesWait = 2;

        if (r1?.GetChatType() == ChatType.Private) return;

        var r2 = r1?.GetMessage();
        if (r2 == null) return;

        var botIdValue = botid.Value;
        var timeUntilDelete = TimeSpan.FromMinutes(minutesWait);
        var timeToDelete = DateTime.Now + timeUntilDelete;

        GlobalVariables.MessagesToDelete ??= new List<MessageToDelete>();

        switch (r2)
        {
            case TLMessage r3:
            {
                lock (GlobalVariables.MessagesToDelete)
                {
                    var toDelete = new MessageToDelete(r3, chatId, timeToDelete, botIdValue,
                        r1?.GetChatType(), null);
                    GlobalVariables.MessagesToDelete.Add(toDelete);

                    FileSerialization.WriteToBinaryFile(Paths.Bin.MessagesToDelete,
                        GlobalVariables.MessagesToDelete);
                }

                break;
            }
            case Message r4:
            {
                lock (GlobalVariables.MessagesToDelete)
                {
                    var toDelete = new MessageToDelete(r4, chatId, timeToDelete, botIdValue,
                        r1?.GetChatType(), null);
                    GlobalVariables.MessagesToDelete.Add(toDelete);

                    FileSerialization.WriteToBinaryFile(Paths.Bin.MessagesToDelete,
                        GlobalVariables.MessagesToDelete);
                }

                break;
            }
            default:
            {
                var e4 = "Attempted to add a message to be deleted in queue\n" + r2.GetType() + " " + r2;
                var e3 = new Exception(e4);
                await NotifyUtil.NotifyOwnerWithLog2(e3, telegramBotClient,
                    EventArgsContainer.Get(messageEventArgs));
                return;
            }
        }
    }

    public static async Task<bool> AntiSpamMeasure(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e,
        SpamType checkSpam)
    {
        Logger.WriteLogComplete(new List<object?>
        {
            checkSpam.ToString(),
            e?.Message.Chat.Id, e?.Message.From?.Id, e?.Message.From?.Username,
            e?.Message.From?.FirstName, e?.Message.MessageId, e?.Message.Chat.Title,
            e?.Message.Text, e?.Message.Caption, e?.Message.Date, e?.Message.Type,
            e?.Message.AuthorSignature, e?.Message.ViaBot?.Username
        }, telegramBotClient, "AntiSpamMeasure");


        if (checkSpam == SpamType.ALL_GOOD)
            return false;

        if (e?.Message.From == null)
            return telegramBotClient != null && e?.Message != null &&
                   await telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);

        await RestrictUser.Mute(60 * 5, telegramBotClient, e.Message.Chat.Id, e.Message.From.Id,
            e.Message.Chat.Type, RestrictAction.MUTE);

        switch (checkSpam)
        {
            case SpamType.SPAM_LINK:
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "en", "You sent a message with spam, and you were muted for 5 minutes" },
                    { "it", "Hai inviato un messaggio con spam, e quindi il bot ti ha mutato per 5 minuti" }
                });

                await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username, text2, ParseMode.Html, null,
                    InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e), false);

                break;
            }
            case SpamType.NOT_ALLOWED_WORDS:
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "en", "You sent a message with banned words, and you were muted for 5 minutes" },
                    {
                        "it",
                        "Hai inviato un messaggio con parole bandite, e quindi il bot ti ha mutato per 5 minuti"
                    }
                });

                await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
                    EventArgsContainer.Get(e));

                break;
            }
            case SpamType.FORMAT_INCORRECT:
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "en", "You have sent a message that does not follow the group format" },
                    { "it", "Hai inviato un messaggio che non rispetta il format del gruppo" }
                });

                await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username, text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(),
                    EventArgsContainer.Get(e), false);

                break;
            }

            case SpamType.FOREIGN:
            {
                var text2 = new Language(new Dictionary<string, string?>
                {
                    { "en", "You sent a message with banned characters, and you were muted for 5 minutes" },
                    {
                        "it",
                        "Hai inviato un messaggio con caratteri banditi, e quindi il bot ti ha mutato per 5 minuti"
                    }
                });

                await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username, text2,
                    ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
                break;
            }

            // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
            case SpamType.ALL_GOOD:
                return true;

            default:
                throw new ArgumentOutOfRangeException(nameof(checkSpam), checkSpam, null);
        }

        return telegramBotClient != null &&
               await telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
    }

    public static async Task<bool> CheckUsernameAndName(MessageEventArgs? e, TelegramBotAbstract? telegramBotClient)
    {
        var usernameCheck = CheckUsername(e);
        if (usernameCheck == null)
            return false;

        if (e?.Message == null)
            return false;

        var donesomething = false;

        var usernameAndNameCheckResults = usernameCheck
            .Where(usernameCheck2 => usernameCheck2.Name || usernameCheck2.UsernameBool);

        foreach (var usernameCheck2 in usernameAndNameCheckResults)
        {
            await SendUsernameWarning(telegramBotClient,
                usernameCheck2.UsernameBool,
                usernameCheck2.Name,
                usernameCheck2.GetLanguage(),
                usernameCheck2.GetUsername(),
                e.Message.Chat.Id,
                usernameCheck2.GetUserId(),
                usernameCheck2.GetMessageId(),
                e.Message.Chat.Type,
                usernameCheck2.GetFirstName(),
                usernameCheck2.GetLastName(),
                e.Message.NewChatMembers,
                e);

            donesomething = true;
        }

        return donesomething;
    }

    public static async Task<bool> PermittedSpamMeasure(TelegramBotAbstract? telegramBotClient,
        EventArgsContainer? e)
    {
        Logger.WriteLogComplete(
            new List<object?>
            {
                e?.MessageEventArgs?.Message.Chat.Id, e?.MessageEventArgs?.Message.From?.Id,
                e?.MessageEventArgs?.Message.From?.Username,
                e?.MessageEventArgs?.Message.From?.FirstName, e?.MessageEventArgs?.Message.MessageId,
                e?.MessageEventArgs?.Message.Chat.Title
            }, telegramBotClient, "PermittedSpamMeasure"
        );

        return await NotifyUtil.NotifyOwnersPermittedSpam(telegramBotClient, e);
    }
}