using PoliNetworkBot_CSharp.Code.Objects;
using System;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Bots.Anon
{
    internal class MessaggeAnonToSendInQueue
    {
        private MessageEventArgs e;
        private WebPost e2;

        public MessaggeAnonToSendInQueue(MessageEventArgs e)
        {
            this.e = e;
        }

        public MessaggeAnonToSendInQueue(WebPost webPost)
        {
            this.e2 = webPost;
        }

        internal string getUsername()
        {
            if (this.e != null)
            {
                return this.e.Message.From.Username;
            }

            return null;
        }

        internal string getLanguageCode()
        {
            if (this.e != null)
            {
                return this.e.Message.From.LanguageCode;
            }

            return null;
        }

        internal bool FromTelegram()
        {
            return this.e != null;
        }

        internal Message getMessage()
        {
            if (this.e != null)
            {
                return this.e.Message;
            }

            return null;
        }

        internal long? getFromUserId()
        {
            if (this.e != null)
            {
                return this.e.Message.From.Id;
            }

            return null;
        }

        internal long? getFromUserIdOrPostId()
        {
            if (this.e != null)
            {
                return this.e.Message.From.Id;
            }

            if (this.e2 != null)
            {
                return this.e2.postid;
            }

            return null;
        }

        internal async System.Threading.Tasks.Task<MessageSentResult> SendMessageInQueueAsync(Objects.TelegramBotAbstract telegramBotAbstract)
        {
            if (telegramBotAbstract == null)
                return null;

            if (this.e2 != null)
            {
                return await SendMessageInQueue2Async(telegramBotAbstract);
            }

            return null;
        }

        private async System.Threading.Tasks.Task<MessageSentResult> SendMessageInQueue2Async(TelegramBotAbstract telegramBotAbstract)
        {
            Language text = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                {"en", this.e2.text }
            });
            var m1 = await telegramBotAbstract.SendTextMessageAsync(Code.Bots.Anon.ConfigAnon.ModAnonCheckGroup, text,
                Telegram.Bot.Types.Enums.ChatType.Group, "en", Telegram.Bot.Types.Enums.ParseMode.Html, null, null, null, false);
            return m1;
        }
    }
}