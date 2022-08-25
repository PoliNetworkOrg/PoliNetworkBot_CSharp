#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;
using Groups = PoliNetworkBot_CSharp.Code.Data.Constants.Groups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;

public static class AutoReplyInGroups
{

    private static readonly List<AutomaticAnswer> _automaticAnswers = new()
    {
        new AutomaticAnswer(new List<List<string>>
        {
            new() {"ciao", "bye"},
            new() {"abc", "cde"},
        }, Reply, 
            new List<long> {Groups.PianoDiStudi, Groups.AskPolimi}, 
            "Ciao 👋 sembra tu stia chiedendo domande in merito al piano di studi. " +
                        "PoliNetwork ti consiglia di scrivere nel gruppo dedicato, " +
                        "<a href='https://t.me/joinchat/aiAC6RgOjBRkYjhk'>clicca qui</a>!"),
        new AutomaticAnswer(new List<List<string>>
            {
                new() {"ciao1", "bye1"},
                new() {"abc", "cde"},
            }, Reply, 
            new List<long> {Groups.Testing}, 
            "Ciao 1 bye1!"),
        new AutomaticAnswerRestricted(new List<List<string>>
            {
                new() {"link", "esiste"},
                new() {"whatsapp"},
                new() {"grupp"}
            }, Reply, 
            new List<long> {Groups.PianoDiStudi, Groups.AskPolimi}, 
            "Controlla i messaggi fissati", 
            e => e.Message.Chat.Title != null && e.Message.Chat.Title.ToLower().Contains("matricole"))
    };

    private static async Task Reply(MessageEventArgs e, TelegramBotAbstract? telegramBotClient, string message)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            {
                "uni",
                message
            }
        });
        await SendMessage.SendMessageInAGroup(telegramBotClient,
            "uni",
            text,
            e,
            e.Message.Chat.Id,
            e.Message.Chat.Type,
            ParseMode.Html,
            e.Message.MessageId,
            true);
    }
    
    internal static void MessageInGroup2Async(TelegramBotAbstract? telegramBotClient, MessageEventArgs? e,
        string text)
    {
        foreach (var answer in _automaticAnswers)
        {
            if (e == null || telegramBotClient == null) continue;
            answer.TryTrigger(e, telegramBotClient, text);
        }
    }

}