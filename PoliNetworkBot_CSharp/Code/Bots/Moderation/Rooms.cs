using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.TelegramMedia;
using PoliNetworkBot_CSharp.Code.Objects.WebObject;
using PoliNetworkBot_CSharp.Code.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Args;

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
                        SearchClassroom(sender, e);
                        return;
                    }

                case 1:
                    {
                        FreeClassroom(sender, e);
                        return;
                    }

                case 2:
                    {
                        OccupanciesOfTheDayAsync(sender, e);
                        return;
                    }

                case 3:
                    {
                        Help(sender, e);
                        return;
                    }


            }

            Language text = new Language(dict: new Dictionary<string, string>() {
                {"it", "Hai compiuto una scelta che non era possibile compiere." },
                {"en", "You choose something that was not possible to choose" }
            });
            //wrong choice: (should be impossible)
            await Utils.SendMessage.SendMessageInPrivate(sender, e.Message.From.Id, e.Message.From.LanguageCode, e.Message.From.Username, text);
        }

        private static void Help(TelegramBotAbstract sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void FreeClassroom(TelegramBotAbstract sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void SearchClassroom(TelegramBotAbstract sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static async System.Threading.Tasks.Task OccupanciesOfTheDayAsync(TelegramBotAbstract sender, MessageEventArgs e)
        {
            int day;
            int month;
            int year;

            var datetime = await Utils.DateTimeClass.AskDateAsync(e.Message.From.Id, "Scegli un giorno", "it", sender, e.Message.From.Username);

            var d2 = datetime.GetDate();
            if (d2 == null)
            {
                return;
            }

            day = d2.Value.Day;
            month = d2.Value.Month;
            year = d2.Value.Year;

            string url = "https://www7.ceda.polimi.it/spazi/spazi/controller/OccupazioniGiornoEsatto.do?" +
          "csic=MIA" +
          "&categoria=tutte" +
          "&tipologia=tutte" +
          "&giorno_day=" + day.ToString() + 
          "&giorno_month=" + month.ToString() + 
          "&giorno_year=" + year.ToString() + 
          "&jaf_giorno_date_format=dd%2FMM%2Fyyyy&evn_visualizza=";

            WebReply html = await Utils.Web.DownloadHtmlAsync(url);
            if (html.IsValid() == false)
            {
                return;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html.GetData());

            ;

            var t1 = Utils.HtmlUtil.getElementsByTagAndClassName(doc.DocumentNode, "", "BoxInfoCard", 1);

            ;

            var t3 = Utils.HtmlUtil.getElementsByTagAndClassName(t1[0], "", "scrollContent", null);

            ;

            Language question = new Language(dict: new Dictionary<string, string>() {
                { "en", "Which room? (example: 3.0.1)"},
                {"it", "Quale aula? (esempio 3.0.1)" }
            });
            string roomName = await AskUser.AskAsync(idUser: e.Message.From.Id, question: question, sender: sender, lang:e.Message.From.LanguageCode,
                username: e.Message.From.Username, true);
            List<HtmlNode> t4 = GetRoomTitleAndHours(table: t3[0], roomName: roomName);

            

            ;


            if (t4 == null || t4.Count == 0)
            {
                return; //todo: send message error
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
                e.Message.From.Username, e.Message.From.LanguageCode);
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
            result.Add(table.ChildNodes[titleIndex.Value+1]);
            result.Add(table.ChildNodes[titleIndex.Value+2]);
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
                        if (child2.InnerHtml.Contains(roomName))
                            return i;
                    }
                }
            }

            return null;
        }
    }
}