using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public static class LoggerSendFile
{
    private static readonly Dictionary<long, object> DictLock = new();

    public static int SendFiles(List<long?> sendTo,
        string fileContent,
        TelegramBotAbstract? sender,
        string textToSendBefore, string applicationOctetStream, string fileName)
    {
        var id = sender?.GetId();
        if (id == null)
        {
            Console.WriteLine("Failed PoliNetworkBot_CSharp.Code.Utils.Logger.SendFiles BOT ID NULL");
            return -1;
        }

        if (!DictLock.ContainsKey(id.Value))
            DictLock[id.Value] = new object();

        lock (DictLock[id.Value])
        {
            return SendFilesBehindLock(textToSendBefore, sendTo, fileContent, applicationOctetStream, sender, fileName);
        }
    }

    private static int SendFilesBehindLock(string textToSendBefore, List<long?> sendTo, string fileContent,
        string fileMimeType, TelegramBotAbstract? sender, string fileName)
    {
        try
        {
            var encoding = Encoding.UTF8;
            var done = 0;

            var text2 = new Language(new Dictionary<string, string?>
            {
                { "uni", textToSendBefore }
            });

            foreach (var sendToSingle in sendTo)
                try
                {
                    var peer = new PeerAbstract(sendToSingle, ChatType.Private);

                    var stream = new MemoryStream(encoding.GetBytes(fileContent));


                   
                    SendMessage.SendFileAsync(new TelegramFile(stream, fileName,
                            null, fileMimeType), peer,
                        text2, TextAsCaption.BEFORE_FILE,
                        sender, null, "it", null, true);

                    done++;
                }
                catch (Exception ex)
                {
                    Logger.WriteLine(ex);
                }

            return done;
        }
        catch (Exception ex2)
        {
            Logger.WriteLine(ex2);
        }

        return -1;
    }
}