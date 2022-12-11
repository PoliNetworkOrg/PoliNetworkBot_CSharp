#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Dispatcher;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.SpamCheck;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.Notify;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

public static class Main
{
    internal static void MainMethod(object? sender, MessageEventArgs? e)
    {
        var t = new Thread(() =>
        {
            if (sender != null && e != null) _ = MainMethod2(new TelegramBotParam(sender, false), e);
        });
        t.Start();
        //var t1 = new Thread(() => _ = CheckAllowedMessageExpiration(sender, e));
        //t1.Start();
    }

    public static async Task<ActionDoneObject> MainMethod2(TelegramBotParam sender, MessageEventArgs? e)
    {
        TelegramBotAbstract? telegramBotClient = null;

        try
        {
            telegramBotClient = sender.GetTelegramBot();

            if (telegramBotClient == null || e?.Message == null)
                return new ActionDoneObject(ActionDoneEnum.NONE, null, null);


            Tuple<ToExit?, ChatMember[]?, List<int>?, string?>? toExit = null;
            try
            {
                toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
            }

            if (toExit is { Item1: ToExit.EXIT })
            {
                var itemToPrint = MemberListToString(toExit.Item2);
                var itemToPrint2 = ListIntToString(toExit.Item3);
                var itemToPrint3 = StringToStringToBePrinted(toExit.Item4);
                var itemToPrintFull = itemToPrint + "\n" + e.Message.Chat.Title;
                itemToPrintFull += "\n----\n" + itemToPrint2 + "\n----\nS:" + itemToPrint3;
                itemToPrintFull += "\n----\n" + e.Message.Chat.Id;
                itemToPrintFull += "\n@@@@@@";

                await Groups.SendMessageExitingAndThenExit(telegramBotClient, e);

                throw new ToExitException(itemToPrintFull);
            }

            Groups.CheckForGroupUpdate(telegramBotClient, e);

            var notAuthorizedBotHasBeenAddedBool =
                await ModerationCheck.CheckIfNotAuthorizedBotHasBeenAdded(e, telegramBotClient);
            if (notAuthorizedBotHasBeenAddedBool is { Count: > 0 })
            {
                foreach (var bot in notAuthorizedBotHasBeenAddedBool)
                    await RestrictUser.BanUserFromGroup(telegramBotClient, bot, e.Message.Chat.Id, null, true);

                Console.WriteLine("todo: send message \"Bots not allowed here!\"");
            }

            await AddedUsersUtil.DealWithAddedUsers(telegramBotClient, e);

            if (BanMessageDetected(e, telegramBotClient))
            {
                var b = await CommandDispatcher.BanMessageActions(telegramBotClient, e);
                return new ActionDoneObject(ActionDoneEnum.BANNED, b, null);
            }

            var toExitBecauseUsernameAndNameCheck =
                await ModerationCheck.CheckUsernameAndName(e, telegramBotClient);
            if (toExitBecauseUsernameAndNameCheck)
                return new ActionDoneObject(ActionDoneEnum.USERNAME_WARNING, null, null);

            Tuple<SpamType, bool?>? result = null;
            try
            {
                result = await CheckSpam.CheckSpamMethod(e, telegramBotClient);
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex);
            }

            switch (result)
            {
                case null:
                    return new ActionDoneObject(ActionDoneEnum.NONE, null, null);
                case { Item2: { } }:
                    return new ActionDoneObject(ActionDoneEnum.CHECK_SPAM, result.Item2, result.Item1);
            }

            if (e.Message.Text != null && e.Message.Text.StartsWith("/"))
            {
                var x = await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
                return new ActionDoneObject(ActionDoneEnum.COMMAND, x, result.Item1);
            }

            var y = await TextConversation.DetectMessage(telegramBotClient, e);
            return new ActionDoneObject(y.ActionDoneEnum, y.Done, result.Item1);
        }
        catch (Exception? exception)
        {
            Logger.WriteLine(exception.Message);

            await NotifyUtil.NotifyOwnerWithLog2(exception, telegramBotClient, EventArgsContainer.Get(e));
        }

        return new ActionDoneObject(ActionDoneEnum.NONE, false, null);
    }

    private static bool BanMessageDetected(MessageEventArgs? messageEventArgs, TelegramBotAbstract? sender)
    {
        try
        {
            if (messageEventArgs is { Message: { } } && (messageEventArgs.Message.Text != null ||
                                                         messageEventArgs.Message.Type != MessageType.ChatMemberLeft))
                return false;
            if (messageEventArgs != null && messageEventArgs.Message?.From?.Id == null) return false;
            if (messageEventArgs?.Message?.LeftChatMember?.Id == null) return false;
            return GlobalVariables.Bots != null &&
                   messageEventArgs.Message.From?.Id != messageEventArgs.Message.LeftChatMember?.Id &&
                   GlobalVariables.Bots.Keys.All(botsKey =>
                       messageEventArgs.Message.From != null && messageEventArgs.Message.From.Id != botsKey);
        }
        catch (Exception? e)
        {
            _ = NotifyUtil.NotifyOwnerWithLog2(e, sender, EventArgsContainer.Get(messageEventArgs));
            return false;
        }
    }

    private static string StringToStringToBePrinted(string? item4)
    {
        if (item4 == null)
            return "[NULL]";

        return item4.Length == 0 ? "[EMPTY]" : item4;
    }

    private static string ListIntToString(IReadOnlyCollection<int>? item3)
    {
        if (item3 == null)
            return "[NULL]";

        return item3.Count == 0 ? "[EMPTY]" : item3.Aggregate("", (current, item4) => current + (item4 + "\n"));
    }

    private static string MemberListToString(IReadOnlyCollection<ChatMember>? item2)
    {
        if (item2 == null)
            return "[NULL]";

        return item2.Count == 0
            ? "[EMPTY]"
            : item2.Aggregate("", (current, item3) => current + item3.User.Username + " " + item3.Status + "\n");
    }
}