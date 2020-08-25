#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Errors;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Args;
using Telegram.Bot.Types;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class RestrictUser
    {
        internal static void Mute(int time, TelegramBotAbstract telegramBotClient, long chatId, int userId)
        {
            var permissions = new ChatPermissions
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
            var untilDate = DateTime.Now.AddSeconds(time);
            telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate);
        }

        internal static async Task<List<DataRow>> BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string target, bool banTarget)
        {
            var targetId = await Info.GetTargetUserIdAsync(target, sender);
            if (targetId == null)
            {
                SendMessage.SendMessageInPrivate(sender, e,
                    "We were not able to BanAll the target '" + target + "', error code " +
                    ErrorCodes.TargetInvalidWhenBanAll);
                return null;
            }

            const string q1 = "SELECT id FROM Groups";
            var dt = SqLite.ExecuteSelect(q1);
            if (dt == null || dt.Rows.Count == 0)
            {
                SendMessage.SendMessageInPrivate(sender, e,
                    "We were not able to BanAll the target '" + target + "', error code " +
                    ErrorCodes.DatatableEmptyWhenBanAll);
                return null;
            }

            var done = new List<DataRow>();

            if (banTarget)
                foreach (DataRow dr in dt.Rows)
                    try
                    {
                        var groupChatId = (long) dr["id"];
                        var success = BanUserFromGroup(sender, e, targetId.Value, groupChatId, null);
                        if (success)
                            done.Add(dr);
                    }
                    catch
                    {
                        ;
                    }
            else
                foreach (DataRow dr in dt.Rows)
                    try
                    {
                        var groupChatId = (long) dr["id"];
                        var success = UnBanUserFromGroup(sender, e, targetId.Value, groupChatId);
                        if (success)
                            done.Add(dr);
                    }
                    catch
                    {
                        ;
                    }

            return done;
        }

        private static bool UnBanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target,
            long groupChatId)
        {
            return sender.UnBanUserFromGroup(target, groupChatId, e);
        }

        public static bool BanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target,
            long groupChatId, string[] time)
        {
            return sender.BanUserFromGroup(target, groupChatId, e, time);
        }
    }
}