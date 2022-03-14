#region

using System.Collections.Generic;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials
{
    internal static class KeyboardMarkup
    {
        internal static List<List<string>> ArrayToMatrixString(List<string> list)
        {
            if (list == null || list.Count == 0)
                return null;

            var r = new List<List<string>>();
            switch (list.Count)
            {
                case 1:
                {
                    var r2 = new List<string>
                    {
                        list[0]
                    };
                    r.Add(r2);
                    return r;
                }

                case 2:
                {
                    var r2 = new List<string>
                    {
                        list[0],
                        list[1]
                    };
                    r.Add(r2);
                    return r;
                }
/*
                case 3:
                    {
                        var r2 = new List<string>
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
                        var r2 = new List<string>
                    {
                        list[0],
                        list[1]
                    };
                        r.Add(r2);

                        var r3 = new List<string>
                    {
                        list[2],
                        list[3]
                    };
                        r.Add(r3);
                        return r;
                    }

                case 5:
                    {
                        var r2 = new List<string>
                    {
                        list[0],
                        list[1]
                    };
                        r.Add(r2);

                        var r3 = new List<string>
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
                        var r2 = new List<string>
                    {
                        list[0],
                        list[1],
                        list[2]
                    };
                        r.Add(r2);

                        var r3 = new List<string>
                    {
                        list[3],
                        list[4],
                        list[5]
                    };
                        r.Add(r3);
                        return r;
                    }
*/
            }

            var lines = list.Count / 2;
            if (list.Count % 2 != 0) lines++;

            for (var i = 0; i < lines; i++) r.Add(new List<string>());

            for (var i = 0; i < list.Count; i++) r[i / 2].Add(list[i]);

            return r;
        }
    }
}