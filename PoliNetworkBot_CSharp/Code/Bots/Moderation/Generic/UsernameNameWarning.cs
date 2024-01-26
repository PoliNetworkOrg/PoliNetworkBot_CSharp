using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Generic;

public static class UsernameNameWarning
{
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

    public static async Task SendUsernameWarning(TelegramBotAbstract? telegramBotClient,
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


    public static async Task SendUsernameWarning2Async(TelegramBotAbstract? telegramBotClient, Language s2,
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
}