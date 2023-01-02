using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneReport
{
    public readonly bool Done;
    public readonly JToken? Extra;
    public readonly string? Message;
    public int? HowManyDone;

    public ActionDoneReport(string message, JToken? extra, bool done, int? i)
    {
        Message = message;
        Extra = extra;
        Done = done;
        HowManyDone = i;
    }

    public JToken GetJObject()
    {
        return new JObject
        {
            ["Extra"] = Extra,
            ["Message"] = Message,
            ["done"] = Done,
            ["howManyDone"] = HowManyDone
        };
    }

    public static JArray GetJArrayOfExceptions(IEnumerable<Exception> exceptions)
    {
        var r = new JArray();
        foreach (var jToken in exceptions.Select(variable => variable.ToString())) r.Add(jToken);

        return r;
    }

    public static JArray GetJarrayOfListOfJObjects(List<JObject> exceptions)
    {
        var r = new JArray();
        foreach (var variable in exceptions) r.Add(variable);
        return r;
    }


    public static JToken GetObject(List<ActionDoneReport?> actionDoneReports)
    {
        var jArray = new JArray();
        foreach (var variable in actionDoneReports)
            if (variable != null)
                jArray.Add(variable.GetJObject());

        return jArray;
    }
}