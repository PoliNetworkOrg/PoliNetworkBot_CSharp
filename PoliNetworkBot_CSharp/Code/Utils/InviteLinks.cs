#region

using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class InviteLinks
    {
        internal static async Task<int> FillMissingLinksIntoDB_Async(TelegramBotAbstract sender)
        {
            const string q1 = "SELECT id FROM Groups WHERE link IS NULL OR link = ''";
            var dt = SqLite.ExecuteSelect(q1);

            var n = 0;
            if (dt == null || dt.Rows.Count == 0)
                return n;

            foreach (DataRow dr in dt.Rows)
            {
                var success = await CreateInviteLinkAsync((long)dr.ItemArray[0], sender);
                if (success)
                    n++;
            }

            return n;
        }

        internal static async Task<bool> CreateInviteLinkAsync(long chatId, TelegramBotAbstract sender)
        {
            string r = null;
            try
            {
                r = await sender.ExportChatInviteLinkAsync(chatId);
            }
            catch
            {
                // ignored
            }

            if (string.IsNullOrEmpty(r))
                return false;

            const string q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                {"@link", r},
                {"@lul", DateTime.Now},
                {"@id", chatId}
            });

            return true;
        }

        internal static async Task UpdateLinksFromJsonAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            try
            {
                if (e.Message.Chat.Type != ChatType.Private)
                    return;

                if (!Utils.Owners.CheckIfOwner(e.Message.From.Id))
                    return;

                if (e.Message.ReplyToMessage == null)
                    return;

                if (e.Message.ReplyToMessage.Document == null)
                    return;

                var d = e.Message.ReplyToMessage.Document;
                var f = await sender.DownloadFileAsync(d);
                Console.WriteLine(f.Item2.Length);
                f.Item2.Seek(0, SeekOrigin.Begin);
                StreamReader reader = new StreamReader(f.Item2);
                string text = reader.ReadToEnd();

                object obj = Newtonsoft.Json.JsonConvert.DeserializeObject(text);
                Console.WriteLine(obj.GetType());
                Newtonsoft.Json.Linq.JArray jArray = (Newtonsoft.Json.Linq.JArray)obj;

                List<Tuple<GruppoTG, bool>> L = new List<Tuple<GruppoTG, bool>>();

                foreach (Newtonsoft.Json.Linq.JToken x in jArray)
                {
                    try
                    {
                        Newtonsoft.Json.Linq.JObject jObject = (Newtonsoft.Json.Linq.JObject)x;
                        GruppoTG gruppoTG = new GruppoTG(jObject["id_link"], jObject["class"]);

                        long? group_id = null;
                        if (!string.IsNullOrEmpty(gruppoTG.idLink))
                        {
                            string s = "SELECT id FROM Groups " +
                                "WHERE Groups.link LIKE '%" + gruppoTG.idLink + "%'";

                            DataTable r1 = Utils.SqLite.ExecuteSelect(s);
                            if (r1 != null && r1.Rows != null && r1.Rows.Count > 0 && r1.Rows[0] != null && r1.Rows[0].ItemArray != null && r1.Rows[0].ItemArray.Length > 0)
                            {
                                var r2 = r1.Rows[0];
                                object r3 = r2.ItemArray[0];
                                group_id = Convert.ToInt64(r3);
                            }
                        }

                        if (group_id == null && !string.IsNullOrEmpty(gruppoTG.nome))
                        {
                            string s = "SELECT id FROM Groups WHERE Groups.title LIKE '%" + gruppoTG.nome + "%'";
                            DataTable r1 = Utils.SqLite.ExecuteSelect(s);
                            if (r1 != null && r1.Rows != null && r1.Rows.Count > 0 && r1.Rows[0] != null && r1.Rows[0].ItemArray != null && r1.Rows[0].ItemArray.Length > 0)
                            {
                                var r2 = r1.Rows[0];
                                object r3 = r2.ItemArray[0];
                                group_id = Convert.ToInt64(r3);
                            }
                        }

                        bool success = false;
                        if (group_id != null)
                        {
                            gruppoTG.UpdateID(group_id.Value);

                            success = await InviteLinks.CreateInviteLinkAsync(group_id.Value, sender);
                        }

                        L.Add(new Tuple<GruppoTG, bool>(gruppoTG, success));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

                string s2 = "Generati " + L.Count.ToString() + " links";

                await sender.SendTextMessageAsync(e.Message.From.Id,
                    new Language(
                        new Dictionary<string, string>() { { "it",
                                        s2 } }),
                    ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);

                foreach (var l2 in L)
                {
                    try
                    {
                        string s3 = "Success: " + (l2.Item2 ? "S" : "N") + "\n" +
                            "IdLink: " + l2.Item1.idLink + "\n" +
                            "Nome: " + l2.Item1.nome;

                        await sender.SendTextMessageAsync(e.Message.From.Id,
                            new Language(
                                new Dictionary<string, string>() { { "it",
                                        s3 } }),
                            ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2);
                    }

                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}