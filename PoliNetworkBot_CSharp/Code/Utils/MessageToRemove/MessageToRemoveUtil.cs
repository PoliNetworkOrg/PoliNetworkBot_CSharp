using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;

namespace PoliNetworkBot_CSharp.Code.Utils.MessageToRemove;

public static class MessageToRemoveUtil
{
    public static void AddMessageToDelete(long chatId, TelegramBotAbstract telegramBotAbstract, long? messageId,
        DateTime deleteWhen)
    {
        if (messageId == null)
            return;
        
        const string q = """
                         INSERT INTO MessagesToRemove (message_id , chat_id, bot_id, inserted_when, delete_when)
                         VALUES (@message_id, @chat_id, @bot_id, @inserted_when, @delete_when)
                         """;
        var idBot = telegramBotAbstract.GetId();
        var keyValuePairs = new Dictionary<string, object?>
        {
            { "@message_id", messageId },
            { "@chat_id", chatId },
            { "@bot_id", idBot },
            { "@inserted_when", DateTime.Now },
            { "@delete_when", deleteWhen },
        };
        Database.Execute(q, telegramBotAbstract?.DbConfig, keyValuePairs);
    }
}