using System;
using System.Collections.Generic;
using System.Data;
using Telegram.Bot.Args;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class RestrictUser
    {
        internal static void Mute(int time, TelegramBotAbstract telegramBotClient, long chat_id, int user_id)
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
            telegramBotClient.RestrictChatMemberAsync(chat_id, user_id, permissions, untilDate);
        }

        internal static List<DataRow> BanAll(TelegramBotAbstract sender, MessageEventArgs e, string target, bool ban_target)
        {
            int? target_id = Info.GetTargetUserId(target, sender);
            if (target_id == null)
            {
                Utils.SendMessage.SendMessageInPrivate(sender, e,
                    "We were not able to BanAll the target '" + target + "', error code " + Errors.ErrorCodes.target_invalid_when_banall);
                return null;
            }

            string q1 = "SELECT id FROM Groups";
            var dt = Utils.SQLite.ExecuteSelect(q1);
            if (dt == null || dt.Rows.Count == 0)
            {
                Utils.SendMessage.SendMessageInPrivate(sender, e,
                    "We were not able to BanAll the target '" + target + "', error code " + Errors.ErrorCodes.datatable_empty_when_banall);
                return null;
            }

            List<DataRow> done = new List<DataRow>();

            if (ban_target)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        long group_chat_id = (long)dr["id"];
                        bool success = BanUserFromGroup(sender, e, target_id.Value, group_chat_id);
                        if (success)
                            done.Add(dr);
                    }
                    catch
                    {
                        ;
                    }
                }
            }
            else
            {
                foreach (DataRow dr in dt.Rows)
                {
                    try
                    {
                        long group_chat_id = (long)dr["id"];
                        bool success = UnBanUserFromGroup(sender, e, target_id.Value, group_chat_id);
                        if (success)
                            done.Add(dr);
                    }
                    catch
                    {
                        ;
                    }
                }
            }

            return done;
        }

        private static bool UnBanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target, long group_chat_id)
        {
            return sender.UnBanUserFromGroup(target, group_chat_id, e);
        }

        private static bool BanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target, long group_chat_id)
        {
            return sender.BanUserFromGroup(target, group_chat_id, e);
        }
    }
}