#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

internal static class ModerationCheck
{
    private static readonly List<long> WhitelistForeignGroups = new()
    {
        -1001394018284 //japan group
    };

    private static readonly object Lock = new();

    public static async Task<Tuple<ToExit, ChatMember[], List<int>, string>> CheckIfToExitAndUpdateGroupList(
        TelegramBotAbstract sender, MessageEventArgs e)
    {
        switch (e.Message.Chat.Type)
        {
            case ChatType.Private:
                return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 13 },
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
            return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 30 },
                null);
        //end | exclude groups
        lock (Lock)
        {
            const string q1 = "SELECT id, valid FROM Groups WHERE id = @id";
            var dt = SqLite.ExecuteSelect(q1, GlobalVariables.DbConnection, new Dictionary<string, object> { { "@id", e.Message.Chat.Id } });
            if (dt != null && dt.Rows.Count > 0)
            {
                var r1 = CheckIfToExit(sender, e, dt.Rows[0].ItemArray[1]).Result;
                r1.Item3.Insert(0, 11);
                return r1;
            }

            InsertGroup(sender, e);
        }

        var r2 = await CheckIfToExit(sender, e, null);
        var list2 = r2.Item3;
        list2.Insert(0, 12);
        return new Tuple<ToExit, ChatMember[], List<int>, string>(r2.Item1, r2.Item2, list2, r2.Item4);
    }

    internal static async Task<List<long>> CheckIfNotAuthorizedBotHasBeenAdded(MessageEventArgs e,
        TelegramBotAbstract telegramBotClient)
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
        return isAdmin.IsSuccess() ? null : notAuthorizedBot;
    }

    private static async Task<Tuple<ToExit, ChatMember[], List<int>, string>> CheckIfToExit(
        TelegramBotAbstract telegramBotClient, MessageEventArgs e,
        object v)
    {
        switch (v)
        {
            case null:
            case DBNull:
            {
                var (toExit, chatMembers, ints) = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e);
                ints.Insert(0, 1);
                return new Tuple<ToExit, ChatMember[], List<int>, string>(toExit, chatMembers, ints, null);
            }
            case char b:
            {
                return b != 'Y'
                    ? await PreExitChecks(b.ToString(), e, telegramBotClient)
                    : new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 7 },
                        b.ToString());
            }
            case string s when string.IsNullOrEmpty(s):
            {
                var (toExit, chatMembers, ints) = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e);
                ints.Insert(0, 14);
                return new Tuple<ToExit, ChatMember[], List<int>, string>(toExit, chatMembers, ints, s);
            }
            case int i2:
            {
                if (i2 != 1)
                    return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.EXIT, null,
                        new List<int> { 41 },
                        i2.ToString());
                return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 42 },
                    i2.ToString());
            }
            case string s:
            {
                s = s.Trim();

                if (s is not ("Y" or "1"))
                    return await PreExitChecks(s, e, telegramBotClient);
                return new Tuple<ToExit, ChatMember[], List<int>, string>(ToExit.STAY, null, new List<int> { 9 },
                    s);
            }
            default:
            {
                var (toExit, chatMembers, ints) = await CheckIfToExit_NullValueAndUpdateIt(telegramBotClient, e);
                ints.Insert(0, 10);
                return new Tuple<ToExit, ChatMember[], List<int>, string>(toExit, chatMembers, ints, v?.ToString());
            }
        }
    }

    private static async Task<Tuple<ToExit, ChatMember[], List<int>, string>> PreExitChecks(string oldValid,
        MessageEventArgs messageEventArgs,
        TelegramBotAbstract telegramBotAbstract)
    {
        var (item1, item2, item3) = await CheckIfToExit_NullValue2Async(telegramBotAbstract, messageEventArgs);
        if (item1 == ToExit.EXIT)
            return new Tuple<ToExit, ChatMember[], List<int>, string>(item1, item2, item3, oldValid);
        try
        {
            const string q = "UPDATE Groups SET valid = @valid WHERE id = @id";
            const string valid = "Y";
            var d = new Dictionary<string, object>
            {
                { "@valid", valid },
                { "@id", messageEventArgs.Message.Chat.Id }
            };
            SqLite.Execute(q, GlobalVariables.DbConnection, d);
            var name = "";
            if (messageEventArgs is { Message.Chat.Title: { } })
                name = messageEventArgs.Message.Chat.Title;

            Logger.WriteLine("Changed group with ID: " + messageEventArgs?.Message?.Chat?.Id + ", name:" + name +
                             " to valid");
        }
        catch (Exception e)
        {
            await NotifyUtil.NotifyOwners(e, telegramBotAbstract, messageEventArgs);
        }

        return new Tuple<ToExit, ChatMember[], List<int>, string>(item1, item2, item3, oldValid);
    }

    private static async Task<Tuple<ToExit, ChatMember[], List<int>>> CheckIfToExit_NullValueAndUpdateIt(
        TelegramBotAbstract telegramBotClient,
        MessageEventArgs e)
    {
        var (toExit, chatMembers, ints) = await CheckIfToExit_NullValue2Async(telegramBotClient, e);
        var valid = toExit == ToExit.STAY ? "Y" : "N";

        var q = "UPDATE Groups SET valid = @valid WHERE id = @id";
        var d = new Dictionary<string, object>
        {
            { "@valid", valid },
            { "@id", e.Message.Chat.Id }
        };
        SqLite.Execute(q, GlobalVariables.DbConnection, d);

        ints.Insert(0, 2);
        return new Tuple<ToExit, ChatMember[], List<int>>(toExit, chatMembers, ints);
    }

    private static async Task<Tuple<ToExit, ChatMember[], List<int>>> CheckIfToExit_NullValue2Async(
        TelegramBotAbstract telegramBotClient, MessageEventArgs e)
    {
        var r = await telegramBotClient.GetChatAdministratorsAsync(e.Message.Chat.Id);
        if (r == null)
            return new Tuple<ToExit, ChatMember[], List<int>>(ToExit.STAY, null, new List<int> { 3 });

        return r.Select(Creators.CheckIfIsCreatorOrSubCreator)
            .Any(isCreator => isCreator != null && isCreator.Value)
            ? new Tuple<ToExit, ChatMember[], List<int>>(ToExit.STAY, r, new List<int> { 4 })
            : new Tuple<ToExit, ChatMember[], List<int>>(ToExit.EXIT, r, new List<int> { 5 });
    }

    private static void InsertGroup(TelegramBotAbstract sender, MessageEventArgs e)
    {
        try
        {
            const string q1 = "INSERT INTO Groups (id, bot_id, type, title) VALUES (@id, @botid, @type, @title)";
            SqLite.Execute(q1, GlobalVariables.DbConnection, new Dictionary<string, object>
            {
                { "@id", e.Message.Chat.Id },
                { "@botid", sender.GetId() },
                { "@type", e.Message.Chat.Type.ToString() },
                { "@title", e.Message.Chat.Title }
            });
            _ = CreateInviteLinkAsync(sender, e);
        }
        catch (Exception ex)
        {
            _ = NotifyUtil.NotifyOwners(ex, sender, e);
        }
    }

    private static async Task<NuovoLink> CreateInviteLinkAsync(TelegramBotAbstract sender, MessageEventArgs e)
    {
        return await InviteLinks.CreateInviteLinkAsync(e.Message.Chat.Id, sender, e);
    }

    private static List<UsernameAndNameCheckResult> CheckUsername(MessageEventArgs e)
    {
        if (GlobalVariables.NoUsernameCheckInThisChats.Contains(e.Message.Chat.Id)) return null;

        if (e.Message is { From: { } } &&
            GlobalVariables.AllowedNoUsernameFromThisUserId.Contains(e.Message.From.Id))
            return null;

        var r = new List<UsernameAndNameCheckResult>
        {
            CheckUsername2(e.Message.From?.Username, e.Message.From?.FirstName,
                lastName: e.Message.From?.LastName, language: e.Message.From?.LanguageCode,
                userId: e.Message.From?.Id, messageId: e.Message.MessageId)
        };

        if (e.Message.NewChatMembers == null || e.Message.NewChatMembers.Length == 0)
            return r;

        r.AddRange(from user in e.Message.NewChatMembers
            where user.Id != r[0].GetUserId()
            select CheckUsername2(user.Username, user.FirstName, user.Id,
                user.LastName, user.LanguageCode, e.Message.MessageId));

        return r;
    }

    private static UsernameAndNameCheckResult CheckUsername2(string fromUsername, string fromFirstName,
        long? userId,
        string lastName, string language, long? messageId)
    {
        var username = false;
        var name = false;

        if (string.IsNullOrEmpty(fromUsername))
        {
            if (userId != null && GlobalVariables.AllowedNoUsernameFromThisUserId.Contains(userId.Value))
                username = false;
            else
                username = true;
        }

        if (fromFirstName.Length < 2)
            name = true;

        return new UsernameAndNameCheckResult(username, name, language,
            fromUsername, userId, fromFirstName, lastName, messageId);
    }

    public static async Task<SpamType> CheckSpamAsync(MessageEventArgs e, TelegramBotAbstract telegramBotClient)
    {
        switch (e.Message)
        {
            case { Chat.Type: ChatType.Private }:
            case { From: { }, Chat: { } }
                when e.Message.From.Id == 777000 || e.Message.From.Id == e.Message.Chat.Id:
                return SpamType.ALL_GOOD;
        }

        if (GlobalVariables.AllowedSpam.ToList().Any(x => x.Matches(e.Message?.From)))
            return SpamType.ALL_GOOD;

        if (e.Message is { From: { }, Chat: { } })
        {
            var storedMessageResult = MessagesStore.StoreAndCheck(e.Message);
            switch (storedMessageResult)
            {
                case SpamType.SPAM_LINK:
                {
                    await DeleteMessage.TryDeleteMessagesAsync(MessagesStore.GetMessages(e.Message.Text),
                        telegramBotClient);
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
        }

        if (string.IsNullOrEmpty(e.Message.Text))
            return SpamTypeUtil.Merge(Blacklist.IsSpam(e.Message.Caption, e.Message.Chat.Id, telegramBotClient),
                Blacklist.IsSpam(e.Message.Photo));

        if (e.Message.Text.StartsWith("/"))
            return SpamType.ALL_GOOD;

        var isForeign = DetectForeignLanguage(e);

        if (isForeign)
            return SpamType.FOREIGN;

        return SpamTypeUtil.Merge(Blacklist.IsSpam(e.Message.Text, e.Message.Chat.Id, telegramBotClient),
            Blacklist.IsSpam(e.Message.Photo));
    }


    private static bool DetectForeignLanguage(MessageEventArgs e)
    {
        if (WhitelistForeignGroups.Contains(e.Message.Chat.Id))
            return false;

        if (e.Message.Text == null)
            return false;

        var koreanCharactersCount = Regex.Matches(e.Message.Text, @"[\uac00-\ud7a3]").Count;
        var japaneseCharactersCount = Regex.Matches(e.Message.Text, @"[\u3040-\u30ff]").Count;
        var chineseCharactersCount = Regex.Matches(e.Message.Text, @"[\u4e00-\u9FFF]").Count;

        return koreanCharactersCount + japaneseCharactersCount + chineseCharactersCount >= 3;
    }

    private static async Task SendUsernameWarning(TelegramBotAbstract telegramBotClient,
        bool username, bool name, string lang, string usernameOfUser,
        long chatId, long? userId, long? messageId, ChatType messageChatType,
        string firstName, string lastName, User[] newChatMembers, MessageEventArgs messageEventArgs)
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

        var s2 = new Language(new Dictionary<string, string>
        {
            { "it", s1I },
            { "en", s1E }
        });

        var r1 = await SendMessage.SendMessageInPrivateOrAGroup(telegramBotClient, s2, lang,
            usernameOfUser, userId, firstName, lastName, chatId, messageChatType);

        const int minutesWait = 2;

        if (r1.GetChatType() != ChatType.Private)
        {
            var r2 = r1.GetMessage();
            if (r2 != null)
                switch (r2)
                {
                    case TLMessage r3:
                    {
                        lock (GlobalVariables.MessagesToDelete)
                        {
                            var timeUntilDelete = TimeSpan.FromMinutes(minutesWait);
                            var timeToDelete = DateTime.Now + timeUntilDelete;
                            var botid = telegramBotClient.GetId();
                            if (botid != null)
                            {
                                var toDelete = new MessageToDelete(r3, chatId, timeToDelete, botid.Value,
                                    r1.GetChatType(), null);
                                GlobalVariables.MessagesToDelete.Add(toDelete);

                                FileSerialization.WriteToBinaryFile(Paths.Bin.MessagesToDelete,
                                    GlobalVariables.MessagesToDelete);
                            }
                        }

                        break;
                    }
                    case Message r4:
                    {
                        lock (GlobalVariables.MessagesToDelete)
                        {
                            var timeUntilDelete = TimeSpan.FromMinutes(minutesWait);
                            var timeToDelete = DateTime.Now + timeUntilDelete;
                            var botId = telegramBotClient.GetId();
                            if (botId != null)
                            {
                                var toDelete = new MessageToDelete(r4, chatId, timeToDelete, botId.Value,
                                    r1.GetChatType(), null);
                                GlobalVariables.MessagesToDelete.Add(toDelete);

                                FileSerialization.WriteToBinaryFile(Paths.Bin.MessagesToDelete,
                                    GlobalVariables.MessagesToDelete);
                            }
                        }

                        break;
                    }
                    default:
                    {
                        var e4 = "Attempted to add a message to be deleted in queue\n" + r2?.GetType() + " " + r2;
                        var e3 = new Exception(e4);
                        await NotifyUtil.NotifyOwners(e3, telegramBotClient, messageEventArgs);
                        break;
                    }
                }
        }

        if (newChatMembers == null || newChatMembers.Length == 0)
            await RestrictUser.Mute(60 * 5, telegramBotClient, chatId, userId, messageChatType,
                RestrictAction.MUTE);

        if (messageId != null)
            await telegramBotClient.DeleteMessageAsync(chatId, messageId.Value, null);
    }

    public static async Task AntiSpamMeasure(TelegramBotAbstract telegramBotClient, MessageEventArgs e,
        SpamType checkSpam)
    {
        if (checkSpam == SpamType.ALL_GOOD)
            return;

        if (e.Message.From != null)
        {
            await RestrictUser.Mute(60 * 5, telegramBotClient, e.Message.Chat.Id, e.Message.From.Id,
                e.Message.Chat.Type, RestrictAction.MUTE);

            switch (checkSpam)
            {
                case SpamType.SPAM_LINK:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        { "en", "You sent a message with spam, and you were muted for 5 minutes" },
                        { "it", "Hai inviato un messaggio con spam, e quindi il bot ti ha mutato per 5 minuti" }
                    });

                    await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                        e.Message.From.LanguageCode,
                        e.Message.From.Username, text2, ParseMode.Html, null);

                    break;
                }
                case SpamType.NOT_ALLOWED_WORDS:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        { "en", "You sent a message with banned words, and you were muted for 5 minutes" },
                        {
                            "it",
                            "Hai inviato un messaggio con parole bandite, e quindi il bot ti ha mutato per 5 minuti"
                        }
                    });

                    await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                        e.Message.From.LanguageCode,
                        e.Message.From.Username, text2, ParseMode.Html, null);

                    break;
                }
                case SpamType.FORMAT_INCORRECT:
                {
                    var text2 = new Language(new Dictionary<string, string>
                    {
                        { "en", "You have sent a message that does not follow the group format" },
                        { "it", "Hai inviato un messaggio che non rispetta il format del gruppo" }
                    });

                    await SendMessage.SendMessageInPrivate(telegramBotClient, e.Message.From.Id,
                        e.Message.From.LanguageCode,
                        e.Message.From.Username, text2, ParseMode.Html, null);

                    break;
                }

                case SpamType.FOREIGN:
                {
                    var text2 = new Language(new Dictionary<string, string>
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
                        ParseMode.Html, null);
                    break;
                }

                // ReSharper disable once UnreachableSwitchCaseDueToIntegerAnalysis
                case SpamType.ALL_GOOD:
                    return;

                default:
                    throw new ArgumentOutOfRangeException(nameof(checkSpam), checkSpam, null);
            }
        }

        await telegramBotClient.DeleteMessageAsync(e.Message.Chat.Id, e.Message.MessageId, null);
    }

    public static async Task<bool> CheckUsernameAndName(MessageEventArgs e, TelegramBotAbstract telegramBotClient)
    {
        var usernameCheck = CheckUsername(e);
        if (usernameCheck == null) return false;

        var donesomething = false;

        foreach (var usernameCheck2 in usernameCheck.Where(usernameCheck2 => usernameCheck2 != null)
                     .Where(usernameCheck2 => usernameCheck2.Name || usernameCheck2.UsernameBool))
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

    public static async Task PermittedSpamMeasure(TelegramBotAbstract telegramBotClient,
        MessageEventArgs messageEventArgs)
    {
        await NotifyUtil.NotifyOwnersPermittedSpam(telegramBotClient, messageEventArgs);
    }
}