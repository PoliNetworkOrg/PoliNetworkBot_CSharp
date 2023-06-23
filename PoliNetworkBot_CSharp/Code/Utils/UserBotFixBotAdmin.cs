#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Objects.TmpResults;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using SampleNuGet.Objects;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core.Network.Exceptions;
using TLChatFull = TeleSharp.TL.Messages.TLChatFull;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class UserBotFixBotAdmin
{
    private static Dictionary<long, bool>? _idOfChatsWeKnowAreOk;

    public static async Task<bool> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot2(
        TelegramBotAbstract? telegramBotAbstract)
    {
        const int limit = 20;
        var i = 0;
        if (telegramBotAbstract == null)
            return false;

        TLAbsInputPeer? u =
            await UserbotPeer.GetPeerUserWithAccessHash("polinetwork3bot", telegramBotAbstract.UserbotClient);
        if (u == null)
            return false;

        _idOfChatsWeKnowAreOk = new Dictionary<long, bool>();

        while (true)
        {
            TLAbsDialogs? x = null;
            FloodException? floodException1 = null;
            try
            {
                if (telegramBotAbstract.UserbotClient != null)
                    x = await telegramBotAbstract.UserbotClient.GetUserDialogsAsync(limit: limit, offsetId: i);
            }
            catch (FloodException? floodException)
            {
                floodException1 = floodException;
            }

            if (x == null && floodException1 != null)
            {
                var untilWhen = GetUntilWhenWeCanMakeRequests(floodException1);
                WaitUntil(untilWhen);

                try
                {
                    if (telegramBotAbstract.UserbotClient != null)
                        x = await telegramBotAbstract.UserbotClient.GetUserDialogsAsync(limit: limit, offsetId: i);
                }
                catch (Exception? e7)
                {
                    Logger.Logger.WriteLine(e7);
                }
            }

            switch (x)
            {
                case null:
                    return i > 0;

                case TLDialogs x2:
                {
                    if (x2.Chats != null)
                        foreach (var x4 in x2.Chats)
                        {
                            var r1 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot3(x4, u,
                                telegramBotAbstract);
                            NotifyUtil.NotifyIfFalseAsync(r1, 1.ToString(), telegramBotAbstract);
                        }

                    break;
                }
                case TLDialogsSlice x3:
                {
                    if (x3.Chats != null)
                        foreach (var x4 in x3.Chats)
                        {
                            var r1 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot3(x4, u,
                                telegramBotAbstract);
                            NotifyUtil.NotifyIfFalseAsync(r1, 2.ToString(), telegramBotAbstract);
                        }

                    break;
                }
            }

            i += limit;
        }
    }

    private static async Task<Tuple<bool?, string, long>?> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot3(
        TLAbsChat x4,
        TLAbsInputPeer? u, TelegramBotAbstract? telegramBotAbstract)
    {
        switch (x4)
        {
            case null:
                return null;

            case TLChat x5 when GlobalVariables.ExcludedChatsForBot != null &&
                                GlobalVariables.ExcludedChatsForBot.Contains(x5.Id):
                return new Tuple<bool?, string, long>(null, x5.Title, x5.Id);

            case TLChat x5:
            {
                var r5 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot4(x5, u, telegramBotAbstract);
                if (r5 == null) return new Tuple<bool?, string, long>(null, x5.Title, x5.Id);

                if (r5.DateTime == null) return new Tuple<bool?, string, long>(r5.B, x5.Title, x5.Id);

                WaitUntil(r5.DateTime);
                var r6 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot4(x5, u, telegramBotAbstract);
                if (r6 == null) return new Tuple<bool?, string, long>(null, x5.Title, x5.Id);

                if (r6.DateTime == null)
                    return new Tuple<bool?, string, long>(r6.B, x5.Title, x5.Id);
                break;
            }
            case TLChannel x6 when GlobalVariables.ExcludedChatsForBot != null &&
                                   GlobalVariables.ExcludedChatsForBot.Contains(x6.Id):
                return new Tuple<bool?, string, long>(null, x6.Title, x6.Id);

            case TLChannel x6:
            {
                var r2 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot5(x6, u, telegramBotAbstract);

                if (r2 == null) return new Tuple<bool?, string, long>(null, x6.Title, x6.Id);

                if (r2.Item2 == null) return new Tuple<bool?, string, long>(r2.Item1, x6.Title, x6.Id);

                WaitUntil(r2.Item2);
                var r3 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot5(x6, u, telegramBotAbstract);
                if (r3 == null) return new Tuple<bool?, string, long>(null, x6.Title, x6.Id);

                if (r3.Item2 == null)
                    return new Tuple<bool?, string, long>(r3.Item1, x6.Title, x6.Id);
                break;
            }
            case TLChatForbidden:
                break;
        }

        return null;
    }

    private static void WaitUntil(DateTime? item2)
    {
        if (item2 == null)
            return;

        while (DateTime.Now < item2.Value) Thread.Sleep(2000);
    }

    private static async Task<Tuple<bool?, DateTime?>?> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot5(
        TLChannel? x5,
        TLAbsInputPeer? u, TelegramBotAbstract? telegramBotAbstract)
    {
        if (x5 == null)
            return null;

        if (x5.Broadcast)
            return null;

        if (x5.AccessHash == null)
            return null;

        var x7 = new TLInputChannel { AccessHash = x5.AccessHash.Value, ChannelId = x5.Id };
        var isBotPresentObject = await CheckIfOurBotIsPresent2Async(x7, telegramBotAbstract);
        if (isBotPresentObject.DateTime != null)
            return new Tuple<bool?, DateTime?>(null, isBotPresentObject.DateTime);

        if (isBotPresentObject.B != null && isBotPresentObject.B.Value)
            return new Tuple<bool?, DateTime?>(true, null);

        var r3 = await InsertOurBotAsyncChannel(x5, u, telegramBotAbstract);
        return new Tuple<bool?, DateTime?>(r3, null);
    }

    private static async Task<IsBotPresentObject> CheckIfOurBotIsPresent2Async(TLInputChannel x5,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (_idOfChatsWeKnowAreOk != null && _idOfChatsWeKnowAreOk.TryGetValue(x5.ChannelId, out var value))
            return new IsBotPresentObject(value, null);

        TLAbsInputChannel channel = new TLInputChannel { ChannelId = x5.ChannelId, AccessHash = x5.AccessHash };
        TLChatFull? x = null;
        try
        {
            if (telegramBotAbstract?.UserbotClient != null)
                x = await telegramBotAbstract.UserbotClient.getFullChat(channel);
        }
        catch (Exception e)
        {
            if (e is FloodException eflood)
            {
                var untilWhen = GetUntilWhenWeCanMakeRequests(eflood);
                return new IsBotPresentObject(null, untilWhen);
            }
        }

        var isOurBotPresent = CheckIfOurBotIsPresent(x);
        if (!isOurBotPresent) return new IsBotPresentObject(isOurBotPresent, null);
        if (_idOfChatsWeKnowAreOk != null)
            _idOfChatsWeKnowAreOk[x5.ChannelId] = true;

        return new IsBotPresentObject(isOurBotPresent, null);
    }

    private static DateTime? GetUntilWhenWeCanMakeRequests(FloodException? eFlood)
    {
        Thread.Sleep(1000);

        if (eFlood != null) return DateTime.Now + eFlood.TimeToWait;
        return null;
    }

    private static async Task<bool?> InsertOurBotAsyncChannel(TLChannel? x5,
        TLAbsInputPeer? u, TelegramBotAbstract? telegramBotAbstract)
    {
        if (x5?.AccessHash == null)
            return false;

        const long userIdOfOurBot = 768169879;
        var channel = new TLInputChannel { AccessHash = x5.AccessHash.Value, ChannelId = x5.Id };

        var r4 = await F1Async(telegramBotAbstract, userIdOfOurBot, u, x5.Title, x5.Id, channel);
        if (r4.ReturnObject != null)
            return r4.ReturnObject;

        if (r4.R2 is { Item1: null })
        {
            var m = "\n";
            m += "We can't make our bot admin in this group:\n";
            m += "[Title] " + x5.Title + "\n";
            m += "[ID]    " + x5.Id;
            m += "\n --- end --- ";
            m += "\n";
            var e2 = new Exception(m, r4.R2.Item2);
            await NotifyUtil.NotifyOwnerWithLog2(e2, telegramBotAbstract, null);

            await DeleteMessageAddedAsync(r4.IdMessageAdded, x5, telegramBotAbstract);

            return false;
        }

        Thread.Sleep(2000);

        try
        {
            await DeleteMessageAddedAsync(r4.IdMessageAdded, x5, telegramBotAbstract);
        }
        catch (Exception? e5)
        {
            await NotifyUtil.NotifyOwnerWithLog2(e5, telegramBotAbstract, null);
        }

        if (_idOfChatsWeKnowAreOk != null) _idOfChatsWeKnowAreOk[x5.Id] = true;

        return r4 is { R2.Item1: not null, R: not null };
    }

    private static async Task DeleteMessageAddedAsync(long? idMessageAdded, TLChannel? x5,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (x5 != null)
            await DeleteMessageAddedAsync2(idMessageAdded, x5.Id, x5.AccessHash, telegramBotAbstract);
    }

    private static async Task DeleteMessageAddedAsync2(long? idMessageAdded, int id, long? accessHash,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (idMessageAdded != null)
        {
            try
            {
                if (telegramBotAbstract != null)
                    await telegramBotAbstract.DeleteMessageAsync(id, idMessageAdded.Value, accessHash);
            }
            catch (Exception? e3)
            {
                Logger.Logger.WriteLine(e3);

                try
                {
                    if (telegramBotAbstract != null)
                        await telegramBotAbstract.DeleteMessageAsync(id, idMessageAdded.Value, accessHash);
                }
                catch
                {
                    try
                    {
                        var messageToDelete = new TLVector<int>
                        {
                            (int)idMessageAdded.Value
                        };
                        TLAbsInputChannel x7 = accessHash == null
                            ? new TLInputChannel { ChannelId = id }
                            : new TLInputChannel { AccessHash = accessHash.Value, ChannelId = id };
                        if (telegramBotAbstract?.UserbotClient != null)
                            await telegramBotAbstract.UserbotClient.ChannelsDeleteMessageAsync(x7, messageToDelete);
                    }
                    catch
                    {
                        try
                        {
                            /*
                            TLVector<int> messageToDelete = new TLVector<int>();
                            messageToDelete.Add(idMessageAdded.Value);
                            TLAbsInputChannel x7 = new TLInputChannel() { AccessHash = x5.AccessHash.Value, ChannelId = x5.Id };
                            await this._userbotClient.DeleteMessageFromChat(x7, messageToDelete);
                            */
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }

            Thread.Sleep(2000);
        }
    }

    private static long? GetIdMessageAdded(TLAbsUpdates? r)
    {
        return r is TLUpdates r2 ? GetIdMessageAdded2(r2) : null;
    }

    private static long? GetIdMessageAdded2(TLUpdates? r2)
    {
        if (r2?.Updates == null || r2.Updates.Count == 0)
            return null;

        foreach (var r3 in r2.Updates)
            switch (r3)
            {
                case TLUpdateMessageID r4:
                    return r4.Id;

                case TLUpdateReadChannelInbox r5:
                    return r5.MaxId;

                case TLUpdateNewChannelMessage r6:
                {
                    var r7 = r6.Message;
                    if (r7 is TLMessageService r8)
                        return r8.Id;
                    break;
                }
            }

        return null;
    }

    private static async Task<Tuple<TLAbsUpdates?, Exception?>?> PromoteToAdminAsync(TLAbsInputUser u2,
        TLInputChannel channel, TelegramBotAbstract? telegramBotAbstract)
    {
        TLAbsUpdates? r2 = null;
        try
        {
            TLAbsChannelParticipantRole role = new TLChannelRoleEditor();
            if (telegramBotAbstract is { UserbotClient: not null })
                r2 = await telegramBotAbstract.UserbotClient.ChannelsEditAdmin(channel, u2, role);
        }
        catch (Exception e2)
        {
            try
            {
                TLAbsChannelParticipantRole role2 = new TLChannelRoleModerator();
                if (telegramBotAbstract?.UserbotClient != null)
                    await telegramBotAbstract.UserbotClient.ChannelsEditAdmin(channel, u2, role2);
            }
            catch (Exception e3)
            {
                try
                {
                    var r3 = telegramBotAbstract is { UserbotClient: not null } &&
                             await telegramBotAbstract.UserbotClient.Messages_EditChatAdmin(channel.ChannelId, u2,
                                 true);
                    if (r3 == false) return new Tuple<TLAbsUpdates?, Exception?>(null, e3);
                }
                catch (Exception e4)
                {
                    return new Tuple<TLAbsUpdates?, Exception?>(null, e4);
                }

                return new Tuple<TLAbsUpdates?, Exception?>(null, e3);
            }

            return new Tuple<TLAbsUpdates?, Exception?>(null, e2);
        }

        return new Tuple<TLAbsUpdates?, Exception?>(r2, null);
    }

    private static async Task<ResultF1> F1Async(TelegramBotAbstract? telegramBotAbstract, long userIdOfOurBot,
        TLAbsInputPeer? u, string x5Title, int x5Id, TLInputChannel channel)
    {
        var users = new TLVector<TLAbsInputUser>();
        if (u == null)
            return new ResultF1(false, null, null, null);

        TLInputPeerUser? u5 = null;
        if (u is TLInputPeerUser u4) u5 = u4;

        if (u5 == null)
            return new ResultF1(false, null, null, null);

        var accessHashUser = u5.AccessHash;
        TLAbsInputUser u2 = new TLInputUser { UserId = (int)userIdOfOurBot, AccessHash = accessHashUser };
        users.Add(u2);
        TLAbsUpdates? r = null;
        try
        {
            if (telegramBotAbstract?.UserbotClient != null)
                r = await telegramBotAbstract.UserbotClient.ChannelsInviteToChannel(channel, users);
        }
        catch (Exception e)
        {
            var m = "\n";
            m += "We can't add our bot in this group:\n";
            m += "[Title] " + x5Title + "\n";
            m += "[ID]    " + x5Id;
            m += "\n --- end --- ";
            m += "\n";
            var e2 = new Exception(m, e);
            await NotifyUtil.NotifyOwnerWithLog2(e2, telegramBotAbstract, null);

            return new ResultF1(false, null, r, null);
        }

        Thread.Sleep(2000);

        var idMessageAdded = GetIdMessageAdded(r);

        Tuple<TLAbsUpdates?, Exception?>? r2 = null;
        try
        {
            r2 = await PromoteToAdminAsync(u2, channel, telegramBotAbstract);
        }
        catch (Exception? e2)
        {
            Logger.Logger.WriteLine(e2);
        }

        return new ResultF1(null, idMessageAdded, r, r2);
    }

    private static async Task<bool?> InsertOurBotAsyncChat(TLChat? x5,
        TLAbsInputPeer? u, long? accessHash, TelegramBotAbstract? telegramBotAbstract)
    {
        const long userIdOfOurBot = 768169879;

        if (x5 == null)
            return null;

        var channel = accessHash != null
            ? new TLInputChannel { AccessHash = accessHash.Value, ChannelId = x5.Id }
            : new TLInputChannel { ChannelId = x5.Id };

        var r4 = await F1Async(telegramBotAbstract, userIdOfOurBot, u, x5.Title, x5.Id, channel);
        if (r4.ReturnObject != null)
            return r4.ReturnObject.Value;

        if (r4.R2 is { Item1: null })
        {
            var m = "\n";
            m += "We can't make our bot admin in this group:\n";
            m += "[Title] " + x5.Title + "\n";
            m += "[ID]    " + x5.Id;
            m += "\n --- end --- ";
            m += "\n";
            var e2 = new Exception(m, r4.R2.Item2);
            await NotifyUtil.NotifyOwnerWithLog2(e2, telegramBotAbstract, null);

            await DeleteMessageAddedAsync(r4.IdMessageAdded, x5, accessHash, telegramBotAbstract);

            return false;
        }

        Thread.Sleep(2000);

        await DeleteMessageAddedAsync(r4.IdMessageAdded, x5, accessHash, telegramBotAbstract);

        if (_idOfChatsWeKnowAreOk != null) _idOfChatsWeKnowAreOk[x5.Id] = true;

        return r4 is { R2.Item1: not null, R: not null };
    }

    private static async Task DeleteMessageAddedAsync(long? idMessageAdded, TLChat? x5, long? accessHash,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (x5 != null)
            await DeleteMessageAddedAsync2(idMessageAdded, x5.Id, accessHash, telegramBotAbstract);
    }

    private static bool CheckIfOurBotIsPresent(TLChatFull? x)
    {
        if (x?.Users == null)
            return false;

        if (x.Users.Count == 0)
            return false;

        foreach (var u in x.Users)
            if (u is TLUser u2)
                if (string.IsNullOrEmpty(u2.Username) == false)
                    if (u2.Username.ToLower() == "polinetwork3bot")
                        return true;

        return false;
    }

    private static async Task<IsBotPresentObject?> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot4(
        TLChat? x5, TLAbsInputPeer? u, TelegramBotAbstract? telegramBotAbstract)
    {
        if (x5 == null)
            return null;

        if (x5.MigratedTo == null)
        {
            var r4 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot6Async(x5, u, telegramBotAbstract);
            return r4;
        }

        var x6 = x5.MigratedTo;
        TLInputChannel x8;
        if (x6 is TLInputChannel x7)
            x8 = x7;
        else
            return null;

        if (x5.MigratedTo == null) return null;

        TLAbsInputChannel channel = x8;
        var channel2 = GetChannel(channel);
        if (channel2 == null)
            return new IsBotPresentObject(false, null);

        var isOurBotPresent2 = await CheckIfOurBotIsPresent2Async(channel2, telegramBotAbstract);

        if (isOurBotPresent2.DateTime != null) return new IsBotPresentObject(null, isOurBotPresent2.DateTime);

        if (isOurBotPresent2.B != null && isOurBotPresent2.B.Value)
            return new IsBotPresentObject(true, null);

        var r3 = await InsertOurBotAsyncChat(x5, u, x8.AccessHash, telegramBotAbstract);
        return new IsBotPresentObject(r3, null);
    }

    private static TLInputChannel? GetChannel(TLAbsInputChannel channel)
    {
        if (channel is TLInputChannel c2) return c2;

        return null;
    }

    private static async Task<IsBotPresentObject?> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot6Async(
        TLChat? x5, TLAbsInputPeer? u, TelegramBotAbstract? telegramBotAbstract)
    {
        var isOurBotPresent2 = await CheckIfOurBotIsPresent3Async(x5, telegramBotAbstract);

        if (isOurBotPresent2.DateTime != null) return new IsBotPresentObject(null, isOurBotPresent2.DateTime);

        if (isOurBotPresent2.B != null && isOurBotPresent2.B.Value)
            return new IsBotPresentObject(true, null);

        var r4 = await InsertOurBotAsyncChat(x5, u, null, telegramBotAbstract);

        return new IsBotPresentObject(r4, null);
    }

    private static async Task<IsBotPresentObject> CheckIfOurBotIsPresent3Async(TLChat? x5,
        TelegramBotAbstract? telegramBotAbstract)
    {
        if (x5 != null && _idOfChatsWeKnowAreOk != null && _idOfChatsWeKnowAreOk.TryGetValue(x5.Id, out var value))
            return new IsBotPresentObject(value, null);

        TLChatFull? x = null;
        try
        {
            if (telegramBotAbstract?.UserbotClient != null)
                if (x5 != null)
                    x = await telegramBotAbstract.UserbotClient.Messages_getFullChat(x5.Id);
        }
        catch (Exception e)
        {
            if (e is FloodException floodException)
            {
                var untilWhen = GetUntilWhenWeCanMakeRequests(floodException);
                return new IsBotPresentObject(null, untilWhen);
            }
        }

        if (x == null) return new IsBotPresentObject(false, null);

        var r = CheckIfOurBotIsPresent(x);
        if (!r || _idOfChatsWeKnowAreOk == null)
            return new IsBotPresentObject(r, null);
        if (x5 != null)
            _idOfChatsWeKnowAreOk[x5.Id] = true;

        return new IsBotPresentObject(r, null);
    }
}