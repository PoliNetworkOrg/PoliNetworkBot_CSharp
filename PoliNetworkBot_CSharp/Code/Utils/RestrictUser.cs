#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Errors;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.BanUnban;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.DatabaseUtils;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class RestrictUser
{
    internal static async Task Mute(int timeInSeconds, TelegramBotAbstract? telegramBotClient, long chatId,
        long? userId,
        ChatType chatType, RestrictAction restrictAction)
    {
        var untilDate = DateTime.Now.AddSeconds(timeInSeconds);
        await Mute2Async(untilDate, telegramBotClient, chatId, userId, chatType, restrictAction);
    }

    private static async Task Mute2Async(DateTime? untilDate, TelegramBotAbstract? telegramBotClient, long chatId,
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

        if (telegramBotClient != null)
            await telegramBotClient.RestrictChatMemberAsync(chatId, userId, permissions, untilDate, chatType);
    }

    private static async Task<BanUnbanAllResultComplete?> BanAllAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e,
        TargetUserObject target, RestrictAction banTarget, DateTime? until,
        bool? revokeMessage)
    {
        var targetId = await Info.GetTargetUserIdAsync(target, sender);
        var dt = await ToExitBan(targetId, sender, target, e);
        return dt == null ? null : await BanAllAsync3(sender, target, e, targetId, banTarget, dt, revokeMessage, until);
    }

    private static async Task<DataTable?> ToExitBan(TargetUserObject targetId, TelegramBotAbstract? sender,
        TargetUserObject target, MessageEventArgs? e)
    {
        var targetEmpty = await targetId.UserIdEmpty(sender);
        if (targetEmpty)
        {
            var text2 = new Language(new Dictionary<string, string?>
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
            if (e is { Message.From: { } })
                await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text2,
                    ParseMode.Html,
                    e.Message.MessageId, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
            return null;
        }

        const string? q1 = "SELECT id, type FROM GroupsTelegram";
        if (sender == null)
            return null;

        var dt = Database.ExecuteSelect(q1, sender.DbConfig);
        if (dt != null && dt.Rows.Count != 0)
            return dt;

        var text3 = new Language(new Dictionary<string, string?>
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
        if (e is { Message.From: { } })
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username,
                text3,
                ParseMode.Html,
                e.Message.MessageId, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
        return null;
    }

    private static async Task<BanUnbanAllResultComplete> BanAllAsync3(TelegramBotAbstract? sender,
        TargetUserObject target, MessageEventArgs? e, TargetUserObject targetId, RestrictAction banTarget,
        DataTable dt, bool? revokeMessage, DateTime? until)
    {
        await AlertActionStartedAsync(sender, target, e);

        var banUnbanAllResultComplete = await BanSingleInAll(banTarget, dt, targetId, sender, revokeMessage, until);

        LogBanAction(targetId.GetUserId(), banTarget, sender, e?.Message.From?.Id, sender);

        await SendFileNotify(targetId, banTarget, banUnbanAllResultComplete.Exceptions,
            banUnbanAllResultComplete.NExceptions, sender, EventArgsContainer.Get(e));

        return banUnbanAllResultComplete;
    }

    private static async Task<BanUnbanAllResultComplete> BanSingleInAll(RestrictAction? banTarget, DataTable? dt,
        TargetUserObject targetId,
        TelegramBotAbstract? sender, bool? revokeMessage, DateTime? until)
    {
        const int timeSleepBetweenBanUnban = 10;
        var nExceptions = 0;
        var done = new List<DataRow>();
        var failed = new List<DataRow>();

        var exceptions = new List<ExceptionNumbered>();
        switch (banTarget)
        {
            case RestrictAction.BAN:
            {
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        Thread.Sleep(timeSleepBetweenBanUnban);
                        try
                        {
                            var groupChatId = (long)dr["id"];
                            if (targetId != null)
                            {
                                var target2 = targetId.GetUserId();
                                var success = await BanUserFromGroup(sender, target2, groupChatId, null,
                                    revokeMessage);
                                if (success != null && success.IsSuccess())
                                    done.Add(dr);
                                else
                                    failed.Add(dr);

                                if (success != null && success.ContainsExceptions())
                                    nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                            }
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                break;
            }

            case RestrictAction.UNBAN:
            {
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        Thread.Sleep(timeSleepBetweenBanUnban);
                        try
                        {
                            var groupChatId = (long)dr["id"];
                            var success = await UnBanUserFromGroup(sender, targetId.GetUserId(), groupChatId);
                            if (success != null && success.IsSuccess())
                                done.Add(dr);
                            else
                                failed.Add(dr);

                            if (success != null && success.ContainsExceptions())
                                nExceptions += AddExceptionIfNeeded(ref exceptions, success.GetFirstException());
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                break;
            }

            case RestrictAction.MUTE:
            {
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        Thread.Sleep(timeSleepBetweenBanUnban);
                        try
                        {
                            var groupChatId = (long)dr["id"];
                            var chatType = GetChatType(dr);
                            var success = await MuteUser(sender, targetId.GetUserId(), groupChatId, until,
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
                            // ignored
                        }
                    }

                break;
            }
            case RestrictAction.UNMUTE:
            {
                if (dt != null)
                    foreach (DataRow dr in dt.Rows)
                    {
                        Thread.Sleep(timeSleepBetweenBanUnban);
                        try
                        {
                            var groupChatId = (long)dr["id"];
                            var chatType = GetChatType(dr);
                            var success = await MuteUser(sender, targetId.GetUserId(), groupChatId, until,
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
                            // ignored
                        }
                    }
            }
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(banTarget), banTarget, null);
        }

        var r5 = new BanUnbanAllResult(done, failed);
        return new BanUnbanAllResultComplete(r5, exceptions, nExceptions);
    }

    private static async Task SendFileNotify(TargetUserObject? targetId, RestrictAction banTarget,
        List<ExceptionNumbered> exceptions, int nExceptions, TelegramBotAbstract? telegramBotAbstract,
        EventArgsContainer? messageEventArgs)
    {
        var targetId2 = targetId?.GetUserId();
        var filename = GetFileName(banTarget, targetId2);
        var r6 = new Tuple<List<ExceptionNumbered>, int>(exceptions, nExceptions);
        var unbanAllOfUnknown = GetBanUnbanText(targetId);
        await NotifyUtil.NotifyOwnersAsync5(r6, telegramBotAbstract, messageEventArgs,
            unbanAllOfUnknown,
            messageEventArgs?.MessageEventArgs?.Message.From?.LanguageCode, filename);
    }

    private static string GetBanUnbanText(TargetUserObject? targetUserObject)
    {
        var banUnbanAllOf = "Ban/Unban All of [" + targetUserObject?.GetTargetHtmlString() + "]";
        const string banUnbanAllOfUnknown = "Ban/Unban All of [UNKNOWN]";
        var unbanAllOfUnknown = targetUserObject?.GetUserId() == null ? banUnbanAllOfUnknown : banUnbanAllOf;
        return unbanAllOfUnknown;
    }

    private static string GetFileName(RestrictAction banTarget, long? targetUserObject)
    {
        var filename = "";
        filename += banTarget switch
        {
            RestrictAction.BAN => "ban",
            RestrictAction.UNBAN => "unban",
            RestrictAction.MUTE => "mute",
            RestrictAction.UNMUTE => "unmute",
            _ => "unknown"
        };

        filename += targetUserObject == null ? "_null" : "_" + targetUserObject.Value;
        filename += ".json";
        return filename;
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
            // ignored
        }

        return null;
    }

    private static async Task<SuccessWithException> MuteUser(TelegramBotAbstract? sender,
        long? value, long groupChatId, DateTime? until, ChatType? chatType, RestrictAction restrictAction)
    {
        try
        {
            await Mute2Async(until, sender, groupChatId, value, chatType, restrictAction);
        }
        catch (Exception? ex)
        {
            return new SuccessWithException(false, ex);
        }

        return new SuccessWithException(true);
    }

    private static async Task AlertActionStartedAsync(TelegramBotAbstract? sender, TargetUserObject target,
        MessageEventArgs? e)
    {
        var text7 = new Language(new Dictionary<string, string?>
        {
            {
                "en", "Action started: '" + target.GetTargetHtmlString() + "'"
            },
            {
                "it", "Azione avviata: '" + target.GetTargetHtmlString() + "'"
            }
        });

        if (e is { Message.From: { } })
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username,
                text7,
                ParseMode.Html,
                e.Message.MessageId, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static int AddExceptionIfNeeded(ref List<ExceptionNumbered> exceptions, Exception? item2)
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
        Exception? item2)
    {
        for (var i = 0; i < exceptions.Count; i++)
        {
            var e1 = exceptions[i];
            if (AreTheySimilar(e1, item2)) return new Tuple<bool, int>(true, i);
        }

        return new Tuple<bool, int>(false, -1);
    }

    private static bool AreTheySimilar(ExceptionNumbered e1, Exception? item2)
    {
        return e1.AreTheySimilar(item2);
    }

    private static bool LogBanAction(long? targetId, RestrictAction bannedTrueUnbannedFalse,
        TelegramBotAbstract? bot, long? whoBanned, TelegramBotAbstract? sender)
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

            const string? q =
                "INSERT INTO Banned (from_bot_id, who_banned, when_banned, target, banned_true_unbanned_false) " +
                " VALUES (@fbi, @whob, @whenb, @target, @btuf)";

            if (bot != null)
            {
                var dict = new Dictionary<string, object?>
                {
                    { "@fbi", bot.GetId() },
                    { "@whob", whoBanned ?? 0 },
                    { "@whenb", DateTime.Now },
                    { "@target", targetId },
                    { "@btuf", StringUtil.ToSn(b) }
                };
                if (sender != null)
                {
                    var done = Database.Execute(q, sender.DbConfig, dict);

                    return done > 0;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static async Task<SuccessWithException?> UnBanUserFromGroup(TelegramBotAbstract? sender, long? target,
        long groupChatId)
    {
        if (sender != null) return await sender.UnBanUserFromGroup(target, groupChatId);
        return null;
    }

    public static async Task<SuccessWithException?> BanUserFromGroup(TelegramBotAbstract? sender,
        long? target,
        long groupChatId, string?[]? time,
        bool? revokeMessage)
    {
        if (sender != null)
            return target != null
                ? await sender.BanUserFromGroup(target.Value, groupChatId, time, revokeMessage)
                : new SuccessWithException(false, new ArgumentNullException());
        return null;
    }

    public static async Task TryMuteUsers(TelegramBotAbstract telegramBotClient, MessageEventArgs messageEventArgs,
        IEnumerable<User> added, TimeSpan fromMinutes)
    {
        foreach (var user in added)
            try
            {
                if (messageEventArgs.Message != null)
                    await Mute((int)fromMinutes.TotalSeconds, telegramBotClient, messageEventArgs.Message.Chat.Id,
                        user.Id,
                        messageEventArgs.Message.Chat.Type,
                        RestrictAction.MUTE);
            }
            catch
            {
                // ignored
            }
    }


    public static async Task<CommandExecutionState> BanAllAsync(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? args)
    {
        if (args is not { Length: >= 1 }) return CommandExecutionState.UNMET_CONDITIONS;
        await BanAllUnbanAllMethod1Async2Async(sender, e, args, e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, RestrictAction.BAN,
            false);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> BanDeleteAllAsync(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? args)
    {
        if (args is not { Length: >= 1 }) return CommandExecutionState.UNMET_CONDITIONS;
        await BanAllUnbanAllMethod1Async2Async(sender, e, args, e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, RestrictAction.BAN,
            true);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> DeleteMessageFromUser(MessageEventArgs? e,
        TelegramBotAbstract? sender, string[]? args)
    {
        if (e?.Message.ReplyToMessage?.Chat.Id == null || sender == null) return CommandExecutionState.UNMET_CONDITIONS;
        await sender.DeleteMessageAsync(e.Message.ReplyToMessage.Chat.Id, e.Message.ReplyToMessage.MessageId, null);
        return CommandExecutionState.SUCCESSFUL;
    }


    private static async Task<SuccessWithException> BanAllUnbanAllMethod1Async2Async(TelegramBotAbstract? sender,
        MessageEventArgs? e,
        IReadOnlyList<string?>? target, string? lang, string? username, RestrictAction ban,
        bool? revokeMessage)
    {
        var d1 = DateTimeClass.GetDateTime(target);
        try
        {
            var targetUserObject = new TargetUserObject(target, sender, e);
            await BanAllUnbanAllMethod1Async(ban, targetUserObject, e, sender, lang,
                username,
                d1?.GetValue(), revokeMessage);
            return new SuccessWithException(true, d1?.GetExceptions());
        }
        catch (Exception? ex)
        {
            var ex2 = ExceptionUtil.Concat(ex, d1);
            return new SuccessWithException(false, ex2);
        }
    }


    private static async Task BanAllUnbanAllMethod1Async(RestrictAction restrictAction,
        TargetUserObject finalTarget,
        MessageEventArgs? e, TelegramBotAbstract? sender, string? lang, string? username, DateTime? until,
        bool? revokeMessage)
    {
        var targetEmpty = await finalTarget.UserIdEmpty(sender);
        if (targetEmpty)
        {
            var lang2 = new Language(new Dictionary<string, string?>
            {
                { "en", "We can't find the target." },
                { "it", "Non riusciamo a trovare il bersaglio" }
            });
            if (sender != null)
                await sender.SendTextMessageAsync(e?.Message.From?.Id, lang2, ChatType.Private,
                    lang, ParseMode.Html, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

            return;
        }

        if (string.IsNullOrEmpty(e?.Message.ReplyToMessage?.Text))
        {
            var lang2 = new Language(new Dictionary<string, string?>
            {
                { "en", "The replied message cannot be empty!" },
                { "it", "Il messaggio a cui rispondi non può essere vuoto" }
            });
            if (e?.Message.From == null) return;
            if (sender != null)
                await sender.SendTextMessageAsync(e.Message.From.Id, lang2, ChatType.Private,
                    lang, ParseMode.Html, username: username,
                    replyMarkupObject: new ReplyMarkupObject(ReplyMarkupEnum.REMOVE));

            return;
        }

        var done =
            await BanAllAsync(sender, e, finalTarget, restrictAction, until, revokeMessage);
        var text2 = done?.BanUnbanAllResult.GetLanguage(restrictAction, finalTarget, done.NExceptions);

        NotifyUtil.NotifyOwnersBanAction(sender, EventArgsContainer.Get(e), restrictAction, done, finalTarget,
            e.Message.ReplyToMessage.Text);

        if (e.Message.From != null)
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode,
                e.Message.From.Username, text2,
                ParseMode.Html,
                e.Message.MessageId, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

        NotifyUtil.SendReportOfSuccessAndFailures(sender, e, done);
    }


    public static async Task<CommandExecutionState> BanUserAsync(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? stringInfo)
    {
        if (e?.Message.From != null)
        {
            var r =
                await Groups.CheckIfAdminAsync(e.Message.From.Id, e.Message.From.Username, e.Message.Chat.Id,
                    sender);
            if (r != null && !r.IsSuccess()) return CommandExecutionState.ERROR_DEFAULT;
        }

        if (e?.Message.ReplyToMessage == null)
        {
            var targetUserObject = new TargetUserObject(stringInfo, sender, e);
            var userIdFound = await Info.GetTargetUserIdAsync(targetUserObject, sender);
            var targetEmpty = await userIdFound.UserIdEmpty(sender);
            if (targetEmpty)
            {
                var e2 = new Exception("Can't find userid (1)");
                NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(e2), sender, EventArgsContainer.Get(e));
                return CommandExecutionState.ERROR_DEFAULT;
            }

            var targetId = userIdFound.GetUserId();
            if (targetId != null && e?.Message != null)
            {
                await BanUserFromGroup(sender, targetId.Value, e.Message.Chat.Id, null, false);
                return CommandExecutionState.SUCCESSFUL;
            }

            var e3 = new Exception("Can't find userid (2)");
            NotifyUtil.NotifyOwnersClassic(new ExceptionNumbered(e3), sender, EventArgsContainer.Get(e));
            return CommandExecutionState.ERROR_DEFAULT;
        }

        var targetInt = e.Message.ReplyToMessage.From?.Id;

        await NotifyUtil.NotifyOwnersBanAction(sender, EventArgsContainer.Get(e), targetInt,
            e.Message.ReplyToMessage.From?.Username);

        await BanUserFromGroup(sender, targetInt, e.Message.Chat.Id, stringInfo,
            false);
        return CommandExecutionState.SUCCESSFUL;
    }


    public static async Task<CommandExecutionState> UnbanAllAsync(
        MessageEventArgs? e, TelegramBotAbstract? sender, string[]? args)
    {
        if (args is not { Length: >= 1 }) return CommandExecutionState.UNMET_CONDITIONS;
        await BanAllUnbanAllMethod1Async2Async(sender, e, args, e?.Message.From?.LanguageCode,
            e?.Message.From?.Username,
            RestrictAction.UNBAN, false);
        return CommandExecutionState.SUCCESSFUL;
    }


    public static async Task<CommandExecutionState> MuteAllAsync(MessageEventArgs? e, TelegramBotAbstract? sender,
        string[]? args)
    {
        if (args is not { Length: >= 1 }) return CommandExecutionState.UNMET_CONDITIONS;
        await BanAllUnbanAllMethod1Async2Async(sender, e, args, e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, RestrictAction.MUTE,
            false);
        return CommandExecutionState.SUCCESSFUL;
    }

    public static async Task<CommandExecutionState> UnMuteAllAsync(
        MessageEventArgs? e, TelegramBotAbstract? sender, string[]? args)
    {
        if (args is not { Length: >= 1 }) return CommandExecutionState.UNMET_CONDITIONS;
        await BanAllUnbanAllMethod1Async2Async(sender, e, args, e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, RestrictAction.UNMUTE,
            false);
        return CommandExecutionState.SUCCESSFUL;
    }
}