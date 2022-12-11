#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Exceptions;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils;
using PoliNetworkBot_CSharp.Code.Utils.Logger;
using PoliNetworkBot_CSharp.Code.Utils.UtilsMedia;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation;

internal static class Rooms
{
    internal static async Task RoomsMainAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        if (e?.Message != null && e.Message.Chat.Type != ChatType.Private)
            return;

        var question = new Language(new Dictionary<string, string?>
        {
            { "it", "Scegli:" },
            { "en", "Choose:" }
        });

        var options2 = new List<Language>
        {
            new(new Dictionary<string, string?>
            {
                { "it", "Cerca aule" },
                { "en", "Search classroom" }
            }),
            new(new Dictionary<string, string?>
            {
                { "it", "Aule libere" },
                { "en", "Free classroom" }
            }),
            new(new Dictionary<string, string?>
            {
                { "it", "Occupazioni del giorno" },
                { "en", "Occupancies of the day" }
            }),

            new(new Dictionary<string, string?>
            {
                { "it", "Aiuto" },
                { "en", "Help" }
            })
        };
        var o3 = KeyboardMarkup.ArrayToMatrixString(options2);

        var r = await AskUser.AskBetweenRangeAsync(e?.Message.From?.Id, question, lang: e?.Message.From?.LanguageCode,
            options: o3, username: e?.Message.From?.Username, sendMessageConfirmationChoice: true, sender: sender);

        var chosen = Language.FindChosen(options2, r, e?.Message.From?.LanguageCode);
        if (chosen == null)
            return;

