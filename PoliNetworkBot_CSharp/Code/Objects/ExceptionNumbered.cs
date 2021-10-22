using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class ExceptionNumbered : Exception
    {
        private const int default_v = 1;
        private int v;

        public ExceptionNumbered(Exception item1, int v = default_v) : base(item1.Message, item1)
        {
            this.v = v;
        }

        public ExceptionNumbered(string message, int v = default_v) : base(message)
        {
            this.v = v;
        }

        internal void Increment()
        {
            v++;
        }

        internal Exception GetException()
        {
            return this;
        }

        internal bool AreTheySimilar(Exception item2)
        {
            if (Message == item2.Message)
                return true;

            return false;
        }

        internal int GetNumberOfTimes()
        {
            return v;
        }

        internal static async Task<bool> SendExceptionAsync(Exception e, TelegramBotAbstract telegramBotAbstract)
        {
            if (telegramBotAbstract == null)
                return false;

            await NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            return true;
        }
    }
}