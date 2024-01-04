using System;
using System.Collections.Generic;
using System.Linq;
using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Model;
using PoliNetworkBot_CSharp.Code.Data.Variables;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class Stats
{
    public static int GetCountStored()
    {
        var s = 0;
        GlobalVariables.Threads ??= new MessageThreadStore();
        GlobalVariables.Threads.Dict ??= new Dictionary<DateTime, List<MessageThread>>();
        lock (GlobalVariables.Threads)
        {
            s += GlobalVariables.Threads.Dict.Keys.Sum(variable => GlobalVariables.Threads.Dict[variable].Count);
        }

        return s;
    }
}