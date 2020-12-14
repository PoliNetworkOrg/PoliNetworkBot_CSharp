using PoliNetworkBot_CSharp.Code.Objects;
using System;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class ExceptionNumbered : Exception
    {
        private int v;

        private const int default_v = 1;

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
            if (this.Message == item2.Message)
                return true;

            return false;
        }

        internal int GetNumberOfTimes()
        {
            return v;
        }

        internal static async System.Threading.Tasks.Task<bool> SendExceptionAsync(Exception e, TelegramBotAbstract telegramBotAbstract)
        {
            if (telegramBotAbstract == null)
                return false;

            await Utils.NotifyUtil.NotifyOwners(e, telegramBotAbstract);
            return true;
        }
    }
}