using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class Logger
    {
        private static readonly Dictionary<long, TelegramBotAbstract> Subscribers = new();
        private static readonly List<LoggerItem> Queue = new();

        internal static async Task MainMethodAsync()
        {
            while (true)
            {
                try
                {
                    LoggerItem loggerItem = null;
                    lock (Queue)
                    {
                        if (Queue.Count > 0)
                        {
                            int where = 0;
                            loggerItem = Queue[where];
                            Queue.RemoveAt(where);
                        }

                    }

                    if (loggerItem != null)
                    {
                        await loggerItem.key.Value.SendTextMessageAsync(loggerItem.key.Key, loggerItem.text, loggerItem.@private,
                            loggerItem.v1, loggerItem.html, null, loggerItem.v2, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR IN LOGGER APPLICATION!");
                    Console.WriteLine(e);
                    Console.WriteLine("------");
                }
            }
        }

        public static void WriteLine(object log)
        {
            try
            {
                Console.WriteLine(log);
                lock (Queue)
                {
                    foreach (KeyValuePair<long, TelegramBotAbstract> subscriber in Subscribers)
                    {
                        var text = new Language(new Dictionary<string, string>
                        {
                            {"un", log.ToString()}
                        });


                        Queue.Add(
                            new LoggerItem(
                                    subscriber, text, ChatType.Private,
                                    "dc", ParseMode.Html,
                                    "un"
                                )
                            );
                    }
                }
                if (log != null)
                    File.AppendAllLinesAsync("./data/log.txt", new[] { log.ToString() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task SubscribeAsync(long fromId, TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
                Subscribers.Add(fromId, telegramBotAbstract);
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            }
        }

        public static void Unsubscribe(long fromId)
        {
            try
            {
                Subscribers.Remove(fromId);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR IN LOGGER APPLICATION!");
                Console.WriteLine(e);
                Console.WriteLine("------");
            }
        }

        public static async Task PrintLog(TelegramBotAbstract sender, long sendTo)
        {
            try
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
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, sender);
            }
        }
    }
}