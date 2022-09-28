using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Materials;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class ConfigUtil
{
    public static async Task<bool> GetConfig(long? fromId, string? fromUsername, TelegramBotAbstract? sender, string? fromLanguageCode, ChatType? chatType)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(PoliNetworkBot_CSharp.Code.MainProgram.Program.BotConfigAll);
        var file = TelegramFile.FromString(json, "config.json", "");
        if (chatType == null) 
            return false;
        
        var peer = new PeerAbstract(fromId, chatType.Value);
        var text = new Language(new Dictionary<string, string?>());
        return await Utils.SendMessage.SendFileAsync(file, peer, text, TextAsCaption.AS_CAPTION, sender, fromUsername,
            fromLanguageCode, null, true, ParseMode.Html);

    }
}