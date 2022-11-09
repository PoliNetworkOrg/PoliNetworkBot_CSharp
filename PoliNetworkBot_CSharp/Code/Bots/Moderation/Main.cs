#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Conversation;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

internal static class Main
{
    internal static void MainMethod(object? sender, MessageEventArgs? e)
    {
        var t = new Thread(() =>
        {
            if (sender != null && e != null) _ = MainMethod2(sender, e);
        });
        t.Start();
        //var t1 = new Thread(() => _ = CheckAllowedMessageExpiration(sender, e));
        //t1.Start();
    }

    private static async Task<bool> MainMethod2(object sender, MessageEventArgs? e)
    {
        TelegramBotClient? telegramBotClientBot = null;
        TelegramBotAbstract? telegramBotClient = null;

        try
        {
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return false;

            if (e?.Message == null)
                return false;

            telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

            var toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (toExit is { Item1: ToExit.EXIT })
            {
                var itemToPrint = MemberListToString(toExit.Item2);
                var itemToPrint2 = ListIntToString(toExit.Item3);
                var itemToPrint3 = StringToStringToBePrinted(toExit.Item4);
                var itemToPrintFull = itemToPrint + "\n" + e.Message?.Chat.Title;
                itemToPrintFull += "\n----\n" + itemToPrint2 + "\n----\nS:" + itemToPrint3;
                itemToPrintFull += "\n----\n" + e.Message?.Chat.Id;
                itemToPrintFull += "\n@@@@@@";

                await Groups.SendMessageExitingAndThenExit(telegramBotClient, e);

                throw new ToExitException(itemToPrintFull);
            }

            Groups.CheckForGroupUpdate(telegramBotClient, e);

            var notAuthorizedBotHasBeenAddedBool =
                await ModerationCheck.CheckIfNotAuthorizedBotHasBeenAdded(e, telegramBotClient);
            if (notAuthorizedBotHasBeenAddedBool is { Count: > 0 })
                if (e.Message != null)
                {
                    foreach (var bot in notAuthorizedBotHasBeenAddedBool)
                        await RestrictUser.BanUserFromGroup(telegramBotClient, bot, e.Message.Chat.Id, null, true);

                    Console.WriteLine("todo: send message \"Bots not allowed here!\"");
                }

            await AddedUsersUtil.DealWithAddedUsers(telegramBotClient, e);

            if (BanMessageDetected(e, telegramBotClient))
                return await CommandDispatcher.BanMessageActions(telegramBotClient, e);

            var toExitBecauseUsernameAndNameCheck =
                await ModerationCheck.CheckUsernameAndName(e, telegramBotClient);
            if (toExitBecauseUsernameAndNameCheck)
                return false;

            var checkSpam = await ModerationCheck.CheckSpamAsync(e, telegramBotClient);
            if (checkSpam != SpamType.ALL_GOOD && checkSpam != SpamType.SPAM_PERMITTED)
                return await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);

            if (checkSpam == SpamType.SPAM_PERMITTED)
                return await ModerationCheck.PermittedSpamMeasure(telegramBotClient, EventArgsContainer.Get(e));

            if (e.Message?.Text != null && e.Message.Text.StartsWith("/"))
                return await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
            await TextConversation.DetectMessage(telegramBotClient, e);
        }
        catch (Exception? exception)
        {
            Logger.WriteLine(exception.Message);

            await NotifyUtil.NotifyOwnerWithLog2(exception, telegramBotClient, EventArgsContainer.Get(e));
        }

        return false;
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