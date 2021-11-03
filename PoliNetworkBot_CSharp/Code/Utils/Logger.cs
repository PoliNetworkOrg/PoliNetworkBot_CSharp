using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class Logger
    {
        private static readonly Dictionary<long, TelegramBotAbstract> Subscribers = new();
        private static readonly BufferBlock<MessageQueue> Buffer = new();


        internal static async Task MainMethodAsync()
        {
            while (await Buffer.OutputAvailableAsync())
            {
                try
                {
                    var messageToBeSent = await Buffer.ReceiveAsync();
                    await messageToBeSent.key.Value.SendTextMessageAsync(messageToBeSent.key.Key, messageToBeSent.text, 
                        messageToBeSent.ChatType, messageToBeSent.Language, messageToBeSent.Parsemode, 
                        null, messageToBeSent.Username, null);
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
            if (log == null || string.IsNullOrEmpty(log.ToString()))
                return;
            try
            {
                Console.WriteLine(log);
                foreach (KeyValuePair<long, TelegramBotAbstract> subscriber in Subscribers)
                {
                    var text = new Language(new Dictionary<string, string>
                    {
                        {"un", log.ToString()}
                    });

                    Buffer.Post(
                        new MessageQueue(
                                subscriber, text, ChatType.Private,
                                "dc", ParseMode.Default,
                                "un"
                            )
                        );
                }
                if (Directory.Exists("./data/") == false)
                {
                    Directory.CreateDirectory("./data/");
                }
                if (!File.Exists("./data/log.txt"))
                {
                    File.WriteAllText("./data/log.txt", "");
                }
                File.AppendAllLinesAsync("./data/log.txt", new[] { log.ToString() });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task Subscribe(long fromId, TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
                Subscribers.TryAdd(fromId, telegramBotAbstract);
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