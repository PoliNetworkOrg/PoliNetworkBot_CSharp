#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class DateTimeClass
    {
        internal static DateTime? GetUntilDate(string[] time)
        {
            if (time == null)
                return null;

            if (time.Length < 3) return null;

            if (string.IsNullOrEmpty(time[1]) || string.IsNullOrEmpty(time[2]))
                return null;

            int? number;
            try
            {
                number = Convert.ToInt32(time[1]);
            }
            catch
            {
                return null;
            }

            var how_may_seconds = GetHowManySeconds(time[2]);

            if (number == null || how_may_seconds == null)
                return null;

            var time_seconds_elapsed = number.Value * how_may_seconds.Value;

            return DateTime.Now.AddSeconds(time_seconds_elapsed);
        }

        private static int? GetHowManySeconds(string v)
        {
            if (string.IsNullOrEmpty(v))
                return null;

            v = v.ToLower();

            switch (v)
            {
                case "y":
                case "year":
                case "years":
                case "anni":
                case "anno":
                {
                    return 60 * 60 * 24 * 365;
                }

                case "stagioni":
                case "stagione":
                {
                    return 60 * 60 * 24 * 91;
                }

                case "mo":
                case "month":
                case "months":
                case "mesi":
                case "mese":
                {
                    return 60 * 60 * 24 * 30;
                }

                case "w":
                case "week":
                case "weeks":
                case "settimane":
                case "settimana":
                {
                    return 60 * 60 * 24 * 7;
                }

                case "d":
                case "day":
                case "days":
                case "giorni":
                case "giorno":
                {
                    return 60 * 60 * 24;
                }

                case "h":
                case "hour":
                case "hours":
                case "ore":
                case "ora":
                {
                    return 60 * 60;
                }

                case "m":
                case "min":
                case "mins":
                case "minute":
                case "minutes":
                case "minuti":
                case "minuto":
                {
                    return 60;
                }

                default:
                {
                    return 1;
                }
            }
        }

        internal static string NowAsStringAmericanFormat()
        {
            var dt = DateTime.Now;
            return string.Format("{0:s}", dt) + ":" + dt.Millisecond.ToString().PadLeft(3, '0');
        }

        internal static async Task<DateTime?> AskDateAsync(int id, string text, string lang, TelegramBotAbstract sender)
        {
            if (string.IsNullOrEmpty(text)) return await AskDate2Async(id, lang, sender);

            var s = text.Split(' ');
            if (s.Length == 1) return await AskDate2Async(id, lang, sender);

            switch (s[1])
            {
                case "ora":
                case "now":
                {
                    return DateTime.Now;
                }
            }

            return await AskDate2Async(id, lang, sender);
        }

        private static async Task<DateTime?> AskDate2Async(int id, string lang, TelegramBotAbstract sender)
        {
            var reply = await AskUser.AskAsync(id, new Dictionary<string, string>
                {
                    {"it", "Inserisci una data (puoi scrivere anche 'fra un'ora')"},
                    {"en", "Insert a date (you can also write 'in an hour'"}
                },
                sender, lang);

            var reply_Datetime = GetDateTimeFromString(reply);
            return reply_Datetime;
        }

        private static DateTime? GetDateTimeFromString(string reply)
        {
            if (string.IsNullOrEmpty(reply)) return null;
            reply = reply.ToLower();

            if (reply.StartsWith("in a"))
            {
                reply = reply.Substring(4).Trim();
                return GetDateTimeFromString2(reply);
            }

            if (reply.StartsWith("fra "))
            {
                reply = reply.Substring(4).Trim();
                return GetDateTimeFromString2(reply);
            }

            if (reply.StartsWith("entro "))
            {
                reply = reply.Substring(5).Trim();
                return GetDateTimeFromString2(reply);
            }

            if (reply.Contains("/"))
            {
                if (reply.Contains(":"))
                {
                    if (reply.Contains(" "))
                    {
                        var s = reply.Split(' ');
                        if (s[0].Contains("/"))
                        {
                            if (s[1].Contains(":"))
                            {
                                var s2 = s[0].Split('/');
                                var s3 = s[1].Split(':');

                                try
                                {
                                    var seconds = 0;
                                    if (s3.Length == 3) seconds = Convert.ToInt32(s3[2]);

                                    return new DateTime(Convert.ToInt32(s2[2]),
                                        Convert.ToInt32(s2[1]), Convert.ToInt32(s2[0]),
                                        Convert.ToInt32(s3[0]), Convert.ToInt32(s3[1]),
                                        seconds);
                                }
                                catch
                                {
                                    ;
                                }

                                ;
                            }
                            else
                            {
                                ;
                            }
                        }
                        else
                        {
                            ;
                        }
                    }
                    else
                    {
                        ;
                    }
                }
                else
                {
                    ;
                }
            }
            else
            {
                if (reply[0] >= '0' && reply[0] <= '9')
                {
                    return GetDateTimeFromString2(reply);
                }

                if (reply.StartsWith("un'"))
                {
                    reply = reply.Substring(3).Trim();
                    return GetDateTimeFromString2(reply);
                }

                ;
            }

            return null;
        }

        private static DateTime? GetDateTimeFromString2(string reply)
        {
            //entro "reply"
            if (string.IsNullOrEmpty(reply))
                return null;

            switch (reply)
            {
                case "un'ora":
                case "hour":
                case "ora":
                case "un ora":
                case "1 hour":
                case "1 ora":
                {
                    return DateTime.Now.AddHours(1);
                }

                case "un mese":
                case "month":
                case "1 mese":
                case "1 month":
                {
                    return DateTime.Now.AddMonths(1);
                }

                case "giorno":
                case "day":
                case "un giorno":
                case "1 giorno":
                case "1 day":
                {
                    return DateTime.Now.AddDays(1);
                }

                case "anno":
                case "year":
                case "un anno":
                case "1 anno":
                case "1 year":
                {
                    return DateTime.Now.AddYears(1);
                }
            }

            if (reply.StartsWith("un'"))
                reply = reply.Substring(3).Trim();
            else if (reply.StartsWith("un ")) reply = reply.Substring(3).Trim();

            var i = GetHowManySeconds(reply);
            if (i == null) return null;

            return DateTime.Now.AddSeconds(i.Value);
        }
    }
}