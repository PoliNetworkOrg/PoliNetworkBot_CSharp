using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class Logger
    {
        private static readonly Dictionary<long, TelegramBotAbstract> Subscribers = new();
        public static void WriteLine(object log)
        {
            try
            {
                Console.WriteLine(log);

                foreach (var subscriber in Subscribers)
                {
                    var text = new Language(new Dictionary<string, string>
                    {
                        {"un", "You have to reply to a message containing the message"}
                    });
                    subscriber.Value.SendTextMessageAsync(subscriber.Key, text, ChatType.Private,
                        "dc", ParseMode.Html, null, "un",
                        null);
                }
                if (log != null)
                    File.AppendAllLinesAsync("./data/log.txt", new[] { log.ToString() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void Subscribe(long fromId, TelegramBotAbstract telegramBotAbstract)
        {
            Subscribers.Add(fromId, telegramBotAbstract);
        }

        public static void Unsubscribe(long fromId)
        {
            Subscribers.Remove(fromId);
        }

        public static async Task PrintLog(TelegramBotAbstract sender, long sendTo)
        {
            const string path = "./data/log.txt";
            var file = await File.ReadAllBytesAsync(path);

            var stream = new MemoryStream(file);

            var text2 = new Language(new Dictionary<string, string>
            {
                {"it", "LOG:"}
            });

            TLAbsInputPeer peer2 = new TLInputPeerUser { UserId = (int)sendTo };
            var peer = new Tuple<TLAbsInputPeer, long>(peer2, sendTo);

            await SendMessage.SendFileAsync(new TelegramFile(stream, "log.log",
                    null, "application/octet-stream"), peer,
                text2, TextAsCaption.BEFORE_FILE,
                sender, null, "it", null, true);
            
            await File.WriteAllTextAsync(path, "");
        }
    }
}