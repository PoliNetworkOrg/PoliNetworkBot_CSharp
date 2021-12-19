#region

using PoliNetworkBot_CSharp.Code.Bots.Anon;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Errors;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class RestrictUser
    {
        internal static async Task Mute(int time, TelegramBotAbstract telegramBotClient, long chatId, long? userId,
            ChatType chatType)
        {
            var untilDate = DateTime.Now.AddSeconds(time);
            await Mute2Async(untilDate, telegramBotClient, chatId, userId, chatType);
        }

        private static async Task Mute2Async(DateTime? untilDate, TelegramBotAbstract telegramBotClient, long chatId,
            long? userId, ChatType? chatType)
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

            if (untilDate == null)
                await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, default, chatType);
            else
                await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate.Value, chatType);
        }

        internal static async Task<Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long>> BanAllAsync(
            TelegramBotAbstract sender, MessageEventArgs e,
            string target, RestrictAction banTarget, DateTime? until,
            bool? revokeMessage)
        {
            var targetId = await Info.GetTargetUserIdAsync(target, sender);
            if (targetId == null || targetId.GetID() == null)
            {
                var exception2 = "";
                if (targetId != null) exception2 += "\n" + targetId.GetError();

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
                    ParseMode.Html,
                    e.Message.MessageId);
                return null;
            }

            const string q1 = "SELECT id, type FROM Groups";
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
                    ParseMode.Html,
                    e.Message.MessageId);
                return null;
            }

            await AlertActionStartedAsync(sender, target, e);

            var done = new List<DataRow>();
            var failed = new List<DataRow>();

            var exceptions = new List<ExceptionNumbered>();

            var nExceptions = 0;

            const int TIME_SLEEP_BETWEEN_BAN_UNBAN = 10;

            switch (banTarget)
            {
                case RestrictAction.BAN:
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Thread.Sleep(TIME_SLEEP_BETWEEN_BAN_UNBAN);
                            try
                            {
                                var groupChatId = (long)dr["id"];
                                var success = await BanUserFromGroup(sender, targetId.GetID().Value, groupChatId, null,
                                    revokeMessage);
                                if (success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success.ContainsExceptions())
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                            }
                            catch
                            {
                                ;
                            }
                        }

                        break;
                    }

                case RestrictAction.UNBAN:
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Thread.Sleep(TIME_SLEEP_BETWEEN_BAN_UNBAN);
                            try
                            {
                                var groupChatId = (long)dr["id"];
                                var success = await UnBanUserFromGroup(sender, targetId.GetID().Value, groupChatId);
                                if (success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success.ContainsExceptions())
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                            }
                            catch
                            {
                                ;
                            }
                        }

                        break;
                    }

                case RestrictAction.MUTE:
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            Thread.Sleep(TIME_SLEEP_BETWEEN_BAN_UNBAN);
                            try
                            {
                                var groupChatId = (long)dr["id"];
                                var chatType = GetChatType(dr);
                                var success = await MuteUser(sender, targetId.GetID().Value, groupChatId, until,
                                    chatType);
                                if (success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success.ContainsExceptions())
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                            }
                            catch
                            {
                                ;
                            }
                        }

                        break;
                    }
            }

            LogBanAction(targetId.GetID().Value, banTarget, sender, e.Message.From.Id);

            var targetId2 = targetId.GetID();
            var r6 = new Tuple<List<ExceptionNumbered>, int>(exceptions, nExceptions);
            if (targetId2 == null)
            {
                await NotifyUtil.NotifyOwnersAsync(r6, sender, "Ban/Unban All of [UNKNOWN]",
                    e.Message.From.LanguageCode);
            }
            else
            {
                var link2 = "tg://user?id=" + targetId2.Value;
                await NotifyUtil.NotifyOwnersAsync(r6, sender,
                    "Ban/Unban All of [<a href='" + link2 + "'>" + targetId2.Value + "</a>]",
                    e.Message.From.LanguageCode);
            }

            var r5 = new BanUnbanAllResult(done, failed);
            return new Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long>(r5, exceptions, nExceptions);
        }

        private static ChatType? GetChatType(DataRow dr)
        {
            try
            {
                var o1 = dr["type"].ToString().ToLower();
                if (o1 == null)
                    return null;
                if (o1 == "supergroup")
                    return ChatType.Supergroup;
                if (o1 == "group")
                    return ChatType.Group;
                if (o1 == "channel")
                    return ChatType.Channel;
                if (o1 == "private")
                    return ChatType.Private;
            }
            catch
            {
                ;
            }

            return null;
        }

        private static async Task<SuccessWithException> MuteUser(TelegramBotAbstract sender,
            long value, long groupChatId, DateTime? until, ChatType? chatType)
        {
            try
            {
                await Mute2Async(until, sender, groupChatId, value, chatType);
            }
            catch (Exception ex)
            {
                return new SuccessWithException(false, ex);
            }

            return new SuccessWithException(true);
        }

        private static async Task AlertActionStartedAsync(TelegramBotAbstract sender, string target, MessageEventArgs e)
        {
            var text7 = new Language(new Dictionary<string, string>
            {
                {
                    "en", "Action started: '" + target + "'"
                },
                {
                    "it", "Azione avviata: '" + target + "'"
                }
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username,
                text7,
                ParseMode.Html,
                e.Message.MessageId);
        }

        private static int AddExceptionIfNeeded(ref List<ExceptionNumbered> exceptions, ExceptionNumbered item2)
        {
            if (item2 == null)
                return 0;

            var isPresent = FindIfPresentSimilarException(exceptions, item2);
            if (isPresent.Item1 == false)
                exceptions.Add(new ExceptionNumbered(item2));
            else
                exceptions[isPresent.Item2].Increment();

            return 1;
        }

        private static Tuple<bool, int> FindIfPresentSimilarException(List<ExceptionNumbered> exceptions,
            ExceptionNumbered item2)
        {
            for (var i = 0; i < exceptions.Count; i++)
            {
                var e1 = exceptions[i];
                if (AreTheySimilar(e1, item2)) return new Tuple<bool, int>(true, i);
            }

            return new Tuple<bool, int>(false, -1);
        }

        private static bool AreTheySimilar(ExceptionNumbered e1, Exception item2)
        {
            return e1.AreTheySimilar(item2);
        }

        private static bool LogBanAction(long targetId, RestrictAction banned_true_unbanned_false,
            TelegramBotAbstract bot, long who_banned)
        {
            if (banned_true_unbanned_false == RestrictAction.BAN || banned_true_unbanned_false == RestrictAction.UNBAN)
                // ban/unban action

                try
                {
                    bool? b = null;
                    if (banned_true_unbanned_false == RestrictAction.BAN)
                        b = true;
                    else if (banned_true_unbanned_false == RestrictAction.UNBAN) b = false;

                    var q =
                        "INSERT INTO Banned (from_bot_id, who_banned, when_banned, target, banned_true_unbanned_false) " +
                        " VALUES (@fbi, @whob, @whenb, @target, @btuf)";

                    var dict = new Dictionary<string, object>
                    {
                        {"@fbi", bot.GetId()},
                        {"@whob", who_banned},
                        {"@whenb", DateTime.Now},
                        {"@target", targetId},
                        {"@btuf", StringUtil.ToSN(b)}
                    };
                    var done = SqLite.Execute(q, dict);

                    if (done > 0)
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }

            return false;
        }

        private static async Task<SuccessWithException> UnBanUserFromGroup(TelegramBotAbstract sender, long target,
            long groupChatId)
        {
            return await sender.UnBanUserFromGroup(target, groupChatId);
        }

        public static async Task<SuccessWithException> BanUserFromGroup(TelegramBotAbstract sender,
            long target,
            long groupChatId, string[] time,
            bool? revokeMessage)
        {
            return await sender.BanUserFromGroup(target, groupChatId, time, revokeMessage);
        }
    }
}