using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

public static class AddedUsersUtil
{
    public static async Task DealWithAddedUsers(TelegramBotAbstract? telegramBotClient, MessageEventArgs? messageEventArgs)
    {
        try
        {
            if (telegramBotClient == null || messageEventArgs?.Message == null)
                return;

            var added = messageEventArgs.Message.NewChatMembers;
            if (added == null)
                return;

            var chatId = messageEventArgs.Message.Chat.Id;
            if (chatId == AutoReplyInGroups.ExcludedGroups[SpecialGroup.PIANO_DI_STUDI])
            {
                await Utils.RestrictUser.TryMuteUsers(telegramBotClient, messageEventArgs, added,
                    TimeSpan.FromMinutes(5));
            }
        }
        catch
        {
            //ignored
        }

    }
}