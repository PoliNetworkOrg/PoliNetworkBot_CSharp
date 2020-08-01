using System;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class RestrictUser
    {
        internal static void Mute(int time, TelegramBotAbstract telegramBotClient, Telegram.Bot.Args.MessageEventArgs e)
        {
            Telegram.Bot.Types.ChatPermissions permissions = new Telegram.Bot.Types.ChatPermissions
            {
                CanSendMessages = false,
                CanInviteUsers = true,
                CanSendOtherMessages = false,
                CanSendPolls = false,
                CanAddWebPagePreviews = false,
                CanChangeInfo = false,
                CanPinMessages = false,
                CanSendMediaMessages = false
            };
            DateTime untilDate = DateTime.Now.AddSeconds(time);
            telegramBotClient.RestrictChatMemberAsync(e.Message.Chat.Id, e.Message.From.Id, permissions, untilDate);
        }
    }
}