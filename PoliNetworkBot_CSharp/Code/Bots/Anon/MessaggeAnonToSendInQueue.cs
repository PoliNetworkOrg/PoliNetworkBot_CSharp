using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
            if (e != null) return e.Message.From.Username;

            return null;
        }

        internal string GetLanguageCode()
        {
            if (e != null) return e.Message.From.LanguageCode;

            return null;
        }

        internal bool FromTelegram()
        {
            return e != null;
        }

        internal Message GetMessage()
        {
            if (e != null) return e.Message;

            return null;
        }

        internal long? GetFromUserId()
        {
            if (e != null) return e.Message.From.Id;

            return null;
        }

        internal long? GetFromUserIdOrPostId()
        {
            if (e != null) return e.Message.From.Id;

            if (e2 != null) return e2.postid;

            return null;
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
                {"en", e2.text}
            });
            var m1 = await telegramBotAbstract.SendTextMessageAsync(ConfigAnon.ModAnonCheckGroup, text,
                ChatType.Group, "en", ParseMode.Html, null, null);
            return m1;
        }
    }
}