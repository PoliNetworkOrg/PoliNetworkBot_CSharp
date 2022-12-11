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
    public static int SendFiles(List<long?> sendTo,
        string fileContent,
        TelegramBotAbstract? sender,
        string textToSendBefore, string applicationOctetStream)
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


                SendMessage.SendFileAsync(new TelegramFile(stream, "log.log",
                        null, applicationOctetStream), peer,
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
}