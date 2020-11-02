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

        internal static async Task<Tuple<List<DataRow>, List<ExceptionNumbered>>> BanAllAsync(TelegramBotAbstract sender, MessageEventArgs e,
            string target, bool banTarget)
        {
            UserIdFound targetId = await Info.GetTargetUserIdAsync(target, sender);
            if (targetId == null || targetId.GetID() == null)
            {
                string exception2 = "";
                if (targetId != null)
                {
                    exception2 += "\n" + targetId.getError();
                }

                var text2 = new Language(new Dictionary<string, string>
                {
                    {
                        "en", "We were not able to BanAll the target '" + target + "', error code " +
                              ErrorCodes.TargetInvalidWhenBanAll + exception2
                    },
                    {
                        "it", "Non siamo riusciti a bannareAll il target '" + target + "', error code " +
                              ErrorCodes.TargetInvalidWhenBanAll + exception2
                    }
                });
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text2,
                    parseMode: ParseMode.Default,
                    e.Message.MessageId);
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
                    text3,
                    parseMode: ParseMode.Default,
                    e.Message.MessageId);
                return null;
            }

            var done = new List<DataRow>();

            List<ExceptionNumbered> exceptions = new List<ExceptionNumbered>();

            if (banTarget)
                foreach (DataRow dr in dt.Rows)
                    try
                    {
                        var groupChatId = (long)dr["id"];
                        Tuple<bool, Exception> success = await BanUserFromGroup(sender, e, targetId.GetID().Value, groupChatId, null);
                        if (success.Item1)
                            done.Add(dr);

                        AddExceptionIfNeeded(ref exceptions, success.Item2);
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
                        Tuple<bool, Exception> success = await UnBanUserFromGroup(sender, e, targetId.GetID().Value, groupChatId);
                        if (success.Item1)
                            done.Add(dr);

                        AddExceptionIfNeeded(ref exceptions, success.Item2);
                    }
                    catch
                    {
                        ;
                    }

            LogBanAction(targetId.GetID().Value, banned_true_unbanned_false: banTarget, bot: sender, who_banned: e.Message.From.Id);

            var r4 = done;

            int? targetId2 = targetId.GetID();
            if (targetId2 == null)
            {
                await NotifyUtil.NotifyOwnersAsync(exceptions, sender, "Ban/Unban All of [UNKNOWN]", e.Message.From.LanguageCode);
            }
            else
            {
                string link2 = "tg://user?id=" + targetId2.Value.ToString();
                await NotifyUtil.NotifyOwnersAsync(exceptions, sender, "Ban/Unban All of [<a href='"+link2+"'>" + targetId2.Value.ToString() +"</a>]", e.Message.From.LanguageCode);
            }

            return new Tuple<List<DataRow>, List<ExceptionNumbered>>(r4, exceptions);
        }

        private static void AddExceptionIfNeeded(ref List<ExceptionNumbered> exceptions, Exception item2)
        {
            if (item2 == null)
                return;

            Tuple<bool,int> isPresent = FindIfPresentSimilarException(exceptions, item2);
            if (isPresent.Item1 == false)
                exceptions.Add(new ExceptionNumbered(item2));
            else
                exceptions[isPresent.Item2].Increment(); 
        }

        private static Tuple<bool, int> FindIfPresentSimilarException(List<ExceptionNumbered> exceptions, Exception item2)
        {
            for (int i = 0; i < exceptions.Count; i++)
            {
                ExceptionNumbered e1 = exceptions[i];
                if (AreTheySimilar(e1, item2))
                {
                    return new Tuple<bool, int>(true, i);
                }
            }

            return new Tuple<bool, int>(false, -1);
        }

        private static bool AreTheySimilar(ExceptionNumbered e1, Exception item2)
        {
            return e1.AreTheySimilar(item2);
        }

        private static bool LogBanAction(int targetId, bool banned_true_unbanned_false, TelegramBotAbstract bot, int who_banned)
        {
            try
            {
                string q = "INSERT INTO Banned (from_bot_id, who_banned, when_banned, target, banned_true_unbanned_false) " +
                    " VALUES (@fbi, @whob, @whenb, @target, @btuf)";

                Dictionary<string, object> dict = new Dictionary<string, object>() {
                    {"@fbi", bot.GetId() },
                    {"@whob", who_banned },
                    {"@whenb", DateTime.Now },
                    {"@target", targetId },
                    {"@btuf", Utils.StringUtil.ToSN(banned_true_unbanned_false)}
                };
                int done = Utils.SqLite.Execute(q, dict);

                if (done > 0)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<Tuple<bool, Exception>> UnBanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, int target,
            long groupChatId)
        {
            return await sender.UnBanUserFromGroup(target, groupChatId, e);
        }

        public static async Task<Tuple<bool, Exception>> BanUserFromGroup(TelegramBotAbstract sender, MessageEventArgs e, long target,
            long groupChatId, string[] time)
        {
            return await sender.BanUserFromGroup(target, groupChatId, e, time);
        }
    }
}