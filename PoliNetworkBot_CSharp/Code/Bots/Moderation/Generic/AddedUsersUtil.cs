#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.AbstractBot;
using PoliNetworkBot_CSharp.Code.Utils;
using Groups = PoliNetworkBot_CSharp.Code.Data.Constants.GroupsConstants;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Generic;

public static class AddedUsersUtil
{
    public static async Task DealWithAddedUsers(TelegramBotAbstract? telegramBotClient,
        MessageEventArgs? messageEventArgs)
    {
        try
        {
            if (telegramBotClient == null || messageEventArgs?.Message == null)
                return;

            var added = messageEventArgs.Message.NewChatMembers;
            if (added == null)
                return;

            var chatId = messageEventArgs.Message.Chat.Id;
            if (Groups.PianoDiStudi.FullLong().Equals(chatId))
                await RestrictUser.TryMuteUsers(telegramBotClient, messageEventArgs, added,
                    TimeSpan.FromMinutes(5));
        }
        catch
        {
            //ignored
        }
    }
}