using System.Collections.Generic;

namespace PoliNetworkBot_CSharp.Utils
{
    internal class KeyboardMarkup
    {
        internal static List<List<string>> ArrayToMatrixString(List<string> list)
        {
            if (list == null || list.Count == 0)
                return null;

            List<List<string>> r = new List<List<string>>();
            switch (list.Count)
            {
                case 1:
                    {
                        List<string> r2 = new List<string>
                        {
                            list[0]
                        };
                        r.Add(r2);
                        return r;
                    }

                case 2:
                    {
                        List<string> r2 = new List<string>
                        {
                            list[0],
                            list[1]
                        };
                        r.Add(r2);
                        return r;
                    }

                case 3:
                    {
                        List<string> r2 = new List<string>
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
                        List<string> r2 = new List<string>
                        {
                            list[0],
                            list[1]
                        };
                        r.Add(r2);

                        List<string> r3 = new List<string>
                        {
                            list[2],
                            list[3]
                        };
                        r.Add(r3);
                        return r;
                    }

                case 5:
                    {
                        List<string> r2 = new List<string>
                        {
                            list[0],
                            list[1]
                        };
                        r.Add(r2);

                        List<string> r3 = new List<string>
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
                        List<string> r2 = new List<string>
                        {
                            list[0],
                            list[1],
                            list[2]
                        };
                        r.Add(r2);

                        List<string> r3 = new List<string>
                        {
                            list[3],
                            list[4],
                            list[5]
                        };
                        r.Add(r3);
                        return r;
                    }
            }

            int lines = list.Count / 3;
            if (list.Count % 3 != 0)
            {
                lines++;
            }

            for (int i = 0; i < lines; i++)
            {
                r.Add(new List<string>());
            }

            for (int i = 0; i < list.Count; i++)
            {
                r[i / 3].Add(list[i]);
            }

            return r;
        }
    }
}