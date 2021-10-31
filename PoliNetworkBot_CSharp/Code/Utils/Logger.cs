using System;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public static class Logger
    {
        public static void WriteLine(object log)
        {
            try
            {
                Console.WriteLine(log);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}