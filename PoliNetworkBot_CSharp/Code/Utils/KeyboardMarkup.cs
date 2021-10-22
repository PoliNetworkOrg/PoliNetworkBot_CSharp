﻿#region

using PoliNetworkBot_CSharp.Code.Objects;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal static class KeyboardMarkup
    {
        public static List<List<KeyboardButton>> OptionsStringToKeyboard(IEnumerable<List<Language>> options,
            string lang)
        {
            return options.Select(o => o.Select(
                o2 =>
                {
                    var o3 = o2.Select(lang);
                    return new KeyboardButton(o3);
                }
            ).ToList()).ToList();
        }

        internal static List<List<Language>> ArrayToMatrixString(List<Language> list)
        {
            if (list == null || list.Count == 0)
                return null;

            var r = new List<List<Language>>();
            switch (list.Count)
            {
                case 1:
                    {
                        var r2 = new List<Language>
                    {
                        list[0]
                    };
                        r.Add(r2);
                        return r;
                    }

                case 2:
                    {
                        var r2 = new List<Language>
                    {
                        list[0],
                        list[1]
                    };
                        r.Add(r2);
                        return r;
                    }

                case 3:
                    {
                        var r2 = new List<Language>
                    {
                        list[0],
                        list[1],
                        list[2]
                    };
                        r.Add(r2);
                        return r;
                    }

                case 4:
                    {
                        var r2 = new List<Language>
                    {
                        list[0],
                        list[1]
                    };
                        r.Add(r2);

                        var r3 = new List<Language>
                    {
                        list[2],
                        list[3]
                    };
                        r.Add(r3);
                        return r;
                    }

                case 5:
                    {
                        var r2 = new List<Language>
                    {
                        list[0],
                        list[1]
                    };
                        r.Add(r2);

                        var r3 = new List<Language>
                    {
                        list[2],
                        list[3],
                        list[4]
                    };
                        r.Add(r3);
                        return r;
                    }

                case 6:
                    {
                        var r2 = new List<Language>
                    {
                        list[0],
                        list[1],
                        list[2]
                    };
                        r.Add(r2);

                        var r3 = new List<Language>
                    {
                        list[3],
                        list[4],
                        list[5]
                    };
                        r.Add(r3);
                        return r;
                    }
            }

            var lines = list.Count / 3;
            if (list.Count % 3 != 0) lines++;

            for (var i = 0; i < lines; i++) r.Add(new List<Language>());

            for (var i = 0; i < list.Count; i++) r[i / 3].Add(list[i]);

            return r;
        }
    }
}