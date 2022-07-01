#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
// ReSharper disable InconsistentNaming

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public static class MessagesStore
{
    private static Dictionary<string, StoredMessage?>? Store;

    public static void InitializeMessageStore()
    {
        try
        {
            Dictionary<string, StoredMessage?>? x = JsonConvert.DeserializeObject<Dictionary<string, StoredMessage?>>(
                File.ReadAllText(Paths.Data.MessageStore));
            Store = x ?? new Dictionary<string, StoredMessage?>();
        }
        catch (Exception? ex)
        {
            Store = new Dictionary<string, StoredMessage?>();
            Logger.WriteLine(ex);
        }
    }

    /// <summary>
    ///     Adds a new message to the storage
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageAllowedStatus"></param>
    /// <param name="timeLater">Allow at a later time starting from now.</param>
    /// <returns></returns>
    public static bool AddMessage(string message,
        MessageAllowedStatusEnum messageAllowedStatus = MessageAllowedStatusEnum.NOT_DEFINED_ERROR,
        TimeSpan? timeLater = null)
    {
        if (message == null)
            return false;

        if (string.IsNullOrEmpty(message))
            return false;

        if (Store != null && Store.ContainsKey(message))
            Store.Remove(message);

        if (timeLater != null && messageAllowedStatus != MessageAllowedStatusEnum.PENDING
            || timeLater == null && messageAllowedStatus == MessageAllowedStatusEnum.PENDING)
            throw new Exception("TimeLater and status mismatch");

        if (Store != null)
            lock (Store)
            {
                Store.Add(message,
                    new StoredMessage(message, allowedSpam: messageAllowedStatus, timeLater: timeLater));
            }

        return true;
    }

    /// <summary>
    ///     Adds new allowed message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public static bool AllowMessage(string message)
    {
        return AddMessage(message, MessageAllowedStatusEnum.ALLOWED);
    }

    public static void CheckTimestamp()
    {
        if (Store != null && Store.Count == 0)
            return;
        if (Store != null)
            foreach (var message in Store.Keys)
            {
                Store.TryGetValue(message, out var storedMessage);
                if (storedMessage == null)
                    continue;

                if (!storedMessage.IsOutdated())
                    continue;

                lock (Store)
                {
                    Store.Remove(message);
                }
            }
    }

    public static void RemoveMessage(string text)
    {
        if (Store != null && !Store.ContainsKey(text)) return;
        if (Store != null)
            lock (Store)
            {
                Store.Remove(text);
            }
    }

    public static List<StoredMessage?>? GetAllMessages(Func<StoredMessage?, bool>? filter = null)
    {
        if (Store != null)
        {
            List<StoredMessage?> r = Store.Values.ToList();
            if (filter != null)
                r = r.Where(filter).ToList();

            return r;
        }

        return null;
    }

    internal static SpamType StoreAndCheck(Message? message)
    {
        if (message == null)
            return SpamType.UNDEFINED;

        if (!string.IsNullOrEmpty(message.Text))
            return StoreAndCheck2(message, message.Text);
        return !string.IsNullOrEmpty(message.Caption) ? StoreAndCheck2(message, message.Caption) : SpamType.UNDEFINED;
    }

    private static SpamType StoreAndCheck2(Message? message, string? text)
    {
        if (message != null && (string.IsNullOrEmpty(text) || message.From == null))
            return SpamType.UNDEFINED;

        if (Store != null)
        {
            if (text != null)
            {
                var storedMessage = Store[text];
                if (text != null && Store != null && Store.ContainsKey(text))
                {
                    if (storedMessage != null)
                    {
                        storedMessage.LastSeenTime = DateTime.Now;
                        storedMessage.HowManyTimesWeSawIt++;
                    }
                }
                else
                {
                    if (Store != null)
                        lock (Store)
                        {
                            if (text != null)
                                if (message != null)
                                    if (message.Text != null)
                                        Store[text] = new StoredMessage
                                        (
                                            lastSeenTime: DateTime.Now,
                                            howManyTimesWeSawIt: 1,
                                            message: message.Text,
                                            allowedSpam: MessageAllowedStatusEnum.NOT_DEFINED_FOUND_IN_A_MESSAGE_SENT
                                        );
                        }
                }

                try
                {
                    if (Store != null)
                        if (storedMessage != null)
                            lock (storedMessage)
                            {
                                if (message != null && message.From != null && message != null && !storedMessage.FromUserId.Contains(message.From.Id))
                                    storedMessage.FromUserId.Add(message.From.Id);

                                if (message != null && !storedMessage.GroupsIdItHasBeenSentInto.Contains(message.Chat.Id))
                                    storedMessage.GroupsIdItHasBeenSentInto.Add(message.Chat.Id);

                                storedMessage.Messages.Add(message);

                                return Store.Count == 0 ? SpamType.UNDEFINED : storedMessage.IsSpam();
                            }
                }
                catch
                {
                    // ignored
                }
            }
        }

        return SpamType.UNDEFINED;
    }

    internal static List<Message?>? GetMessages(string text)
    {
        try
        {
            if (Store != null)
            {
                var messages = Store[text]?.Messages;
                if (messages != null)
                    return messages;
            }
        }
        catch
        {
            // ignored
        }

        return null;
    }

    internal static async Task SendMessageDetailsAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message?.ReplyToMessage == null || string.IsNullOrEmpty(e.Message.ReplyToMessage.Text))
            return;

        if (Store != null && !Store.ContainsKey(e.Message.ReplyToMessage.Text))
        {
            Language language1 = new(new Dictionary<string, string?>
            {
                {
                    "it", "Non è stato trovato nessun messaggio"
                },
                {
                    "en", "There are no messages"
                }
            });
            if (sender != null)
                await sender.SendTextMessageAsync(e.Message.From?.Id, language1, ChatType.Private,
                    e.Message.From?.LanguageCode, ParseMode.Html, null, e.Message.From?.Username);
            return;
        }

        if (Store != null)
        {
            var storedMessage = Store[e.Message.ReplyToMessage.Text];
            var json = storedMessage?.ToJson();
            Language? language2 = new(new Dictionary<string, string?>
            {
                {
                    "en", "Messages"
                },
                {
                    "it", "Messaggi"
                }
            });

            if (json != null)
            {
                var stream = UtilsFileText.GenerateStreamFromString(json);
                var tf = new TelegramFile(stream, "messagesSent.json", "Messages", "text/plain");
                PeerAbstract peer = new(e.Message.From?.Id, e.Message.Chat.Type);
                if (sender != null)
                    await sender.SendFileAsync(tf, peer, language2, TextAsCaption.AS_CAPTION, e.Message.From?.Username,
                        e.Message.From?.LanguageCode, null, true);
            }
        }
    }

    /// <summary>
    ///     Veto message button callback
    /// </summary>
    /// <param name="message"></param>
    /// <returns>true if the veto was on time, false otherwise</returns>
    public static bool VetoMessage(string message)
    {
        if (Store != null)
        {
            var storedMessage = Store![message];
            if (storedMessage != null)
            {
                var allowedTime = storedMessage.AllowedStatus.GetAllowedTime();
                if (allowedTime != null && allowedTime.Value < DateTime.Now) return false;
            }
        }

        if (Store != null) Store[message]?.RemoveMessage(true);
        return true;
    }

    /// <summary>
    /// </summary>
    /// <param name="message"></param>
    /// <returns>True if message is Allowed to be sent</returns>
    public static bool MessageIsAllowed(string message)
    {
        if (Store != null)
        {
            var storedMessage = Store[message];
            return storedMessage != null && Store != null && storedMessage.AllowedStatus.GetStatus() == MessageAllowedStatusEnum.ALLOWED;
        }

        return false;
    }

#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati

    private static void DisallowMessage(string message)
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati
    {
        if (Store != null)
        {
            var storedMessage = Store[message];
            if (storedMessage != null) 
                storedMessage.RemoveMessage(false);
        }
    }

    public static void AllowMessageOwner(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;
        AllowMessage(text);
        if (Store != null)
        {
            var storedMessage = Store[text];
            if (storedMessage != null) storedMessage.ForceAllowMessage();
        }
    }

    public static bool CanBeVetoed(string message)
    {
        var storedMessage = Store?[message];
        return storedMessage != null && Store != null && storedMessage.InsertedTime.AddHours(48) >= DateTime.Now;
    }

    internal static void BackupToFile()
    {
        try
        {
            File.WriteAllText(Paths.Data.MessageStore, JsonConvert.SerializeObject(Store));
        }
        catch (Exception? ex)
        {
            Logger.WriteLine(ex, LogSeverityLevel.CRITICAL);
        }
    }

    public static DateTime? GetAllowedTime(string message)
    {
        if (Store != null)
        {
            var storedMessage = Store[message];
            if (storedMessage != null) return storedMessage.AllowedStatus.GetAllowedTime();
        }

        return null;
    }
}