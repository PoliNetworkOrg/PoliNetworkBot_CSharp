using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Objects;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class MessageList
{
    public readonly List<Message?> Messages;
    public readonly Mutex Mutex;

    
    public MessageList()
    {
        Messages = new List<Message?>();
        Mutex = new Mutex();
    }

    public async Task TryDeleteMessagesAsync(TelegramBotAbstract telegramBotClient)
    {
        Mutex.WaitOne();
        try
        {
            foreach (var m in Messages)
            {
                try
                {
                    await Utils.DeleteMessage.DeleteIfMessageIsNotInPrivate(telegramBotClient, m);
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

        Mutex.ReleaseMutex();
    }

    public void Add(Message message)
    {
        Mutex.WaitOne();
        try
        {
            Messages.Add(message);
        }
        catch
        {
            // ignored
        }

        Mutex.ReleaseMutex();
    }
}