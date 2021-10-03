using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Code.Bots.Administration
{
    internal class Main
    {
        internal static async System.Threading.Tasks.Task MainMethodAsync(TelegramBotAbstract telegramBotAbstract)
        {
            ;

            try
            {
                //    await telegramBotAbstract.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync();
                //file,,,
                string name = "Test group sept 2021 t3";
                string desc = "Desc test";
                List<long> members = new List<long>() { 107050697, 768169879 };
                long? chatID = await telegramBotAbstract.CreateGroup(name, "", members);
                if (chatID == null)
                {
                    return;
                }
                var channel = await telegramBotAbstract.UpgradeGroupIntoSupergroup(chatID);
                if (channel == null)
                    return;

                await telegramBotAbstract.EditDescriptionChannel(channel, desc);

                List<long> admins = new List<long>() { 768169879 };

                foreach (var admin in admins)
                {
                    await telegramBotAbstract.PromoteChatMember(Convert.ToInt32(admin), channel.Id, channel.AccessHash);
                }
            }
            catch (Exception e)
            {
                await NotifyUtil.NotifyOwners(e, telegramBotAbstract, 0);
            }
        }
    }
}