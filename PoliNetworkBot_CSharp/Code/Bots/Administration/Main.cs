using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using TeleSharp.TL;

namespace PoliNetworkBot_CSharp.Code.Bots.Administration
{
    internal class Main
    {
        internal static async System.Threading.Tasks.Task MainMethodAsync(TelegramBotAbstract telegramBotAbstract)
        {
            try
            {
                ;
                List<string> links = new List<string>();
                string groupsRaw = System.IO.File.ReadAllText(@"C:\Users\eliam\Documents\groups.csv");
                string[] groups = Regex.Split(groupsRaw, "\r\n|\r|\n");
                //using StreamWriter groupsFile = new StreamWriter(@"C:\Users\eliam\Documents\WriteLines.txt", append: true);
                //await groupsFile.WriteLineAsync("Nome Gruppo $ Link di Invito");
                using (StreamWriter sw = File.AppendText(@"C:\Users\eliam\Documents\groupslist.txt"))
                {
                    sw.WriteLine("Nome Gruppo $ Link di Invito");
                }
                foreach (string group in groups)
                {   
                    bool toBeDone = true;
                    while (toBeDone == true)
                    {
                        if (group.Length > 0)
                            try
                            {
                                //    await telegramBotAbstract.FixTheFactThatSomeGroupsDoesNotHaveOurModerationBotAsync();
                                //file,,,
                                string name = group;
                                if (name.Length > 255)
                                {
                                    using (StreamWriter sw = File.AppendText(@"C:\Users\eliam\Documents\errorlist.txt"))
                                    {
                                        sw.WriteLine(name + " FAILED");
                                    }
                                }
                                string desc = "Gruppo @polinetwork \nPer tutti i link: polinetwork.github.io";

                                List<long> members = new List<long>() {  }; //ID members to insert
                                long? chatID = null;
                                while (chatID == null)
                                {
                                    chatID = await telegramBotAbstract.CreateGroup(name, desc, members);
                                }
                                Thread.Sleep(1 * 1000 * 10);
                                var channel = await telegramBotAbstract.upgradeGroupIntoSupergroup(chatID);
                                if (channel == null)
                                    return;
                                //await telegramBotAbstract.EditDescriptionChannel(channel, desc);
                                Thread.Sleep(1 * 1000 * 10);
                                await telegramBotAbstract.addUserIntoChannel(userID: "@polinetwork3bot", channel);
                                
                                List<TLInputUser> admins = new List<TLInputUser>();

                                List<String> adminTags = new List<string>() { "polinetwork3bot" }; //tag members to set admins (MUST BE INSIDE THE members ARRAY)
                                foreach (var admin in adminTags)
                                {
                                    Thread.Sleep(1 * 1000 * 10);
                                    TLAbsInputPeer u = await UserbotPeer.GetPeerUserWithAccessHash(admin, telegramBotAbstract._userbotClient);
                                    if (u is TLInputPeerUser u2)
                                    {
                                        TLInputUser user1 = new TLInputUser() { AccessHash = u2.AccessHash, UserId = u2.UserId };
                                        admins.Add(user1);
                                    }
                                    
                                }
                                foreach (var admin in admins)
                                {
                                    Thread.Sleep(1 * 1000 * 10);
                                    await telegramBotAbstract.PromoteChatMember(admin, channel.Id, channel.AccessHash);
                                }
                                Thread.Sleep(1 * 1000 * 10);
                                var link = await telegramBotAbstract.ExportChatInviteLinkAsync(channel.Id, channel.AccessHash);
                                links.Add(link);
                                using (StreamWriter sw = File.AppendText(@"C:\Users\eliam\Documents\groupslist.txt"))
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
                                Thread.Sleep(Int32.Parse(Regex.Match(e.Message, @"\d+").Value) * 1000);
                                await NotifyUtil.NotifyOwners(e, telegramBotAbstract, 0);
                            }
                    }
                }
                Console.WriteLine("====== CREATION COMPLETE ======");
            }
            catch(Exception ignore)
            {
                ;
            }
        }
    }
}