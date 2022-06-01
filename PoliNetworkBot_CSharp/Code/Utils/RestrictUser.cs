﻿#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Errors;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class RestrictUser
{
    internal static async Task Mute(int time, TelegramBotAbstract telegramBotClient, long chatId, long? userId,
        ChatType chatType, RestrictAction restrictAction)
    {
        var untilDate = DateTime.Now.AddSeconds(time);
        await Mute2Async(untilDate, telegramBotClient, chatId, userId, chatType, restrictAction);
    }

    private static async Task Mute2Async(DateTime? untilDate, TelegramBotAbstract telegramBotClient, long chatId,
        long? userId, ChatType? chatType, RestrictAction restrictAction)
    {
        var permissions = restrictAction switch
        {
            RestrictAction.BAN or RestrictAction.UNBAN => throw new ArgumentException(),
            RestrictAction.MUTE => new ChatPermissions
            {
                CanSendMessages = false,
                CanInviteUsers = true,
                CanSendOtherMessages = false,
                CanSendPolls = false,
                CanAddWebPagePreviews = false,
                CanChangeInfo = false,
                CanPinMessages = false,
                CanSendMediaMessages = false
            },
            RestrictAction.UNMUTE => new ChatPermissions
            {
                CanSendMessages = true,
                CanInviteUsers = true,
                CanSendOtherMessages = true,
                CanSendPolls = true,
                CanAddWebPagePreviews = true,
                CanChangeInfo = true,
                CanPinMessages = true,
                CanSendMediaMessages = true
            },
            _ => throw new ArgumentOutOfRangeException(nameof(restrictAction), restrictAction, null)
        };

        await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate, chatType);
    }

    internal static async Task<Tuple<BanUnbanAllResult, List<ExceptionNumbered>, long>> BanAllAsync(
        TelegramBotAbstract sender, MessageEventArgs e,
        string target, RestrictAction banTarget, DateTime? until,
        bool? revokeMessage)
    {
        var targetId = await Info.GetTargetUserIdAsync(target, sender);
        if (targetId?.GetId() == null)
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
            if (e.Message.From != null)
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text2,
                    ParseMode.Html,
                    e.Message.MessageId);
            return null;
        }

        const string q1 = "SELECT id, type FROM GroupsTelegram";
        var dt = Database.ExecuteSelect(q1, sender.DbConfig);
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
            if (e.Message.From != null)
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

        const int timeSleepBetweenBanUnban = 10;

        switch (banTarget)
        {
            case RestrictAction.BAN:
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Thread.Sleep(timeSleepBetweenBanUnban);
                    try
                    {
                        var groupChatId = (long)dr["id"];
                        var success = await BanUserFromGroup(sender, targetId.GetId().Value, groupChatId, null,
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
                    Thread.Sleep(timeSleepBetweenBanUnban);
                    try
                    {
                        var groupChatId = (long)dr["id"];
                        var success = await UnBanUserFromGroup(sender, targetId.GetId(), groupChatId);
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
                    Thread.Sleep(timeSleepBetweenBanUnban);
                    try
                    {
                        var groupChatId = (long)dr["id"];
                        var chatType = GetChatType(dr);
                        var success = await MuteUser(sender, targetId.GetId().Value, groupChatId, until,
                            chatType, RestrictAction.MUTE);
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
            case RestrictAction.UNMUTE:
            {
                foreach (DataRow dr in dt.Rows)
                {
                    Thread.Sleep(timeSleepBetweenBanUnban);
                    try
                    {
                        var groupChatId = (long)dr["id"];
                        var chatType = GetChatType(dr);
                        var success = await MuteUser(sender, targetId.GetId().Value, groupChatId, until,
                            chatType, RestrictAction.UNMUTE);
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
            }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(banTarget), banTarget, null);
        }

        LogBanAction(targetId.GetId().Value, banTarget, sender, e.Message.From.Id, sender);

        var targetId2 = targetId.GetId();
        var r6 = new Tuple<List<ExceptionNumbered>, int>(exceptions, nExceptions);
        if (targetId2 == null)
        {
            await NotifyUtil.NotifyOwnersAsync(r6, sender, e, "Ban/Unban All of [UNKNOWN]",
                e.Message.From.LanguageCode);
        }
        else
        {
            var link2 = "tg://user?id=" + targetId2.Value;
            await NotifyUtil.NotifyOwnersAsync(r6, sender, e,
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
            var o1 = dr["type"].ToString()?.ToLower();
            switch (o1)
            {
                case null:
                    return null;

                case "supergroup":
                    return ChatType.Supergroup;

                case "group":
                    return ChatType.Group;

                case "channel":
                    return ChatType.Channel;

                case "private":
                    return ChatType.Private;
            }
        }
        catch
        {
            ;
        }

        return null;
    }

    private static async Task<SuccessWithException> MuteUser(TelegramBotAbstract sender,
        long value, long groupChatId, DateTime? until, ChatType? chatType, RestrictAction restrictAction)
    {
        try
        {
            await Mute2Async(until, sender, groupChatId, value, chatType, restrictAction);
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
        
        if (e.Message.From != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username,
                text7,
                ParseMode.Html,
                e.Message.MessageId);
    }

    private static int AddExceptionIfNeeded(ref List<ExceptionNumbered> exceptions, Exception item2)
    {
        if (item2 == null)
            return 0;

        var (item1, i) = FindIfPresentSimilarException(exceptions, item2);
        if (item1 == false)
            exceptions.Add(new ExceptionNumbered(item2));
        else
            exceptions[i].Increment();

        return 1;
    }

    private static Tuple<bool, int> FindIfPresentSimilarException(IReadOnlyList<ExceptionNumbered> exceptions,
        Exception item2)
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

    private static bool LogBanAction(long targetId, RestrictAction bannedTrueUnbannedFalse,
        TelegramBotAbstract bot, long whoBanned, TelegramBotAbstract sender)
    {
        if (bannedTrueUnbannedFalse != RestrictAction.BAN &&
            bannedTrueUnbannedFalse != RestrictAction.UNBAN) return false;

        try
        {
            bool? b = bannedTrueUnbannedFalse switch
            {
                RestrictAction.BAN => true,
                RestrictAction.UNBAN => false,
                _ => null
            };

            const string q =
                "INSERT INTO Banned (from_bot_id, who_banned, when_banned, target, banned_true_unbanned_false) " +
                " VALUES (@fbi, @whob, @whenb, @target, @btuf)";

            var dict = new Dictionary<string, object>
            {
                { "@fbi", bot.GetId() },
                { "@whob", whoBanned },
                { "@whenb", DateTime.Now },
                { "@target", targetId },
                { "@btuf", StringUtil.ToSn(b) }
            };
            var done = Database.Execute(q, sender.DbConfig, dict);

            return done > 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<SuccessWithException> UnBanUserFromGroup(TelegramBotAbstract sender, long? target,
        long groupChatId)
    {
        return await sender.UnBanUserFromGroup(target, groupChatId);
    }

    public static async Task<SuccessWithException> BanUserFromGroup(TelegramBotAbstract sender,
        long? target,
        long groupChatId, string[] time,
        bool? revokeMessage)
    {
        return target != null
            ? await sender.BanUserFromGroup(target.Value, groupChatId, time, revokeMessage)
            : new SuccessWithException(false, new ArgumentNullException());
    }
}