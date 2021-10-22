using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;

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
                var groupsRaw = File.ReadAllText(@"C:\Users\eliam\Documents\groups.csv");
                var groups = Regex.Split(groupsRaw, "\r\n|\r|\n");
                //using StreamWriter groupsFile = new StreamWriter(@"C:\Users\eliam\Documents\WriteLines.txt", append: true);
                //await groupsFile.WriteLineAsync("Nome Gruppo $ Link di Invito");
                using (var sw = File.AppendText(@"C:\Users\eliam\Documents\groupslist.txt"))
                {
                    sw.WriteLine("Nome Gruppo $ Link di Invito");
                }

                foreach (var group in groups)
                {
                    var toBeDone = true;
                    while (toBeDone)
                        if (group.Length > 0)
                            try
                            {
                                //    await telegramBotAbstract.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync();
                                //file,,,
                                var name = group;
                                if (name.Length > 255)
                                    using (var sw = File.AppendText(@"C:\Users\eliam\Documents\errorlist.txt"))
                                    {
                                        sw.WriteLine(name + " FAILED");
                                    }

                                var desc = "Gruppo @polinetwork \nPer tutti i link: polinetwork.github.io";

                                var members = new List<long>(); //ID members to insert
                                long? chatID = null;
                                while (chatID == null)
                                    chatID = await telegramBotAbstract.CreateGroup(name, desc, members);
                                Thread.Sleep(1 * 1000 * 10);
                                var channel = await telegramBotAbstract.UpgradeGroupIntoSupergroup(chatID);
                                if (channel == null)
                                    return;
                                //await telegramBotAbstract.EditDescriptionChannel(channel, desc);
                                Thread.Sleep(1 * 1000 * 10);
                                await telegramBotAbstract.AddUserIntoChannel("@polinetwork3bot", channel);

                                var admins = new List<TLInputUser>();

                                var adminTags = new List<string>
                                    {"polinetwork3bot"}; //tag members to set admins (MUST BE INSIDE THE members ARRAY)
                                foreach (var admin in adminTags)
                                {
                                    Thread.Sleep(1 * 1000 * 10);
                                    TLAbsInputPeer u =
                                        await UserbotPeer.GetPeerUserWithAccessHash(admin,
                                            telegramBotAbstract._userbotClient);
                                    if (u is TLInputPeerUser u2)
                                    {
                                        var user1 = new TLInputUser { AccessHash = u2.AccessHash, UserId = u2.UserId };
                                        admins.Add(user1);
                                    }
                                }

                                foreach (var admin in admins)
                                {
                                    Thread.Sleep(1 * 1000 * 10);
                                    await telegramBotAbstract.PromoteChatMember(admin, channel.Id, channel.AccessHash);
                                }

                                Thread.Sleep(1 * 1000 * 10);
                                var link = await telegramBotAbstract.ExportChatInviteLinkAsync(channel.Id,
                                    channel.AccessHash);
                                links.Add(link);
                                using (var sw = File.AppendText(@"C:\Users\eliam\Documents\groupslist.txt"))
                                {
                                    sw.WriteLine(group + " $ " + link);
                                }

                                Console.WriteLine("added: " + group + " $ " + link);
                                Thread.Sleep(5 * 1000 * 60);
                                toBeDone = false;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message + " -> In Main Thread!");
                                Thread.Sleep(int.Parse(Regex.Match(e.Message, @"\d+").Value) * 1000);
                                await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
                            }
                }

                Console.WriteLine("====== CREATION COMPLETE ======");
            }
            catch (Exception ignore)
            {
                ;
            }
        }
    }
}