using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Data;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TmpResults;
using Telegram.Bot.Types.Enums;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core.Network.Exceptions;
using TLChatFull = TeleSharp.TL.Messages.TLChatFull;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class UserBotFixBotAdmin
    {
        public static Dictionary<long, bool> id_of_chats_we_know_are_ok;

        public static async Task<bool> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot2(
            TelegramBotAbstract telegramBotAbstract)
        {
            const int LIMIT = 20;
            var i = 0;
            TLAbsInputPeer u =
                await UserbotPeer.GetPeerUserWithAccessHash("polinetwork3bot", telegramBotAbstract._userbotClient);
            if (u == null)
                return false;

            id_of_chats_we_know_are_ok = new Dictionary<long, bool>();

            while (true)
            {
                TLAbsDialogs x = null;
                FloodException floodException1 = null;
                try
                {
                    x = await telegramBotAbstract._userbotClient.GetUserDialogsAsync(limit: LIMIT, offsetId: i);
                }
                catch (FloodException floodException)
                {
                    floodException1 = floodException;
                }

                if (x == null && floodException1 != null)
                {
                    var untilWhen = GetUntilWhenWeCanMakeRequests(floodException1);
                    WaitUntil(untilWhen);

                    try
                    {
                        x = await telegramBotAbstract._userbotClient.GetUserDialogsAsync(limit: LIMIT, offsetId: i);
                    }
                    catch (Exception e7)
                    {
                        Console.WriteLine(e7);
                    }
                }

                if (x == null)
                    return i > 0;

                if (x is TLDialogs x2)
                {
                    if (x2.Chats != null)
                        foreach (var x4 in x2.Chats)
                        {
                            var r1 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot3(x4, u,
                                telegramBotAbstract);
                            await NotifyUtil.NotifyIfFalseAsync(r1, 1.ToString(), telegramBotAbstract);
                        }
                }
                else if (x is TLDialogsSlice x3)
                {
                    if (x3.Chats != null)
                        foreach (var x4 in x3.Chats)
                        {
                            var r1 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot3(x4, u,
                                telegramBotAbstract);
                            await NotifyUtil.NotifyIfFalseAsync(r1, 2.ToString(), telegramBotAbstract);
                        }
                }
                else
                {
                    ;
                }

                i += LIMIT;
            }

            throw new NotImplementedException();
        }

        private static async Task<Tuple<bool?, string, long>> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot3(
            TLAbsChat x4,
            TLAbsInputPeer u, TelegramBotAbstract telegramBotAbstract)
        {
            if (x4 == null)
                return null;

            if (x4 is TLChat x5)
            {
                if (GlobalVariables.ExcludedChatsForBot.Contains(x5.Id))
                    return new Tuple<bool?, string, long>(null, x5.Title, x5.Id);

                var r5 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot4(x5, u, telegramBotAbstract);
                if (r5 == null) return new Tuple<bool?, string, long>(null, x5.Title, x5.Id);

                if (r5.Item2 == null) return new Tuple<bool?, string, long>(r5.Item1, x5.Title, x5.Id);

                WaitUntil(r5.Item2);
                var r6 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot4(x5, u, telegramBotAbstract);
                if (r6 == null)
                {
                    ;

                    return new Tuple<bool?, string, long>(null, x5.Title, x5.Id);
                }

                if (r6.Item2 == null)
                    return new Tuple<bool?, string, long>(r6.Item1, x5.Title, x5.Id);
                ;
            }
            else if (x4 is TLChannel x6)
            {
                if (GlobalVariables.ExcludedChatsForBot.Contains(x6.Id))
                    return new Tuple<bool?, string, long>(null, x6.Title, x6.Id);

                var r2 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot5(x6, u, telegramBotAbstract);

                if (r2 == null)
                {
                    ;

                    return new Tuple<bool?, string, long>(null, x6.Title, x6.Id);
                }

                if (r2.Item2 == null) return new Tuple<bool?, string, long>(r2.Item1, x6.Title, x6.Id);

                WaitUntil(r2.Item2);
                var r3 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot5(x6, u, telegramBotAbstract);
                if (r3 == null)
                {
                    ;

                    return new Tuple<bool?, string, long>(null, x6.Title, x6.Id);
                }

                if (r3.Item2 == null)
                    return new Tuple<bool?, string, long>(r3.Item1, x6.Title, x6.Id);
                ;
            }
            else if (x4 is TLChatForbidden chatForbidden)
            {
                ;
            }
            else
            {
                ;
            }

            return null;
        }

        private static void WaitUntil(DateTime? item2)
        {
            if (item2 == null)
                return;

            while (DateTime.Now < item2.Value) Thread.Sleep(2000);
        }

        private static async Task<Tuple<bool?, DateTime?>> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot5(
            TLChannel x5,
            TLAbsInputPeer u, TelegramBotAbstract telegramBotAbstract)
        {
            if (x5 == null)
                return null;

            ;

            if (x5.Broadcast)
                return null;

            ;

            if (x5.AccessHash == null)
                return null;

            var x7 = new TLInputChannel {AccessHash = x5.AccessHash.Value, ChannelId = x5.Id};
            var isOurBotPresent = await CheckIfOurBotIsPresent2Async(x7, telegramBotAbstract);
            if (isOurBotPresent.Item2 != null) return new Tuple<bool?, DateTime?>(null, isOurBotPresent.Item2);

            if (isOurBotPresent.Item1 != null && isOurBotPresent.Item1.Value)
                return new Tuple<bool?, DateTime?>(true, null);

            var r3 = await InsertOurBotAsyncChannel(x5, u, telegramBotAbstract);
            return new Tuple<bool?, DateTime?>(r3, null);
        }

        private static async Task<Tuple<bool?, DateTime?>> CheckIfOurBotIsPresent2Async(TLInputChannel x5,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (id_of_chats_we_know_are_ok.ContainsKey(x5.ChannelId))
                return new Tuple<bool?, DateTime?>(id_of_chats_we_know_are_ok[x5.ChannelId], null);

            TLAbsInputChannel channel = new TLInputChannel {ChannelId = x5.ChannelId, AccessHash = x5.AccessHash};
            TLChatFull x = null;
            try
            {
                x = await telegramBotAbstract._userbotClient.getFullChat(channel);
            }
            catch (Exception e)
            {
                ;

                if (e is FloodException eflood)
                {
                    ;

                    var untilWhen = GetUntilWhenWeCanMakeRequests(eflood);
                    return new Tuple<bool?, DateTime?>(null, untilWhen);
                }
            }

            var isOurBotPresent = CheckIfOurBotIsPresent(x);
            if (isOurBotPresent) id_of_chats_we_know_are_ok[x5.ChannelId] = true;

            return new Tuple<bool?, DateTime?>(isOurBotPresent, null);
        }

        private static DateTime GetUntilWhenWeCanMakeRequests(FloodException eflood)
        {
            ;

            Thread.Sleep(1000);

            return DateTime.Now + eflood.TimeToWait;
        }

        private static async Task<bool?> InsertOurBotAsyncChannel(TLChannel x5,
            TLAbsInputPeer u, TelegramBotAbstract telegramBotAbstract)
        {
            ;

            if (x5.AccessHash == null)
                return false;

            const long userIdOfOurBot = 768169879;
            var channel = new TLInputChannel {AccessHash = x5.AccessHash.Value, ChannelId = x5.Id};

            var r4 = await F1Async(telegramBotAbstract, userIdOfOurBot, u, x5.Title, x5.Id, channel);
            if (r4.returnobject != null)
                return r4.returnobject;

            if (r4.r2.Item1 == null)
            {
                var m = "\n";
                m += "We can't make our bot admin in this group:\n";
                m += "[Title] " + x5.Title + "\n";
                m += "[ID]    " + x5.Id;
                m += "\n --- end --- ";
                m += "\n";
                var e2 = new Exception(m, r4.r2.Item2);
                await NotifyUtil.NotifyOwners(e2, telegramBotAbstract);

                await DeleteMessageAddedAsync(r4.idMessageAdded, x5, telegramBotAbstract);

                return false;
            }

            Thread.Sleep(2000);

            ;

            try
            {
                await DeleteMessageAddedAsync(r4.idMessageAdded, x5, telegramBotAbstract);
            }
            catch (Exception e5)
            {
                ;
                await NotifyUtil.NotifyOwners(e5, telegramBotAbstract);
            }

            id_of_chats_we_know_are_ok[x5.Id] = true;

            return r4.r != null && r4.r2.Item1 != null;
        }

        private static async Task DeleteMessageAddedAsync(int? idMessageAdded, TLChannel x5,
            TelegramBotAbstract telegramBotAbstract)
        {
            await DeleteMessageAddedAsync2(idMessageAdded, x5.Id, x5.AccessHash, telegramBotAbstract);
        }

        private static async Task DeleteMessageAddedAsync2(int? idMessageAdded, int id, long? accessHash,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (idMessageAdded != null)
            {
                try
                {
                    await telegramBotAbstract.DeleteMessageAsync(id, idMessageAdded.Value, ChatType.Supergroup,
                        accessHash);
                }
                catch (Exception e3)
                {
                    Console.WriteLine(e3);

                    try
                    {
                        await telegramBotAbstract.DeleteMessageAsync(id, idMessageAdded.Value, ChatType.Group,
                            accessHash);
                    }
                    catch
                    {
                        ;

                        try
                        {
                            var messageToDelete = new TLVector<int>
                            {
                                idMessageAdded.Value
                            };
                            TLAbsInputChannel x7 = new TLInputChannel {AccessHash = accessHash.Value, ChannelId = id};
                            await telegramBotAbstract._userbotClient.ChannelsDeleteMessageAsync(x7, messageToDelete);
                        }
                        catch
                        {
                            ;

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
                                ;
                            }
                        }
                    }
                }

                ;

                Thread.Sleep(2000);
            }
        }

        private static int? GetIdMessageAdded(TLAbsUpdates r)
        {
            if (r == null)
                return null;

            if (r is TLUpdates r2)
                return GetIdMessageAdded2(r2);
            ;

            return null;
        }

        private static int? GetIdMessageAdded2(TLUpdates r2)
        {
            if (r2 == null)
                return null;

            if (r2.Updates == null || r2.Updates.Count == 0)
                return null;

            foreach (var r3 in r2.Updates)
                if (r3 is TLUpdateMessageID r4)
                {
                    return r4.Id;
                }
                else if (r3 is TLUpdateReadChannelInbox r5)
                {
                    return r5.MaxId;
                }
                else if (r3 is TLUpdateNewChannelMessage r6)
                {
                    var r7 = r6.Message;
                    if (r7 is TLMessageService r8)
                        return r8.Id;
                    ;
                }
                else
                {
                    ;
                }

            return null;
        }

        private static async Task<Tuple<TLAbsUpdates, Exception>> PromoteToAdminAsync(TLAbsInputUser u2,
            TLInputChannel channel, TelegramBotAbstract telegramBotAbstract)
        {
            TLAbsUpdates r2 = null;
            try
            {
                TLAbsChannelParticipantRole role = new TLChannelRoleEditor();
                r2 = await telegramBotAbstract._userbotClient.ChannelsEditAdmin(channel, u2, role);
            }
            catch (Exception e2)
            {
                ;

                try
                {
                    TLAbsChannelParticipantRole role2 = new TLChannelRoleModerator();
                    r2 = await telegramBotAbstract._userbotClient.ChannelsEditAdmin(channel, u2, role2);
                }
                catch (Exception e3)
                {
                    try
                    {
                        var r3 = await telegramBotAbstract._userbotClient.Messages_EditChatAdmin(channel.ChannelId, u2,
                            true);
                        if (r3 == false) return new Tuple<TLAbsUpdates, Exception>(null, e3);
                    }
                    catch (Exception e4)
                    {
                        ;

                        return new Tuple<TLAbsUpdates, Exception>(null, e4);
                    }

                    return new Tuple<TLAbsUpdates, Exception>(null, e3);
                }

                return new Tuple<TLAbsUpdates, Exception>(null, e2);
            }

            ;

            return new Tuple<TLAbsUpdates, Exception>(r2, null);
        }

        private static async Task<ResultF1> F1Async(TelegramBotAbstract telegramBotAbstract, long userIdOfOurBot,
            TLAbsInputPeer u, string x5Title, int x5Id, TLInputChannel channel)
        {
            var users = new TLVector<TLAbsInputUser>();
            if (u == null)
                return new ResultF1(false, null, null, null);

            long accessHashUser = 0;

            ;

            TLInputPeerUser u5 = null;
            if (u is TLInputPeerUser u4) u5 = u4;

            if (u5 == null)
                return new ResultF1(false, null, null, null);

            accessHashUser = u5.AccessHash;

            TLAbsInputUser u2 = new TLInputUser {UserId = (int) userIdOfOurBot, AccessHash = accessHashUser};
            users.Add(u2);
            TLAbsUpdates r = null;
            try
            {
                r = await telegramBotAbstract._userbotClient.ChannelsInviteToChannel(channel, users);
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
                await NotifyUtil.NotifyOwners(e2, telegramBotAbstract);

                return new ResultF1(false, null, r, null);
            }

            Thread.Sleep(2000);

            var idMessageAdded = GetIdMessageAdded(r);

            Tuple<TLAbsUpdates, Exception> r2 = null;
            try
            {
                r2 = await PromoteToAdminAsync(u2, channel, telegramBotAbstract);
            }
            catch (Exception e2)
            {
                Console.WriteLine(e2);
            }

            ;

            return new ResultF1(null, idMessageAdded, r, r2);
        }

        private static async Task<bool?> InsertOurBotAsyncChat(TLChat x5,
            TLAbsInputPeer u, long? accessHash, TelegramBotAbstract telegramBotAbstract)
        {
            ;

            ;

            const long userIdOfOurBot = 768169879;

            TLInputChannel channel = null;
            if (accessHash != null)
                channel = new TLInputChannel {AccessHash = accessHash.Value, ChannelId = x5.Id};
            else
                channel = new TLInputChannel {ChannelId = x5.Id};

            var r4 = await F1Async(telegramBotAbstract, userIdOfOurBot, u, x5.Title, x5.Id, channel);
            if (r4.returnobject != null)
                return r4.returnobject.Value;

            if (r4.r2.Item1 == null)
            {
                var m = "\n";
                m += "We can't make our bot admin in this group:\n";
                m += "[Title] " + x5.Title + "\n";
                m += "[ID]    " + x5.Id;
                m += "\n --- end --- ";
                m += "\n";
                var e2 = new Exception(m, r4.r2.Item2);
                await NotifyUtil.NotifyOwners(e2, telegramBotAbstract);

                await DeleteMessageAddedAsync(r4.idMessageAdded, x5, accessHash, telegramBotAbstract);

                return false;
            }

            Thread.Sleep(2000);

            ;

            await DeleteMessageAddedAsync(r4.idMessageAdded, x5, accessHash, telegramBotAbstract);

            id_of_chats_we_know_are_ok[x5.Id] = true;

            return r4.r != null && r4.r2.Item1 != null;

            return null;
        }

        private static async Task DeleteMessageAddedAsync(int? idMessageAdded, TLChat x5, long? accessHash,
            TelegramBotAbstract telegramBotAbstract)
        {
            await DeleteMessageAddedAsync2(idMessageAdded, x5.Id, accessHash, telegramBotAbstract);
        }

        private static bool CheckIfOurBotIsPresent(TLChatFull x)
        {
            if (x == null)
                return false;

            if (x.Users == null)
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

        private static async Task<Tuple<bool?, DateTime?>> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot4(
            TLChat x5, TLAbsInputPeer u, TelegramBotAbstract telegramBotAbstract)
        {
            if (x5 == null)
                return null;

            ;

            ;

            if (x5.MigratedTo == null)
            {
                var r4 = await FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot6Async(x5, u, telegramBotAbstract);
                return r4;
            }

            var x6 = x5.MigratedTo;
            TLInputChannel x8 = null;
            if (x6 is TLInputChannel x7)
            {
                x8 = x7;
            }
            else
            {
                ;
                return null;
            }

            if (x5.MigratedTo == null) return null;

            TLAbsInputChannel channel = x8;
            TLChatFull x = null;

            var channel2 = GetChannel(channel);
            if (channel2 == null)
                return new Tuple<bool?, DateTime?>(false, null);

            var isOurBotPresent2 = await CheckIfOurBotIsPresent2Async(channel2, telegramBotAbstract);

            if (isOurBotPresent2 == null)
                return new Tuple<bool?, DateTime?>(false, null);

            if (isOurBotPresent2.Item2 != null) return new Tuple<bool?, DateTime?>(null, isOurBotPresent2.Item2);

            if (isOurBotPresent2.Item1 != null && isOurBotPresent2.Item1.Value)
                return new Tuple<bool?, DateTime?>(true, null);

            var r3 = await InsertOurBotAsyncChat(x5, u, x8.AccessHash, telegramBotAbstract);
            return new Tuple<bool?, DateTime?>(r3, null);
        }

        private static TLInputChannel GetChannel(TLAbsInputChannel channel)
        {
            if (channel == null)
                return null;

            if (channel is TLInputChannel c2) return c2;

            return null;
        }

        private static async Task<Tuple<bool?, DateTime?>> FixTheFactThatSomeGroupsDoesNotHaveOurModerationBot6Async(
            TLChat x5, TLAbsInputPeer u, TelegramBotAbstract telegramBotAbstract)
        {
            ;

            var isOurBotPresent2 = await CheckIfOurBotIsPresent3Async(x5, telegramBotAbstract);
            if (isOurBotPresent2 == null)
                return new Tuple<bool?, DateTime?>(false, null);
            ;
            if (isOurBotPresent2.Item2 != null) return new Tuple<bool?, DateTime?>(null, isOurBotPresent2.Item2);

            if (isOurBotPresent2.Item1 != null && isOurBotPresent2.Item1.Value)
                return new Tuple<bool?, DateTime?>(true, null);

            var r4 = await InsertOurBotAsyncChat(x5, u, null, telegramBotAbstract);

            return new Tuple<bool?, DateTime?>(r4, null);
        }

        private static async Task<Tuple<bool?, DateTime?>> CheckIfOurBotIsPresent3Async(TLChat x5,
            TelegramBotAbstract telegramBotAbstract)
        {
            if (id_of_chats_we_know_are_ok.ContainsKey(x5.Id))
                return new Tuple<bool?, DateTime?>(id_of_chats_we_know_are_ok[x5.Id], null);

            TLChatFull x = null;
            try
            {
                x = await telegramBotAbstract._userbotClient.Messages_getFullChat(x5.Id);
            }
            catch (Exception e)
            {
                if (e is FloodException floodException)
                {
                    var untilWhen = GetUntilWhenWeCanMakeRequests(floodException);
                    return new Tuple<bool?, DateTime?>(null, untilWhen);
                }
            }

            if (x == null) return new Tuple<bool?, DateTime?>(false, null);
            ;

            var r = CheckIfOurBotIsPresent(x);
            if (r) id_of_chats_we_know_are_ok[x5.Id] = r;

            return new Tuple<bool?, DateTime?>(r, null);
        }
    }
}