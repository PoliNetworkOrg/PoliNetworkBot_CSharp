#region

using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
                NuovoLink success = await CreateInviteLinkAsync((long)dr.ItemArray[0], sender);
                if (success.isNuovo)
                    n++;
            }

            return n;
        }

        internal static async Task<NuovoLink> CreateInviteLinkAsync(long chatId, TelegramBotAbstract sender)
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
                return new NuovoLink(false);

            const string q1 = "UPDATE Groups SET link = @link, last_update_link = @lul WHERE id = @id";
            SqLite.Execute(q1, new Dictionary<string, object>
            {
                {"@link", r},
                {"@lul", DateTime.Now},
                {"@id", chatId}
            });

            return new NuovoLink(true, r);
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
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
                        await Task.Delay(100);
                    }
                    catch
                    {
                        ;
                    }

                    try
                    {
                        Newtonsoft.Json.Linq.JObject jObject = (Newtonsoft.Json.Linq.JObject)x;
                        GruppoTG gruppoTG = new GruppoTG(jObject["id_link"], jObject["class"]);

                        long? group_id = null;

                        string sql1 = "empty";

                        if (!string.IsNullOrEmpty(gruppoTG.idLink))
                        {
                            sql1 = "SELECT id FROM Groups " +
                              "WHERE Groups.link LIKE '%" + gruppoTG.idLink + "%'";
                        }

                        try
                        {
                            if (!string.IsNullOrEmpty(gruppoTG.idLink))
                            {
                          

                                DataTable r1 = Utils.SqLite.ExecuteSelect(sql1);
                                if (r1 != null && r1.Rows != null && r1.Rows.Count > 0 && r1.Rows[0] != null && r1.Rows[0].ItemArray != null && r1.Rows[0].ItemArray.Length > 0)
                                {
                                    var r2 = r1.Rows[0];
                                    object r3 = r2.ItemArray[0];
                                    group_id = Convert.ToInt64(r3);
                                }
                            }
                        }
                        catch (Exception ex1)
                        {
                            Console.WriteLine(ex1);
                            string ex1m = "1" + "\n\n" + ex1.Message;
                            await sender.SendTextMessageAsync(e.Message.From.Id,
                             new Language(
                                 new Dictionary<string, string>() { { "it",
                                                    ex1m } }),
                             ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                            return;
                        }

                        string sql2 = "SELECT id FROM Groups WHERE Groups.title LIKE '%' || @nome || '%'";

                        try
                        {
                            if (group_id == null && !string.IsNullOrEmpty(gruppoTG.nome))
                            {
                                DataTable r1 = Utils.SqLite.ExecuteSelect(sql2, new Dictionary<string, object> { { "@nome", gruppoTG.nome } });
                                if (r1 != null && r1.Rows != null && r1.Rows.Count > 0 && r1.Rows[0] != null && r1.Rows[0].ItemArray != null && r1.Rows[0].ItemArray.Length > 0)
                                {
                                    var r2 = r1.Rows[0];
                                    object r3 = r2.ItemArray[0];
                                    group_id = Convert.ToInt64(r3);
                                }
                            }
                        }
                        catch (Exception ex2)
                        {
                            Console.WriteLine(ex2);
                            string ex2m = "2" + "\n\n" + ex2.Message + "\n\n" + sql2 + "\n\n" + gruppoTG.nome;
                            await sender.SendTextMessageAsync(e.Message.From.Id,
                             new Language(
                                 new Dictionary<string, string>() { { "it",
                                                    ex2m } }),
                             ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                            return;
                        }

                        bool success = false;
                        try
                        {
                            if (group_id != null)
                            {
                                gruppoTG.UpdateID(group_id.Value);

                                var s3 = await InviteLinks.CreateInviteLinkAsync(group_id.Value, sender);
                                success = s3.isNuovo;
                                gruppoTG.UpdateNewLink(s3.link);
                            }
                        }
                        catch (Exception ex3)
                        {
                            Console.WriteLine(ex3);
                            string ex3m = "3" + "\n\n" + ex3.Message;
                            await sender.SendTextMessageAsync(e.Message.From.Id,
                             new Language(
                                 new Dictionary<string, string>() { { "it",
                                                    ex3m} }),
                             ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                            return;
                        }

                        L.Add(new Tuple<GruppoTG, bool>(gruppoTG, success));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        string ex4m = "4" + "\n\n" + ex.Message;
                        await sender.SendTextMessageAsync(e.Message.From.Id,
                         new Language(
                             new Dictionary<string, string>() { { "it",
                                                 ex4m} }),
                         ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                        return;
                    }
                }

                string s2 = "Generati " + L.Count.ToString() + " links";

                await sender.SendTextMessageAsync(e.Message.From.Id,
                    new Language(
                        new Dictionary<string, string>() { { "it",
                                        s2 } }),
                    ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);

                string st = "";

                foreach (var l2 in L)
                {
                    try
                    {
                        string s3 = "Success: " + (l2.Item2 ? "S" : "N") + "\n" +
                            "IdLink: " + StringNotNull(l2.Item1.idLink) + "\n" +
                            "NewLink: " + StringNotNull(l2.Item1.newLink) + "\n" +
                            "Nome: " + StringNotNull(l2.Item1.nome);
                        st += s3 + "\n\n";

                        /*await sender.SendTextMessageAsync(e.Message.From.Id,
                            new Language(
                                new Dictionary<string, string>() { { "it",
                                        s3 } }),
                            ChatType.Private, "it", ParseMode.Default, null, e.Message.From.Username);
                        */
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2);
                    }

                    //Thread.Sleep(500);
                }

                Dictionary<string, string> dict = new Dictionary<string, string>() {
                         { "it", "Gruppi con link rigenerati"}
                    };
                Stream stream = GenerateStreamFromString(st);
                Objects.TelegramMedia.TelegramFile tf = new Objects.TelegramMedia.TelegramFile(stream, "groups.txt", "Gruuppi con link rigenerati", "text/plain");
                await sender.SendFileAsync(tf, new Tuple<TeleSharp.TL.TLAbsInputPeer, long>(null, e.Message.From.Id),
                    new Language(dict),
                    Enums.TextAsCaption.AFTER_FILE, e.Message.From.Username, e.Message.From.LanguageCode, null, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static string StringNotNull(string idLink)
        {
            if (idLink == null)
                return "[null]";

            if (idLink == "")
                return "[EMPTY]";

            return idLink;
        }
    }
}