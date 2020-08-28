#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot;
using Telegram.Bot.Args;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal static class Main
    {
        internal static void MainMethod(object sender, MessageEventArgs e)
        {
            var t = new Thread(() => _ = MainMethod2(sender, e));
            t.Start();
        }

        private static async Task MainMethod2(object sender, MessageEventArgs e)
        {
            TelegramBotClient telegramBotClientBot = null;
            if (sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

            if (telegramBotClientBot == null)
                return;

            var telegramBotClient = TelegramBotAbstract.GetFromRam(telegramBotClientBot);

            var toExit = await ModerationCheck.CheckIfToExitAndUpdateGroupList(telegramBotClient, e);
            if (toExit)
            {
                await LeaveChat.ExitFromChat(telegramBotClient, e);
                return;
            }

            var usernameCheck = ModerationCheck.CheckUsername(e);
            if (usernameCheck == null)
            {
                ;
            }
            else if (usernameCheck.Count == 1)
            {
                if (usernameCheck[0].UsernameBool || usernameCheck[0].Name)
                {
                    await ModerationCheck.SendUsernameWarning(telegramBotClient,  usernameCheck[0].UsernameBool,
                        usernameCheck[0].Name, e.Message.From.LanguageCode, 
                        e.Message.From.Username, chatId: e.Message.Chat.Id,
                        userId: e.Message.From.Id, messageId: e.Message.MessageId,
                        messageChatType: e.Message.Chat.Type, e.Message.From.FirstName, 
                        e.Message.From.LastName);
                    return;
                }
            }
            else
            {
                foreach (var usernameCheck2 in usernameCheck)
                {
                    if (usernameCheck2 != null)
                    {
                        if (usernameCheck2.Name || usernameCheck2.UsernameBool)
                        {
                            await ModerationCheck.SendUsernameWarning(telegramBotClient, 
                                usernameCheck2.UsernameBool,
                                name: usernameCheck2.Name,
                                usernameCheck2.GetLanguage(),
                                usernameCheck2.GetUsername(), 
                                chatId: e.Message.Chat.Id,
                                userId: usernameCheck2.GetUserId(),
                                messageId: null, 
                                messageChatType: e.Message.Chat.Type, 
                                firstName: usernameCheck2.GetFirstName(),
                                lastName: usernameCheck2.GetLastName());
                        }
                    }
                }

                return;
            }



            var checkSpam = ModerationCheck.CheckSpam(e);
            if (checkSpam != SpamType.ALL_GOOD)
            {
                await ModerationCheck.AntiSpamMeasure(telegramBotClient, e, checkSpam);
                return;
            }

            if (string.IsNullOrEmpty(e.Message.Text))
                return;

            if (e.Message.Text.StartsWith("/"))
                await CommandDispatcher.CommandDispatcherMethod(telegramBotClient, e);
            else
                await TextConversation.DetectMessage(telegramBotClient, e);
        }
    }
}