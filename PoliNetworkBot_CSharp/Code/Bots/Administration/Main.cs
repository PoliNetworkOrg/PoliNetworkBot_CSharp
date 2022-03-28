#region

using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Administration
{
    internal class Main
    {
        internal static async Task MainMethodAsync(TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
                ;
                var links = new List<string>();
                var groupsRaw = await File.ReadAllTextAsync(@"C:\Users\eliam\Documents\groups.csv");
                var groups = Regex.Split(groupsRaw, "\r\n|\r|\n");
                //using StreamWriter groupsFile = new StreamWriter(@"C:\Users\eliam\Documents\WriteLines.txt", append: true);
                //await groupsFile.WriteLineAsync("Nome Gruppo $ Link di Invito");
                await using (var sw = File.AppendText(@"C:\Users\eliam\Documents\groupslist.txt"))
                {
                    await sw.WriteLineAsync("Nome Gruppo $ Link di Invito");
                }

                foreach (var group in groups) await MainMethodAsync2Async(group, telegramBotAbstract, links);

                Logger.WriteLine("====== CREATION COMPLETE ======");
            }
            catch (Exception ex1)
            {
                Console.WriteLine(ex1);
            }
        }

        private static async Task MainMethodAsync2Async(string group, TelegramBotAbstract telegramBotAbstract,
            List<string> links)
        {
            var toBeDone = true;
            while (toBeDone)
                if (group.Length > 0)
                    try
                    {
                        //    await telegramBotAbstract.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync();
                        //file,,,
                        if (group.Length > 255)
                        {
                            await using var sw = File.AppendText(@"C:\Users\eliam\Documents\errorlist.txt");
                            await sw.WriteLineAsync(group + " FAILED");
                        }

                        const string desc = "Gruppo @polinetwork \nPer tutti i link: polinetwork.github.io";

                        var members = new List<long>(); //ID members to insert
                        long? chatID = null;
                        while (chatID == null)
                            chatID = await telegramBotAbstract.CreateGroup(group, desc, members);
                        Thread.Sleep(1 * 1000 * 10);
                        var channel = await telegramBotAbstract.UpgradeGroupIntoSupergroup(chatID);
                        if (channel == null)
                            return;
                        //await telegramBotAbstract.EditDescriptionChannel(channel, desc);
                        Thread.Sleep(1 * 1000 * 10);
                        await telegramBotAbstract.AddUserIntoChannel("@polinetwork3bot", channel.channel);

                        var admins = new List<TLInputUser>();

                        var adminTags = new List<string>
                            { "polinetwork3bot" }; //tag members to set admins (MUST BE INSIDE THE members ARRAY)
                        foreach (var admin in adminTags)
                        {
                            Thread.Sleep(1 * 1000 * 10);
                            TLAbsInputPeer u =
                                await UserbotPeer.GetPeerUserWithAccessHash(admin,
                                    telegramBotAbstract.UserbotClient);
                            if (u is not TLInputPeerUser u2) continue;
                            var user1 = new TLInputUser { AccessHash = u2.AccessHash, UserId = u2.UserId };
                            admins.Add(user1);
                        }

                        foreach (var admin in admins)
                        {
                            Thread.Sleep(1 * 1000 * 10);
                            await telegramBotAbstract.PromoteChatMember(admin, channel.channel.Id,
                                channel.channel.AccessHash);
                        }

                        Thread.Sleep(1 * 1000 * 10);
                        var link = await telegramBotAbstract.ExportChatInviteLinkAsync(channel.channel.Id,
                            channel.channel.AccessHash);
                        links.Add(link);
                        await using (var sw = File.AppendText(@"C:\Users\eliam\Documents\groupslist.txt"))
                        {
                            await sw.WriteLineAsync(group + " $ " + link);
                        }

                        Logger.WriteLine("added: " + group + " $ " + link);
                        Thread.Sleep(5 * 1000 * 60);
                        toBeDone = false;
                    }
                    catch (Exception e)
                    {
                        Thread.Sleep(int.Parse(Regex.Match(e.Message, @"\d+").Value) * 1000);
                        await NotifyUtil.NotifyOwners(e, telegramBotAbstract, null);
                    }
        }
    }
}