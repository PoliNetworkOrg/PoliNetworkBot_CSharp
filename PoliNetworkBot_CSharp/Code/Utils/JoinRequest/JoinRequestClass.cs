using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.InfoBot;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils.JoinRequest;

public static class JoinRequestClass
{
    public static void JoinHandle(Update update, BotClientWhole botClientWhole)
    {
        //todo: eventualmente gestire le richieste di ingresso ai gruppi
        ;

        var telegramBotAbstract = TelegramBotAbstract.From(botClientWhole);

        var updateChatJoinRequest = update.ChatJoinRequest;
        var vuoiJoinareIlGruppo = "Vuoi joinare il gruppo?\n\n" + updateChatJoinRequest?.Chat.Title;
        Language text = new L(vuoiJoinareIlGruppo);
        var sendTextMessageAsync = telegramBotAbstract.SendTextMessageAsync(updateChatJoinRequest?.From.Id, text, ChatType.Private, null,
            ParseMode.Html,
            null, null, null);
        sendTextMessageAsync.Wait();
        ;
    }
}