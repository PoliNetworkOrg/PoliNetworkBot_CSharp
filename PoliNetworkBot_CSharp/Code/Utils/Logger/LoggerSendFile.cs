using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils.Logger;

public static class LoggerSendFile
{
    private static readonly Dictionary<long, object> DictLock = new();

    public static int SendFiles(List<long?> sendTo,
        StringOrStream fileContent,
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

    private static int SendFilesBehindLock(string textToSendBefore,
        List<long?> sendTo, StringOrStream fileContent,
        string fileMimeType, TelegramBotAbstract? sender, string fileName)
    {
        try
        {
            const string lang = "uni";
            var done = 0;

            var text2 = new Language(new Dictionary<string, string?>
            {
                { lang, textToSendBefore }
            });

            foreach (var sendToSingle in sendTo)
                try
                {
                    var peer = new PeerAbstract(sendToSingle, ChatType.Private);

                    var stream = fileContent.GetStream();


                    SendMessage.SendFileAsync(new TelegramFile(stream, fileName,
                            text2, fileMimeType, TextAsCaption.AS_CAPTION), peer,
                        sender, null, lang, null, true);

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