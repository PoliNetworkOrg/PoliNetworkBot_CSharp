#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class MessaggeAnonToSendInQueue
    {
        private readonly MessageEventArgs e;
        private readonly WebPost e2;

        public MessaggeAnonToSendInQueue(MessageEventArgs e)
        {
            this.e = e;
        }

        public MessaggeAnonToSendInQueue(WebPost webPost)
        {
            e2 = webPost;
        }

        internal string GetUsername()
        {
            return e?.Message.From.Username;
        }

        internal string GetLanguageCode()
        {
            return e?.Message.From.LanguageCode;
        }

        internal bool FromTelegram()
        {
            return e != null;
        }

        internal Message GetMessage()
        {
            return e?.Message;
        }

        internal long? GetFromUserId()
        {
            return e?.Message.From.Id;
        }

        internal long? GetFromUserIdOrPostId()
        {
            return e != null ? e.Message.From.Id : e2?.postid;
        }

        internal async Task<MessageSentResult> SendMessageInQueueAsync(TelegramBotAbstract telegramBotAbstract)
        {
            if (telegramBotAbstract == null)
                return null;

            if (e2 != null) return await SendMessageInQueue2Async(telegramBotAbstract);

            return null;
        }

        private async Task<MessageSentResult> SendMessageInQueue2Async(TelegramBotAbstract telegramBotAbstract)
        {
            var text = new Language(new Dictionary<string, string>
            {
                { "en", e2.text }
            });
            var m1 = await telegramBotAbstract.SendTextMessageAsync(ConfigAnon.ModAnonCheckGroup, text,
                ChatType.Group, "en", ParseMode.Html, null, null);
            return m1;
        }
    }
}