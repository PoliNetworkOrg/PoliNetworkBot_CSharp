#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.Files;

public class TelegramFileContent
{
    private readonly string? _caption;
    private readonly StringJson? _fileContent;

    public TelegramFileContent(StringJson? fileContent, string? caption)
    {
        _fileContent = fileContent;
        _caption = caption;
    }

    public async Task<List<MessageSentResult?>?> SendToOwners(TelegramBotAbstract sender, string? langCode,
        long? replyToMessageId2, MessageEventArgs? messageEventArgs, FileTypeJsonEnum whatWeWant)
    {
        if ((_fileContent == null || _fileContent.IsEmpty()) && string.IsNullOrEmpty(_caption)) return null;

        if (_fileContent == null || _fileContent.IsEmpty())
        {
            var text1 = new Language(new Dictionary<string, string?>
            {
                { "it", "Eccezione! " + _caption },
                { "en", "Exception! " + _caption }
            });

            var r11 = await NotifyUtil.NotifyOwners_AnError_AndLog2(text1, sender, langCode, replyToMessageId2,
                messageEventArgs,
                _fileContent, whatWeWant);
            return r11;
        }


        if (string.IsNullOrEmpty(_caption))
            await NotifyUtil.SendString(
                _fileContent, messageEventArgs, sender,
                "ex.json", "", replyToMessageId2, ParseMode.Html, whatWeWant);

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Eccezione! " + _caption },
            { "en", "Exception! " + _caption }
        });

        var r1 = await NotifyUtil.NotifyOwners_AnError_AndLog2(text, sender, langCode, replyToMessageId2,
            messageEventArgs, null,
            null);
        return r1;
    }

    public StringJson? GetFileContentStringJson()
    {
        return _fileContent;
    }
}