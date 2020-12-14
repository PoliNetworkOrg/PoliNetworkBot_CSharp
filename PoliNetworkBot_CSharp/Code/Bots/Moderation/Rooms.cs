using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Objects.WebObject;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal class Rooms
    {
        internal static async System.Threading.Tasks.Task RoomsMainAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            if (e.Message.Chat.Type != Telegram.Bot.Types.Enums.ChatType.Private)
                return;

            Language question = new Language(dict: new System.Collections.Generic.Dictionary<string, string>() {
                {"it", "Scegli:" },
                {"en", "Choose:"}
            });

            List<Language> options2 = new List<Language>() {
                new Language(dict: new Dictionary<string, string>(){
                    {"it", "Cerca aule" },
                    {"en", "Search classroom" }
                }),
                new Language(dict: new Dictionary<string, string>(){
                    {"it", "Aule libere" },
                    {"en", "Free classroom" }
                }),
                new Language(dict: new Dictionary<string, string>(){
                    {"it", "Occupazioni del giorno" },
                    {"en", "Occupancies of the day" }
                }),

                new Language(dict: new Dictionary<string, string>(){
                    {"it", "Aiuto" },
                    {"en", "Help" }
                })
            };
            var o3 = KeyboardMarkup.ArrayToMatrixString(options2);

            var r = await AskUser.AskBetweenRangeAsync(e.Message.From.Id, question: question, lang: e.Message.From.LanguageCode,
               options: o3, username: e.Message.From.Username, sendMessageConfirmationChoice: true, sender: sender);

            int? chosen = Language.FindChosen(options2, r, e.Message.From.LanguageCode);
            if (chosen == null)
                return;

            switch (chosen.Value)
            {
                case 0:
                    {
                        await SearchClassroomAsync(sender, e);
                        return;
                    }

                case 1:
                    {
                        await FreeClassroomAsync(sender, e);
                        return;
                    }

                case 2:
                    {
                        await OccupanciesOfTheDayAsync(sender, e);
                        return;
                    }

                case 3:
                    {
                        await HelpAsync(sender, e);
                        return;
                    }
            }

            Language text = new Language(dict: new Dictionary<string, string>() {
                {"it", "Hai compiuto una scelta che non era possibile compiere." },
                {"en", "You choose something that was not possible to choose" }
            });
            //wrong choice: (should be impossible)
            await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id, e.Message.From.LanguageCode,
                e.Message.From.Username, text,
                parseMode: ParseMode.Default,
                null);
        }

        private static async System.Threading.Tasks.Task HelpAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Language text = new Language(dict: new Dictionary<string, string>() {
                {"it", "Usa /rooms per cercare le aule!"} ,
                {"en", "Use /rooms to find rooms!"}
            });
            await SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode, e.Message.From.Username, text: text, parseMode: ParseMode.Default, null);
        }

        private static async System.Threading.Tasks.Task FreeClassroomAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            List<HtmlNode> t3 = await GetDailySituationAsync(sender, e);
            if (t3 == null)
            {
                Language text4 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Errore nella consultazione del sito del polimi!" },
                    {"en", "Error while getting polimi website!" }
                });
                await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    langCode: e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text: text4,
                    parseMode: ParseMode.Default, null);
                return;
            }

            Tuple<DateTime, DateTime> tupleTime = await GetStartAndStopHoursAsync(sender, e);

            List<string> t4 = GetFreeRooms(table: t3[0], tupleTime.Item1, tupleTime.Item2);
            if (t4 == null || t4.Count == 0)
            {
                Language text3 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Nessuna aula libera trovata!" },
                    {"en", "No free rooms found!" }
                });
                await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    langCode: e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text: text3,
                    parseMode: ParseMode.Default, null);
                return;
            }

            string reply_text = "";
            foreach (var room in t4)
            {
                reply_text += room + "\n";
            }

            Language text2 = new Language(dict: new Dictionary<string, string>() {
                    {"en", reply_text }
                });
            await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                langCode: e.Message.From.LanguageCode,
                e.Message.From.Username,
                text: text2,
                parseMode: ParseMode.Html, null);
            return;
        }

        private static async System.Threading.Tasks.Task<Tuple<DateTime, DateTime>> GetStartAndStopHoursAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Language question = new Language(dict: new Dictionary<string, string>() {
                {"it", "Ora di inizio? (esempio 8:15)" },
                {"en", "Start time? (example 8:15)" }
            });
            string start = await AskUser.AskAsync(e.Message.From.Id, question: question,
                sender, e.Message.From.LanguageCode, e.Message.From.Username, false);

            Language question2 = new Language(dict: new Dictionary<string, string>() {
                {"it", "Ora di fine? (esempio 11:15)" },
                {"en", "End time? (example 11:15)" }
            });
            string end = await AskUser.AskAsync(e.Message.From.Id, question: question2,
                sender, e.Message.From.LanguageCode, e.Message.From.Username, false);

            if (start.Contains(":") && end.Contains(":"))
            {
                var start2 = start.Split(":");
                var end2 = end.Split(":");

                try
                {
                    DateTime dt1 = new DateTime(2000, 1, 1, Convert.ToInt32(start2[0]), Convert.ToInt32(start2[1]), 0);
                    DateTime dt2 = new DateTime(2000, 1, 1, Convert.ToInt32(end2[0]), Convert.ToInt32(end2[1]), 0);
                    return new Tuple<DateTime, DateTime>(dt1, dt2);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static List<string> GetFreeRooms(HtmlNode table, DateTime start, DateTime stop)
        {
            if (table == null || table.ChildNodes == null)
                return null;

            List<string> result = new List<string>();

            foreach (var child in table.ChildNodes)
            {
                if (child == null)
                    continue;

                if (child.GetClasses().Contains("normalRow"))
                {
                    if (child.ChildNodes != null)
                    {
                        bool? isRowEmptyBool = IsRowEmpty(child, start, stop);
                        if (isRowEmptyBool != null && isRowEmptyBool.Value)
                        {
                            try
                            {
                                var a2 = child.ChildNodes[1];
                                if (a2.ChildNodes != null && a2.ChildNodes.Count > 0)
                                {
                                    bool toAdd = false;
                                    string name = "";

                                    var a1 = a2.ChildNodes[0];
                                    name = a1.InnerHtml.Trim();
                                    if (string.IsNullOrEmpty(name) == false)
                                    {
                                        toAdd = true;
                                    }
                                    else if (a2.ChildNodes.Count > 1)
                                    {
                                        var a3 = a2.ChildNodes[1];
                                        name = a3.InnerHtml.Trim();
                                        if (string.IsNullOrEmpty(name) == false)
                                        {
                                            toAdd = true;
                                        }
                                    }

                                    if (toAdd)
                                    {
                                        result.Add(name);
                                    }
                                }
                            }
                            catch
                            {
                                ;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static bool? IsRowEmpty(HtmlNode node, DateTime start, DateTime stop)
        {
            if (node == null || node.ChildNodes == null)
                return null;

            int shiftStart = (start.Hour - 8) * 4;
            int shiftEnd = (stop.Hour - 8) * 4;

            shiftStart += start.Minute / 15;
            shiftEnd += stop.Minute / 15;

            int colsizetotal = 0;
            for (int i = 2; i < node.ChildNodes.Count; i++)
            {
                int colsize;
                if (node.ChildNodes[i].Attributes.Contains("colspan"))
                {
                    colsize = Convert.ToInt32(node.ChildNodes[i].Attributes["colspan"].Value);
                }
                else
                {
                    colsize = 1;
                }

                colsizetotal += colsize;

                int v = i - 2 - 1 + colsizetotal;
                if (v >= shiftStart && v <= shiftEnd)
                {
                    if (string.IsNullOrEmpty(node.ChildNodes[i].InnerHtml.Trim()) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static async System.Threading.Tasks.Task SearchClassroomAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            Language question = new Language(dict: new Dictionary<string, string>() {
                {"it", "Nome dell'aula?" },
                {"en", "Name of the room?" }
            });
            string sigla = await AskUser.AskAsync(e.Message.From.Id, question: question, sender,
                e.Message.From.LanguageCode, e.Message.From.Username);

            string url = "https://www7.ceda.polimi.it/spazi/spazi/controller/RicercaAula.do?spazi___model" +
                "___formbean___RicercaAvanzataAuleVO___postBack=true&spazi___model___formbean___" +
                "RicercaAvanzataAuleVO___formMode=FILTER&evn_ricerca_avanzata=&spazi___model___formbean___" +
                "RicercaAvanzataAuleVO___sede=tutte&spazi___model___formbean___RicercaAvanzataAuleVO___sigla=" +
                sigla + "&spazi___model___formbean___RicercaAvanzataAuleVO___categoriaScelta=tutte&spazi" +
                "___model___formbean___RicercaAvanzataAuleVO___tipologiaScelta=tutte&spazi___model" +
                "___formbean___RicercaAvanzataAuleVO___iddipScelto=tutti&spazi___model___formbean___" +
                "RicercaAvanzataAuleVO___soloPreseElettriche_default=N&spazi___model___formbean___" +
                "RicercaAvanzataAuleVO___soloPreseDiRete_default=N";

            WebReply webReply = await Utils.Web.DownloadHtmlAsync(url, System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            if (webReply == null || !webReply.IsValid())
            {
                return; //todo: notify user that download failed
            }

            ;

            var doc = new HtmlDocument();
            doc.LoadHtml(webReply.GetData());

            var t1 = Utils.HtmlUtil.GetElementsByTagAndClassName(doc?.DocumentNode, "", "TableDati-tbody", null);

            ;

            HtmlNode t2 = t1?[0];

            ;

            var t3 = Utils.HtmlUtil.GetElementsByTagAndClassName(t2, "tr", "", null);

            ;

            int? roomIndex = FindRoomIndex(t3, sigla);
            if (roomIndex == null)
            {
                return; //todo: send to the user "room not found"
            }

            ;

            var t4 = t3[roomIndex.Value];

            ;

            var t5 = Utils.HtmlUtil.GetElementsByTagAndClassName(t4, "td", "", null);

            ;

            if (t5.Count < 3)
            {
                return; //todo: send to the user "room not found"
            }

            var t6 = t5[2];

            ;

            var t7 = Utils.HtmlUtil.GetElementsByTagAndClassName(t6, "a", "", null);

            if (t7.Count < 1)
            {
                return; //todo: send to the user "room not found"
            }

            ;

            var t8 = t7[0];

            ;

            var t9 = t8.Attributes;

            if (t9 == null)
            {
                return; //todo: send to the user "room not found"
            }

            HtmlAttribute t10 = t9["href"];

            if (t10 == null)
            {
                return; //todo: send to the user "room not found"
            }

            if (string.IsNullOrEmpty(t10.Value))
            {
                return; //todo: send to the user "room not found"
            }

            string result = "https://www7.ceda.polimi.it/spazi/spazi/controller/" + t10.Value;
            Language text2 = new Language(dict: new Dictionary<string, string>() {
                {"en", result }
            });
            await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                e.Message.From.LanguageCode, e.Message.From.Username,
                text2, parseMode: ParseMode.Html, null);
        }

        private static int? FindRoomIndex(List<HtmlNode> t3, string sigla)
        {
            if (t3 == null || t3.Count == 0)
                return null;

            for (int i = 0; i < t3.Count; i++)
            {
                var t4 = t3[i];
                if (t4 == null || t4.ChildNodes == null || t4.ChildNodes.Count < 2)
                    continue;

                var t6 = Utils.HtmlUtil.GetElementsByTagAndClassName(t4, "td", "", null);

                ;

                if (t6 == null || t6.Count < 2)
                    return null;

                var t7 = t6[1];

                ;

                var t8 = Utils.HtmlUtil.GetElementsByTagAndClassName(t7, "b", "", null);

                ;

                var t9 = t8[0].InnerHtml.Trim();

                ;

                if (t9 == sigla)
                    return i;

                bool? found = Utils.StringUtil.CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(sigla, t8[0]);
                if (found != null && found.Value)
                    return i;
            }

            return null;
        }

        private static async System.Threading.Tasks.Task OccupanciesOfTheDayAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            List<HtmlNode> t3 = await GetDailySituationAsync(sender, e);

            if (t3 == null)
            {
                Language text2 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Errore nella consultazione del sito del polimi!" },
                    {"en", "Error while getting polimi website!" }
                });
                await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    langCode: e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text: text2,
                    parseMode: ParseMode.Default, null);
                return;
            }

            Language question = new Language(dict: new Dictionary<string, string>() {
                { "en", "Which room? (example: 3.0.1)"},
                {"it", "Quale aula? (esempio 3.0.1)" }
            });
            string roomName = await AskUser.AskAsync(idUser: e.Message.From.Id, question: question, sender: sender, lang: e.Message.From.LanguageCode,
                username: e.Message.From.Username, true);
            List<HtmlNode> t4 = GetRoomTitleAndHours(table: t3[0], roomName: roomName);

            ;

            if (t4 == null || t4.Count == 0)
            {
                Language text2 = new Language(dict: new Dictionary<string, string>() {
                    {"it", "Aula non trovata!" },
                    {"en", "Room not found!" }
                });
                await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id,
                    langCode: e.Message.From.LanguageCode,
                    e.Message.From.Username,
                    text: text2,
                    parseMode: ParseMode.Default, null);
                return;
            }

            string htmlresult = "<html><head><style>td {border: 1px solid;}</style></head><body><table>";
            foreach (var t5 in t4)
            {
                htmlresult += t5.OuterHtml;
            }
            htmlresult += "</table></body></html>";

            ;

            TeleSharp.TL.TLAbsInputPeer peer2 = Utils.UserbotPeer.GetPeerFromIdAndType(e.Message.From.Id,
                Telegram.Bot.Types.Enums.ChatType.Private);
            Tuple<TeleSharp.TL.TLAbsInputPeer, long> peer = new Tuple<TeleSharp.TL.TLAbsInputPeer, long>(peer2, e.Message.From.Id);
            Language text = new Language(dict: new Dictionary<string, string>() {
                { "en", roomName }
            });
            TelegramFile document = Utils.UtilsMedia.UtilsFileText.GenerateFileFromString(htmlresult, roomName + ".html",
                roomName, "text/html");

            await sender.SendFileAsync(documentInput: document,
                peer: peer, text: text,
                textAsCaption: Enums.TextAsCaption.AS_CAPTION,
                e.Message.From.Username, e.Message.From.LanguageCode, null, true);
        }

        private static async System.Threading.Tasks.Task<List<HtmlNode>> GetDailySituationAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            int day;
            int month;
            int year;

            Tuple<DateTimeSchedule, Exception, string> datetime = await Utils.DateTimeClass.AskDateAsync(e.Message.From.Id, "Scegli un giorno", "it", sender, e.Message.From.Username);

            if (datetime.Item2 != null)
            {
                throw datetime.Item2;
                return null;
            }

            var d2 = datetime.Item1.GetDate();
            if (d2 == null)
            {
                return null;
            }

            day = d2.Value.Day;
            month = d2.Value.Month;
            year = d2.Value.Year;

            string sede = await AskUser.GetSedeAsync(sender, e);
            if (string.IsNullOrEmpty(sede))
            {
                return null;
            }

            string url = "https://www7.ceda.polimi.it/spazi/spazi/controller/OccupazioniGiornoEsatto.do?" +
          "csic=" + sede +
          "&categoria=tutte" +
          "&tipologia=tutte" +
          "&giorno_day=" + day.ToString() +
          "&giorno_month=" + month.ToString() +
          "&giorno_year=" + year.ToString() +
          "&jaf_giorno_date_format=dd%2FMM%2Fyyyy&evn_visualizza=";

            WebReply html = await Utils.Web.DownloadHtmlAsync(url, System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            if (html.IsValid() == false)
            {
                return null;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html.GetData());

            ;

            var t1 = Utils.HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "", "BoxInfoCard", 1);

            ;

            var t3 = Utils.HtmlUtil.GetElementsByTagAndClassName(t1[0], "", "scrollContent", null);
            return t3;
        }

        private static List<HtmlNode> GetRoomTitleAndHours(HtmlNode table, string roomName)
        {
            if (table == null)
                return null;

            if (string.IsNullOrEmpty(roomName))
                return null;

            List<HtmlNode> result = new List<HtmlNode>();
            int? roomIndex = FindRoom(table, roomName);
            if (roomIndex == null)
                return null;

            int? titleIndex = FindTitleIndex(table, roomIndex.Value);
            if (titleIndex == null)
                return null;

            result.Add(table.ChildNodes[titleIndex.Value]);
            result.Add(table.ChildNodes[titleIndex.Value + 1]);
            result.Add(table.ChildNodes[titleIndex.Value + 2]);
            result.Add(table.ChildNodes[roomIndex.Value]);

            return result;
        }

        private static int? FindTitleIndex(HtmlNode table, int roomIndex)
        {
            for (int i = roomIndex; i >= 0; i--)
            {
                HtmlNode child = table.ChildNodes[i];
                if (child.GetClasses().Contains("normalRow") == false)
                {
                    return i - 2;
                }
            }

            return null;
        }

        private static int? FindRoom(HtmlNode table, string roomName)
        {
            for (int i = 0; i < table.ChildNodes.Count; i++)
            {
                HtmlNode child = table.ChildNodes[i];

                if (child.ChildNodes != null && child.GetClasses().Contains("normalRow"))
                {
                    foreach (var child2 in child.ChildNodes)
                    {
                        bool? found = Utils.StringUtil.CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(roomName, child2);
                        if (found != null && found.Value)
                            return i;
                    }
                }
            }

            return null;
        }
    }
}