        switch (chosen.Value)
        {
            case 0:
                await SearchClassroomAsync(sender, e);
                return;

            case 1:
                await FreeClassroomAsync(sender, e);
                return;

            case 2:
                await OccupanciesOfTheDayAsync(sender, e);
                return;

            case 3:
                await HelpAsync(sender, e);
                return;
        }

        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Hai compiuto una scelta che non era possibile compiere." },
            { "en", "You choose something that was not possible to choose" }
        });
        //wrong choice: (should be impossible)
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id, e?.Message.From?.LanguageCode,
            e?.Message.From?.Username, text,
            ParseMode.Html,
            null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static async Task HelpAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var text = new Language(new Dictionary<string, string?>
        {
            { "it", "Usa /rooms per cercare le aule!" },
            { "en", "Use /rooms to find rooms!" }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode, e?.Message.From?.Username, text,
            ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static async Task FreeClassroomAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var t3 = await GetDailySituationAsync(sender, e);
        if (t3 is not { Item1: null })
        {
            Logger.WriteLine(t3?.Item1);
            var text4 = new Language(new Dictionary<string, string?>
            {
                { "it", "Errore nella consultazione del sito del polimi!" },
                { "en", "Error while getting polimi website!" }
            });
            await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
                e?.Message.From?.LanguageCode,
                e?.Message.From?.Username,
                text4,
                ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
            return;
        }

        var (item1, item2) = await GetStartAndStopHoursAsync(sender, e) ?? new Tuple<DateTime?, DateTime?>(null, null);

        var t4 = GetFreeRooms(t3.Item2?[0], item1, item2);
        if (t4 == null || t4.Count == 0)
        {
            var text3 = new Language(new Dictionary<string, string?>
            {
                { "it", "Nessuna aula libera trovata!" },
                { "en", "No free rooms found!" }
            });
            await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
                e?.Message.From?.LanguageCode,
                e?.Message.From?.Username,
                text3,
                ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
            return;
        }

        var replyText = t4.Aggregate("", (current, room) => current + room + "\n");

        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", replyText }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode,
            e?.Message.From?.Username,
            text2,
            ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static async Task<Tuple<DateTime?, DateTime?>?> GetStartAndStopHoursAsync(TelegramBotAbstract? sender,
        MessageEventArgs? e)
    {
        var question = new Language(new Dictionary<string, string?>
        {
            { "it", "Ora di inizio? (esempio 8:15)" },
            { "en", "Start time? (example 8:15)" }
        });
        var start = await AskUser.AskHours(e?.Message.From?.Id, question,
            sender, e?.Message.From?.LanguageCode, e?.Message.From?.Username);

        var question2 = new Language(new Dictionary<string, string?>
        {
            { "it", "Ora di fine? (esempio 11:15)" },
            { "en", "End time? (example 11:15)" }
        });
        var end = await AskUser.AskHours(e?.Message.From?.Id, question2,
            sender, e?.Message.From?.LanguageCode, e?.Message.From?.Username);

        if (start != null && end != null)
            return new Tuple<DateTime?, DateTime?>(start.Value, end.Value);

        return null;
    }

    private static List<string?>? GetFreeRooms(HtmlNode? table, DateTime? start, DateTime? stop)
    {
        if (table?.ChildNodes == null)
            return null;

        var shiftStart = GetShiftSlotFromTime(start);
        var shiftEnd = GetShiftSlotFromTime(stop);

        return (from child in table.ChildNodes
            where child != null
            select CheckIfFree(child, shiftStart, shiftEnd)
            into toAdd
            where !string.IsNullOrEmpty(toAdd)
            select toAdd).ToList();
    }

    /// <summary>
    ///     Retrieves the number of quarters elapsed from 8:00, for easier counting as this is how
    ///     the columns on the Polimi page are spread
    /// </summary>
    private static int? GetShiftSlotFromTime(DateTime? time)
    {
        if (time == null) return null;
        var shiftSlot = (time.Value.Hour - 8) * 4;
        shiftSlot += time.Value.Minute / 15;
        return shiftSlot;
    }

    /// <summary>
    ///     Recunstruct a readable time string from the number of slots in the Polimi webpage table
    /// </summary>
    private static string TimeStringFromSlot(int slot)
    {
        var hours = (8 + slot / 4).ToString().PadLeft(2, '0');
        var minutes = (15 * (slot % 4)).ToString().PadLeft(2, '0');
        return $"{hours}:{minutes}";
    }

    /// <summary>
    ///     Checks if a room is empty given it's html node, and the start and end of the time period
    /// </summary>
    /// <returns>a string with the room name if the room is empty, null otherwise</returns>
    private static string? CheckIfFree(HtmlNode? node, int? shiftStart, int? shiftEnd)
    {
        if (node != null && !node.GetClasses().Contains("normalRow")) return null;
        if (node?.ChildNodes == null) return null;

        if (!node.ChildNodes.Any(x =>
                x.HasClass("dove")
                && x.ChildNodes != null
                && x.ChildNodes.Any(x2 => x2.Name == "a" && !x2.InnerText.ToUpper().Contains("PROVA"))
            ))
            return null;

        var roomFree = IsRoomFree(node, shiftStart, shiftEnd);
        return roomFree ? GetNomeAula(node) : null;
    }

    /// <summary>
    ///     retrieves the room name from the node
    /// </summary>
    private static string? GetNomeAula(HtmlNode? node)
    {
        var dove = node?.ChildNodes.First(x => x.HasClass("dove"));
        var a = dove?.ChildNodes.First(x => x.Name == "a");
        return a?.InnerText.Trim();
    }

    /// <summary>
    ///     calculates if a room is free in the given time window
    /// </summary>
    private static bool IsRoomFree(HtmlNode? node, int? shiftStart, int? shiftEnd)
    {
        if (node?.ChildNodes == null)
            return true;

        var colsizetotal = 0;
        // the first two children are not time slots
        for (var i = 2; i < node.ChildNodes.Count; i++)
        {
            int colsize;
            // for each column, take it's span as the colsize
            if (node.ChildNodes[i].Attributes.Contains("colspan"))
                colsize = (int)Convert.ToInt64(node.ChildNodes[i].Attributes["colspan"].Value);
            else
                colsize = 1;

            // the time start in shifts for each column, is the previous total
            var vStart = colsizetotal;
            colsizetotal += colsize;
            var vEnd = colsizetotal; // the end is the new total (prev + colsize)

            // this is the trickery, if any column ends before the shift start or starts before
            // the shift end, then we skip
            if (vEnd < shiftStart || vStart > shiftEnd)
                continue;

            // if one of the not-skipped column represents an actual lesson, then return false,
            // the room is occupied
            if (!string.IsNullOrEmpty(node.ChildNodes[i].InnerHtml.Trim()))
                return false;
        }

        // if no lesson takes place in the room in the time window, the room is free (duh)
        return true;
    }

    private static async Task SearchClassroomAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var question = new Language(new Dictionary<string, string?>
        {
            { "it", "Nome dell'aula?" },
            { "en", "Name of the room?" }
        });
        var sigla = await AskUser.AskAsync(e?.Message.From?.Id, question, sender,
            e?.Message.From?.LanguageCode, e?.Message.From?.Username);

        var url = "https://www7.ceda.polimi.it/spazi/spazi/controller/RicercaAula.do?spazi___model" +
                  "___formbean___RicercaAvanzataAuleVO___postBack=true&spazi___model___formbean___" +
                  "RicercaAvanzataAuleVO___formMode=FILTER&evn_ricerca_avanzata=&spazi___model___formbean___" +
                  "RicercaAvanzataAuleVO___sede=tutte&spazi___model___formbean___RicercaAvanzataAuleVO___sigla=" +
                  sigla + "&spazi___model___formbean___RicercaAvanzataAuleVO___categoriaScelta=tutte&spazi" +
                  "___model___formbean___RicercaAvanzataAuleVO___tipologiaScelta=tutte&spazi___model" +
                  "___formbean___RicercaAvanzataAuleVO___iddipScelto=tutti&spazi___model___formbean___" +
                  "RicercaAvanzataAuleVO___soloPreseElettriche_default=N&spazi___model___formbean___" +
                  "RicercaAvanzataAuleVO___soloPreseDiRete_default=N";

        var webReply = await Web.DownloadHtmlAsync(url);
        if (!webReply.IsValid())
        {
            await DownloadFailedAsync(sender, e);
            return;
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(webReply.GetData());

        var t1 = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "", "TableDati-tbody");

        var t2 = t1?[0];

        var t3 = HtmlUtil.GetElementsByTagAndClassName(t2, "tr");

        var roomIndex = FindRoomIndex(t3, sigla);
        if (roomIndex == null)
        {
            await RoomNotFoundAsync(sender, e);
            return;
        }

        var t4 = t3?[(int)roomIndex.Value];

        var t5 = HtmlUtil.GetElementsByTagAndClassName(t4, "td");

        if (t5 is { Count: < 3 })
        {
            await RoomNotFoundAsync(sender, e);
            return;
        }

        var t6 = t5?[2];

        var t7 = HtmlUtil.GetElementsByTagAndClassName(t6, "a");

        if (t7 is { Count: < 1 })
        {
            await RoomNotFoundAsync(sender, e);
            return;
        }

        var t8 = t7?[0];

        var t9 = t8?.Attributes;

        var t10 = t9?["href"];

        if (t10 == null)
        {
            await RoomNotFoundAsync(sender, e);
            return;
        }

        if (string.IsNullOrEmpty(t10.Value))
        {
            await RoomNotFoundAsync(sender, e);
            return;
        }

        var result = "https://www7.ceda.polimi.it/spazi/spazi/controller/" + t10.Value;
        var text2 = new Language(new Dictionary<string, string?>
        {
            { "en", result }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode, e?.Message.From?.Username,
            text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static async Task RoomNotFoundAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        Language text2 = new(new Dictionary<string, string?>
        {
            {
                "it", "Aula non trovata."
            },
            {
                "en", "Room not found."
            }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode, e?.Message.From?.Username,
            text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static async Task DownloadFailedAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        Language text2 = new(new Dictionary<string, string?>
        {
            {
                "it", "Aula non trovata."
            },
            {
                "en", "Room not found."
            }
        });
        await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
            e?.Message.From?.LanguageCode, e?.Message.From?.Username,
            text2, ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
    }

    private static long? FindRoomIndex(IReadOnlyList<HtmlNode?>? t3, string? sigla)
    {
        if (t3 == null || t3.Count == 0)
            return null;

        for (var i = 0; i < t3.Count; i++)
        {
            var t4 = t3[i];
            if (t4?.ChildNodes == null || t4.ChildNodes.Count < 2)
                continue;

            var t6 = HtmlUtil.GetElementsByTagAndClassName(t4, "td");

            if (t6 == null || t6.Count < 2)
                return null;

            var t7 = t6[1];

            var t8 = HtmlUtil.GetElementsByTagAndClassName(t7, "b");

            var t9 = t8?[0]?.InnerHtml.Trim();

            if (t9 == sigla)
                return i;

            var found = StringUtil.CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(sigla, t8?[0]);
            if (found != null && found.Value)
                return i;
        }

        return null;
    }

    /// <summary>
    ///     This method represents the Occupancies of The Day button within the /rooms command,
    ///     searches on the website a specific room and retrieves when said room is free to use or
    ///     is occupied
    /// </summary>
    private static async Task OccupanciesOfTheDayAsync(TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        // Ask the user fot the date (which we'll need later)
        var tuple1 = await AskUser.AskDateAsync(e?.Message.From?.Id,
            "Scegli un giorno", "it", sender,
            e?.Message.From?.Username);
        if (tuple1 != null)
        {
            var dateTimeSchedule = tuple1.Item1;
            var exception = tuple1.Item2;
            if (exception != null) throw exception;
            if (dateTimeSchedule != null)
            {
                var d2 = dateTimeSchedule.GetDate();
                if (d2 == null)
                {
                    var text2 = new Language(new Dictionary<string, string?>
                    {
                        { "it", "La data inserita non è valida!" },
                        { "en", "This date is not valid!" }
                    });
                    await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
                        e?.Message.From?.LanguageCode,
                        e?.Message.From?.Username,
                        text2,
                        ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
                    return;
                }

                // retrieves the table for the day
                var t3 = await GetDailySituationOnDate(sender, e, d2.Value);
                if (t3 == null)
                {
                    var text2 = new Language(new Dictionary<string, string?>
                    {
                        { "it", "Errore nella consultazione del sito del polimi!" },
                        { "en", "Error while getting polimi website!" }
                    });
                    await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
                        e?.Message.From?.LanguageCode,
                        e?.Message.From?.Username,
                        text2,
                        ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
                    return;
                }

                // finds within the table, the row for a specific room
                var question = new Language(new Dictionary<string, string?>
                {
                    { "en", "Which room? (example: 3.0.1)" },
                    { "it", "Quale aula? (esempio 3.0.1)" }
                });
                var roomName = await AskUser.AskAsync(e?.Message.From?.Id, question, sender,
                    e?.Message.From?.LanguageCode,
                    e?.Message.From?.Username, true);
                var t4 = GetRoomTitleAndHours(t3[0], roomName);

                if (t4 == null || t4.Count == 0)
                {
                    var text2 = new Language(new Dictionary<string, string?>
                    {
                        { "it", "Aula non trovata!" },
                        { "en", "Room not found!" }
                    });
                    await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
                        e?.Message.From?.LanguageCode,
                        e?.Message.From?.Username,
                        text2,
                        ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));
                    return;
                }

                var occupationRow = t4[3];
                var slot = GetShiftSlotFromTime(d2.Value);

                // Compose the message to send, start with room name and date:
                var text = $"L'aula <b>{roomName}</b> il <b>{d2.Value:dd/MM/yyyy}</b>";
                var textEng = $"The room <b>{roomName}</b> on <b>{d2.Value:dd/MM/yyyy}</b>";

#pragma warning disable CS8794 // L'input corrisponde sempre al criterio specificato.
                if (d2.Value.Hour is >= 8 or < 20)
                {
                    // if we are in a period between open hours, say more specific things
                    // add time (time is not important if we just consider the whole day)
                    text += $" alle <b>{d2.Value:HH:mm}</b> ";
                    textEng += $" at <b>{d2.Value:HH:mm}</b> ";

                    // say if it's free or not
                    var isFree = IsRoomFree(occupationRow, slot, slot + 1);
                    text += $" è <b>{(isFree ? "libera" : "occupata")}</b>";
                    textEng += $" is <b>{(isFree ? "free" : "occupied")}</b>";

                    // and add until when if the information is available
                    var nextTransition = GetFirstSlotTransition(occupationRow, slot);

                    if (!string.IsNullOrEmpty(nextTransition))
                    {
                        text += $"\ne lo rimarrà fino alle {nextTransition}";
                        textEng += $"\nand will be until {nextTransition}";
                    }

                    text += "\n\nQuest'aula";
                    textEng += "\n\nThis room";
                }
#pragma warning restore CS8794 // L'input corrisponde sempre al criterio specificato.

                // add a list with all the free slots
                var freeSlots = GetFreeTimeSlots(occupationRow);
                text += " è libera nelle seguenti fasce orarie:\n";
                textEng += " is free in the following time slots:\n";

                foreach (var (item1, item2) in freeSlots)
                {
                    text += $"• Dalle <b>{item1}</b> alle <b>{item2}</b>\n";
                    textEng += $"• From <b>{item1}</b> to <b>{item2}</b>\n";
                }

                text += "\nQua sotto trovi la tabella completa delle occupazioni dell'aula per questa giornata";
                textEng += "\nBelow you'll find the complete table with all occupations of this room for this day";

                var message = new Language(new Dictionary<string, string?>
                {
                    { "it", text },
                    { "en", textEng }
                });

                await SendMessage.SendMessageInPrivate(sender, e?.Message.From?.Id,
                    e?.Message.From?.LanguageCode,
                    e?.Message.From?.Username,
                    message,
                    ParseMode.Html, null, InlineKeyboardMarkup.Empty(), EventArgsContainer.Get(e));

                // send the table as an html document for further info
                var htmlResult = t4.Aggregate(
                    "<html><head><style>td {border: #333 1px solid;} td.slot {background: #cce6ff;}</style></head><body><table>",
                    (current, t5) => current + t5.OuterHtml);
                htmlResult += "</table></body></html>";

                var peer = new PeerAbstract(e?.Message.From?.Id, ChatType.Private);
                message = new Language(new Dictionary<string, string?>
                {
                    { "en", roomName }
                });
                var document = UtilsFileText.GenerateFileFromString(htmlResult, roomName + ".html",
                    message, TextAsCaption.AS_CAPTION, "text/html");

                sender?.SendFileAsync(document,
                    peer, e?.Message.From?.Username,
                    e?.Message.From?.LanguageCode, null, true);
            }
        }
    }

    /// <summary>
    ///     Returns the time string of the first transition (from free to occupied or vice versa) of
    ///     the occupancy state for a given room, if no transition can be found it returns null
    /// </summary>
    /// <param name="node">The HTML row for the room</param>
    /// <param name="startSlot">the start time as a time slot</param>
    /// <returns>a string with the time if a transition is found, null otherwise</returns>
    private static string? GetFirstSlotTransition(HtmlNode? node, int? startSlot)
    {
        var colsizetotal = 1;
        var isCurrentlyFree = false;
        var afterStartSlot = false;

        // start from 8:15, the third child
        if (node == null)
            return null;

        for (var i = 3; i < node.ChildNodes.Count; i++)
        {
            var colsize = 1;
            if (node.ChildNodes[i].Attributes.Contains("colspan"))
                colsize = (int)Convert.ToInt64(node.ChildNodes[i].Attributes["colspan"].Value);

            if (string.IsNullOrEmpty(node.ChildNodes[i].InnerHtml.Trim()) == !isCurrentlyFree)
            {
                // check for a transition, from free to occupied or vice versa
                isCurrentlyFree = !isCurrentlyFree;
                // only return if we are after the starting time slot
                if (afterStartSlot)
                    return TimeStringFromSlot(colsizetotal);
            }

            colsizetotal += colsize; // keep track of the columns

            // quit searching after 19, dont want to consider a transition this late
            if (colsizetotal >= 44) break;
            if (colsizetotal >= startSlot) afterStartSlot = true;
        }

        // if there is no transition after the startSlot, just return null
        return null;
    }

    /// <summary>
    ///     Get a list of tuples with the start and end time of each free slot given the HTML row of
    ///     a room
    /// </summary>
    /// <param name="node">The HTML row for the room</param>
    /// <returns>a list of tuples with starting and ending time strings for each slot</returns>
    private static List<(string?, string?)> GetFreeTimeSlots(HtmlNode? node)
    {
        var list = new List<(string?, string?)>();

        var colsizetotal = 1;
        var isCurrentlyFree = false;
        var currentSlotStart = 1;

        // start from 8:15, the third child
        if (node != null)
            for (var i = 3; i < node.ChildNodes.Count; i++)
            {
                var colsize = 1;
                if (node.ChildNodes[i].Attributes.Contains("colspan"))
                    colsize = (int)Convert.ToInt64(node.ChildNodes[i].Attributes["colspan"].Value);

                if (string.IsNullOrEmpty(node.ChildNodes[i].InnerHtml.Trim())) // if the child is an empty slot
                {
                    if (!isCurrentlyFree)
                    {
                        // and if we come from a non-empty slot, set the start of this window
                        isCurrentlyFree = true;
                        currentSlotStart = colsizetotal;
                    }
                }
                else if (isCurrentlyFree)
                {
                    // else we close this free slot, appending it to the list
                    isCurrentlyFree = false;
                    var s1 = TimeStringFromSlot(currentSlotStart);
                    var s2 = TimeStringFromSlot(colsizetotal);
                    var s3 = (s1, s2);
                    list.Add(s3);
                }

                colsizetotal += colsize; // keep track of the columns
            }

        if (isCurrentlyFree && currentSlotStart < 44)
            // add one last element if the last thing ends before 19:00
            list.Add((TimeStringFromSlot(currentSlotStart), "20:00"));

        return list;
    }

    /// <summary>
    ///     Asks the user for date and site, returns the list of html nodes from the
    ///     "OccupazioniGiornoEsatto" page and an exception (if generated by user interaction)
    /// </summary>
    private static async Task<Tuple<ExceptionNumbered?, List<HtmlNode?>?>?> GetDailySituationAsync(
        TelegramBotAbstract? sender, MessageEventArgs? e)
    {
        var tuple = await AskUser.AskDateAsync(e?.Message.From?.Id,
            "Scegli un giorno", "it", sender,
            e?.Message.From?.Username);

        var exception = tuple?.Item2;
        if (exception != null)
            return new Tuple<ExceptionNumbered?, List<HtmlNode?>?>(new ExceptionNumbered(exception), null);

        var dateTimeSchedule = tuple?.Item1;
        var d2 = dateTimeSchedule?.GetDate();
        if (d2 == null) return null;

        var d = await GetDailySituationOnDate(sender, e, d2.Value);
        return new Tuple<ExceptionNumbered?, List<HtmlNode?>?>(null, d);
    }

    /// <summary>
    ///     Given a date, asks the user for the site and returns the html table from the
    ///     "OccupazioniGiornoEsatto" page
    /// </summary>
    private static async Task<List<HtmlNode?>?> GetDailySituationOnDate(TelegramBotAbstract? sender,
        MessageEventArgs? e, DateTime date)
    {
        var day = date.Day;
        var month = date.Month;
        var year = date.Year;

        var sede = await AskUser.GetSedeAsync(sender, e);
        if (string.IsNullOrEmpty(sede)) return null;

        var url = "https://www7.ceda.polimi.it/spazi/spazi/controller/OccupazioniGiornoEsatto.do?" +
                  "csic=" + sede +
                  "&categoria=tutte" +
                  "&tipologia=tutte" +
                  "&giorno_day=" + day +
                  "&giorno_month=" + month +
                  "&giorno_year=" + year +
                  "&jaf_giorno_date_format=dd%2FMM%2Fyyyy&evn_visualizza=";

        var html = await Web.DownloadHtmlAsync(url);
        if (html.IsValid() == false) return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html.GetData());

        var t1 = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "", "BoxInfoCard", 1);

        var t3 = HtmlUtil.GetElementsByTagAndClassName(t1?[0], "", "scrollContent");
        return t3;
    }

    private static List<HtmlNode>? GetRoomTitleAndHours(HtmlNode? table, string? roomName)
    {
        if (table == null)
            return null;

        if (string.IsNullOrEmpty(roomName))
            return null;

        var result = new List<HtmlNode>();
        var roomIndex = (int?)FindRoom(table, roomName);
        if (roomIndex == null)
            return null;

        var titleIndex = (int?)FindTitleIndex(table, roomIndex.Value);
        if (titleIndex == null)
            return null;

        result.Add(table.ChildNodes[titleIndex.Value]);
        result.Add(table.ChildNodes[titleIndex.Value + 1]);
        result.Add(table.ChildNodes[titleIndex.Value + 2]);
        result.Add(table.ChildNodes[roomIndex.Value]);

        return result;
    }

    private static long? FindTitleIndex(HtmlNode? table, long roomIndex)
    {
        for (var i = (int)roomIndex; i >= 0; i--)
        {
            var child = table?.ChildNodes[i];
            if (child != null && child.GetClasses().Contains("normalRow") == false) return i - 2;
        }

        return null;
    }

    private static long? FindRoom(HtmlNode? table, string? roomName)
    {
        if (table == null) return null;
        for (var i = 0; i < table.ChildNodes.Count; i++)
        {
            var child = table.ChildNodes[i];

            if (child.ChildNodes == null || !child.GetClasses().Contains("normalRow")) continue;
            if (child.ChildNodes
                .Select(child2 => StringUtil.CheckIfTheStringIsTheSameAndValidRoomNameInsideAText(roomName, child2))
                .Any(found => found != null && found.Value)) return i;
        }

        return null;
    }
}