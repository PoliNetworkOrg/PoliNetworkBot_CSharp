#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageList
{
    public readonly List<Message?> Messages;


    public MessageList()
    {
        Messages = new List<Message?>();
    }

    public void TryDeleteMessagesAsync(AbstractBot.TelegramBotAbstract telegramBotClient)
    {
        lock (this)
        {
            try
            {
                foreach (var m in Messages)
                    try
                    {
                        var a = DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, m);
                        a.Wait();
                    }
                    catch
                    {
                        // ignored
                    }
            }
            catch
            {
                // ignored
            }
        }
    }

    public void Add(Message message)
    {
        lock (this)
        {
            try
            {
                Messages.Add(message);
            }
            catch
            {
                // ignored
            }
        }
    }
}