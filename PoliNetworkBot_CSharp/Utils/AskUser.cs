using PoliNetworkBot_CSharp.Objects;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class AskUser
    {
        public static Dictionary<int, AnswerTelegram> userAnswers = new Dictionary<int, AnswerTelegram>();
            
        internal static string Ask(int id, Dictionary<string, string> dictionary, TelegramBotAbstract sender, string lang)
        {
            string to_send = dictionary[lang];
            sender.SendTextMessageAsync(id, to_send, Telegram.Bot.Types.Enums.ChatType.Private,  v:default, force_reply : true);
            userAnswers[id] = new AnswerTelegram();
            while (userAnswers[id].GetState() == AnswerTelegram.State.WaitingForAnswer)
            {
                Thread.Sleep(3000);
            }

            return userAnswers[id].GetAnswer();
        }
    }
}