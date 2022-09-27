#region

using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;

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

    public async Task<List<MessageSentResult>?> Send(TelegramBotAbstract sender, int loopNumber, string? langCode,
        long? replyToMessageId2, MessageEventArgs? messageEventArgs)
    {
        if ((_fileContent == null || _fileContent.IsEmpty()) && string.IsNullOrEmpty(_caption)) return null;

        if (_fileContent == null || _fileContent.IsEmpty())
        {
            var text1 = new Language(new Dictionary<string, string?>
            {
                { "it", "Eccezione! " + _caption },
                { "en", "Exception! " + _caption }
            });

            var r11 = await NotifyUtil.NotifyOwners7(text1, sender, langCode, replyToMessageId2, messageEventArgs,
                _fileContent);
            return r11;
        }


        if (string.IsNullOrEmpty(_caption))
            await NotifyUtil.SendString(
                _fileContent, messageEventArgs, sender,
                "ex.json", "", replyToMessageId2
            );

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Eccezione! " + _caption },
            { "en", "Exception! " + _caption }
        });

        var r1 = await NotifyUtil.NotifyOwners7(text, sender, langCode, replyToMessageId2, messageEventArgs);
        return r1;
    }

    public StringJson? GetFileContentStringJson()
    {
        return this._fileContent;
    }
}