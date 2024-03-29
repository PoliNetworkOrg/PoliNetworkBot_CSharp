﻿#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.Objects;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal static class DateTimeClass
{
    internal static DateTime? GetUntilDate(string?[]? time)
    {
        if (time == null)
            return null;

        if (time.Length < 3) return null;

        if (string.IsNullOrEmpty(time[1]) || string.IsNullOrEmpty(time[2]))
            return null;

        long? number;
        try
        {
            number = Convert.ToInt64(time[1]);
        }
        catch
        {
            return null;
        }

        var howMaySeconds = GetHowManySeconds(time[2]);

        if (howMaySeconds == null)
            return null;

        var timeSecondsElapsed = number.Value * howMaySeconds.Value;

        return DateTime.Now.AddSeconds(timeSecondsElapsed);
    }

    private static long? GetHowManySeconds(string? v)
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
        return $"{dt:s}" + ":" + dt.Millisecond.ToString().PadLeft(3, '0');
    }

    public static Tuple<DateTime?, Exception?>? GetDateTimeFromString(string? reply)
    {
        if (string.IsNullOrEmpty(reply))
            return null;

        reply = reply.ToLower();

        switch (reply)
        {
            case "now" or "ora" or "oggi" or "today":
                return new Tuple<DateTime?, Exception?>(DateTime.Now, null);

            case "domani" or "tomorrow":
                return new Tuple<DateTime?, Exception?>(DateTime.Now.AddDays(1), null);

            case "dopodomani" or "in two days" or "in 2 days":
                return new Tuple<DateTime?, Exception?>(DateTime.Now.AddDays(2), null);
        }

        if (reply.StartsWith("in a"))
        {
            reply = reply[4..].Trim();
            return GetDateTimeFromString2(reply);
        }

        if (reply.StartsWith("fra "))
        {
            reply = reply[4..].Trim();
            return GetDateTimeFromString2(reply);
        }

        if (reply.StartsWith("entro "))
        {
            reply = reply[5..].Trim();
            var d2 = GetDateTimeFromString2(reply);
            return d2;
        }

        if (reply.Contains('/'))
        {
            if (!reply.Contains(':'))
                return null;

            if (!reply.Contains(' '))
                return null;

            var s = reply.Split(' ');
            if (!s[0].Contains('/'))
                return null;

            if (!s[1].Contains(':'))
                return null;

            var s2 = s[0].Split('/');
            var s3 = s[1].Split(':');

            try
            {
                long seconds = 0;
                if (s3.Length == 3) seconds = Convert.ToInt64(s3[2]);

                var d1 = new DateTime((int)Convert.ToInt64(s2[2]),
                    (int)Convert.ToInt64(s2[1]),
                    (int)Convert.ToInt64(s2[0]),
                    (int)Convert.ToInt64(s3[0]),
                    (int)Convert.ToInt64(s3[1]),
                    (int)seconds);

                return new Tuple<DateTime?, Exception?>(d1, null);
            }
            catch
            {
                // ignored
            }
        }
        else if (reply.Contains('-'))
        {
            if (reply.Contains(' '))
                try
                {
                    var x1 = reply.Split(" ");
                    var x2 = x1[0];
                    var x = x2.Split("-");
                    var orario = x1[1];
                    var orario2 = orario.Split(":");
                    var d3 = new DateTime((int)Convert.ToInt64(x[0]),
                        (int)Convert.ToInt64(x[1]),
                        (int)Convert.ToInt64(x[2]),
                        (int)Convert.ToInt64(orario2[0]),
                        (int)Convert.ToInt64(orario2[1]),
                        0);

                    return new Tuple<DateTime?, Exception?>(d3, null);
                }
                catch (Exception e2)
                {
                    return new Tuple<DateTime?, Exception?>(null, e2);
                }

            {
                var x = reply.Split('-');
                return new Tuple<DateTime?, Exception?>(
                    new DateTime(
                        (int)Convert.ToInt64(x[0]),
                        (int)Convert.ToInt64(x[1]),
                        (int)Convert.ToInt64(x[2])),
                    null
                );
            }
        }
        else if (reply.Contains(':')) // 10:34
        {
            var s2 = reply.Split(":");
            var dt = new DateTime(2000, 1, 1, Convert.ToInt32(s2[0]), Convert.ToInt32(s2[1]), 0);
            return new Tuple<DateTime?, Exception?>(dt, null);
        }
        else
        {
            if (reply[0] >= '0' && reply[0] <= '9')
                return GetDateTimeFromString2(reply);

            if (!reply.StartsWith("un'"))
                return null;

            reply = reply[3..].Trim();
            return GetDateTimeFromString2(reply);
        }

        return null;
    }

    internal static DateTime? GetHours(string? s)
    {
        var dt = GetDateTimeFromString(s);
        return dt?.Item1;
    }

    internal static string? DateTimeToAmericanFormat(DateTime? dt)
    {
        if (dt == null)
            return null;

        return dt.Value.Year.ToString().PadLeft(4, '0') + "-" +
               dt.Value.Month.ToString().PadLeft(2, '0') + "-" +
               dt.Value.Day.ToString().PadLeft(2, '0') + " " +
               dt.Value.Hour.ToString().PadLeft(2, '0') + ":" +
               dt.Value.Minute.ToString().PadLeft(2, '0') + ":" +
               dt.Value.Second.ToString().PadLeft(2, '0') + "." +
               dt.Value.Millisecond.ToString().PadLeft(3, '0');
    }

    private static Tuple<DateTime?, Exception?>? GetDateTimeFromString2(string? reply)
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
                return new Tuple<DateTime?, Exception?>(DateTime.Now.AddHours(1), null);
            }

            case "un mese":
            case "month":
            case "1 mese":
            case "1 month":
            {
                return new Tuple<DateTime?, Exception?>(DateTime.Now.AddMonths(1), null);
            }

            case "giorno":
            case "day":
            case "un giorno":
            case "1 giorno":
            case "1 day":
            {
                return new Tuple<DateTime?, Exception?>(DateTime.Now.AddDays(1), null);
            }

            case "anno":
            case "year":
            case "un anno":
            case "1 anno":
            case "1 year":
            {
                return new Tuple<DateTime?, Exception?>(DateTime.Now.AddYears(1), null);
            }
        }

        if (reply.StartsWith("un'"))
            reply = reply[3..].Trim();
        else if (reply.StartsWith("un ")) reply = reply[3..].Trim();

        if (reply.Contains('-')) return GetDateTimeFromString(reply);

        var i = GetHowManySeconds(reply);
        return i == null ? null : new Tuple<DateTime?, Exception?>(DateTime.Now.AddSeconds(i.Value), null);
    }

    internal static ValueWithException<DateTime?> GetFromString(string? v)
    {
        try
        {
            if (v != null && v.Contains(' '))
            {
                var v2 = v.Split(' ');
                //v2[0] data, v2[1] ora

                if (v2[0].Contains('-'))
                {
                    var data = v2[0].Split('-');
                    if (v2[1].Contains('.'))
                    {
                        var v4 = v2[1].Split('.');
                        if (v4[0].Contains(':'))
                        {
                            var orario = v4[0].Split(':');
                            var d2 = new DateTime((int)Convert.ToInt64(data[0]), (int)Convert.ToInt64(data[1]),
                                (int)Convert.ToInt64(data[2]), (int)Convert.ToInt64(orario[0]),
                                (int)Convert.ToInt64(orario[1]),
                                (int)Convert.ToInt64(orario[2]), (int)Convert.ToInt64(v4[1].Trim()[..3]));
                            return new ValueWithException<DateTime?>(d2, null);
                        }
                    }
                }
            }
            else if (v != null && v.Contains('-'))
            {
                var data = v.Split('-');
                var d1 = new DateTime((int)Convert.ToInt64(data[0]), (int)Convert.ToInt64(data[1]),
                    (int)Convert.ToInt64(data[2]));
                return new ValueWithException<DateTime?>(d1, null);
            }
        }
        catch (Exception? ex)
        {
            return new ValueWithException<DateTime?>(null, ex);
        }

        return new ValueWithException<DateTime?>(null, new NotImplementedException());
    }


    public static ValueWithException<DateTime?>? GetDateTime(IReadOnlyList<string?>? target)
    {
        if (target == null)
            return null;
        if (target.Count < 3)
            return null;

        var s = "";
        for (var i = 1; i < target.Count; i++) s += target[i] + " ";

        s = s.Trim();
        return GetFromString(s);
    }
}