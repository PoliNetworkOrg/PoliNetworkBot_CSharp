using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class ScriptUtil
{
    public static List<string?> DoScript(PowerShell powershell, string script, bool debug)
    {
        powershell.AddScript(script);
        List<string?> listString = new();
        if (debug)
        {
            var x = "Executing command: " + script;
            Logger.Logger.WriteLine(x);
            listString.Add(x);
        }

        var results = powershell.Invoke().ToList();
        if (debug)
            foreach (var t in results)
            {
                Logger.Logger.WriteLine(t.ToString());
                listString.Add(t.ToString());
            }

        powershell.Commands.Clear();
        return listString;
    }
}