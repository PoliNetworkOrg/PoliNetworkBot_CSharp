#region

using PoliNetworkBot_CSharp.Code.Errors;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class RestrictUser
    {
        internal static async Task Mute(int time, TelegramBotAbstract telegramBotClient, long chatId, int userId, ChatType chatType)
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
            await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate, chatType);
        }

        internal static async Task<List<DataRow>> BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string target, bool banTarget)
        {
            var targetId = await Info.GetTargetUserIdAsync(target, sender);
            if (targetId == null)
            {
                var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en", "We were not able to BanAll the target '" + target + "', error code " +
                              ErrorCodes.TargetInvalidWhenBanAll
                    },
                    {
                        "it", "Non siamo riusciti a bannareAll il target '" + target + "', error code " +
                              ErrorCodes.TargetInvalidWhenBanAll
                    }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username,
                    text2);
                return null;
            }

            const string q1 = "SELECT id FROM Groups";
            var dt = SqLite.ExecuteSelect(q1);
            if (dt == null || dt.Rows.Count == 0)
            {
                var text3 = new Language(new Dictionary<string, string>
                {
                    {
                        "en", "We were not able to BanAll the target '" + target + "', error code " +
                              ErrorCodes.DatatableEmptyWhenBanAll
                    },
                    {
                        "it", "Non siamo riusciti a bannareAll il target '" + target + "', error code " +
                              ErrorCodes.DatatableEmptyWhenBanAll
                    }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
e.Message.From.LanguageCode,
e.Message.From.Username,
                    text3);
                return null;
            }

            var done = new List<DataRow>();

            if (banTarget)
                foreach (DataRow dr in dt.Rows)
                    try
                    {
                        var groupChatId = (long)dr["id"];
                        var success = await BanUserFromGroup(sender, e, targetId.Value, groupChatId, null);
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
                        var groupChatId = (long)dr["id"];
                        var success = await UnBanUserFromGroup(sender, e, targetId.Value, groupChatId);
                        if (success)
                            done.Add(dr);
                    }
                    catch
                    {
                        ;
                    }

            return done;
        }

        private static async Task<bool> UnBanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target,
            long groupChatId)
        {
            return await sender.UnBanUserFromGroup(target, groupChatId, e);
        }

        public static async Task<bool> BanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, long target,
            long groupChatId, string[] time)
        {
            return await sender.BanUserFromGroup(target, groupChatId, e, time);
        }
    }
}