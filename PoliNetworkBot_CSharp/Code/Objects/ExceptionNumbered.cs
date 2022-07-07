#region

using System;
using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Utils;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects;

public class ExceptionNumbered : Exception
{
    private const int DefaultV = 1;
    private int _v;

    public ExceptionNumbered(Exception? item1, int v = DefaultV) : base(item1?.Message, item1)
    {
        _v = v;
    }

    public ExceptionNumbered(string message, int v = DefaultV) : base(message)
    {
        _v = v;
    }

    internal void Increment()
    {
        _v++;
    }

    internal Exception GetException()
    {
        return this;
    }

    internal bool AreTheySimilar(Exception? item2)
    {
        return Message == item2?.Message;
    }

    internal int GetNumberOfTimes()
    {
        return _v;
    }

    internal static async Task<bool> SendExceptionAsync(Exception? e, TelegramBotAbstract? telegramBotAbstract,
        MessageEventArgs? messageEventArgs)
    {
        if (telegramBotAbstract == null)
            return false;

        await NotifyUtil.NotifyOwners(e, telegramBotAbstract, messageEventArgs);
        return true;
    }
}