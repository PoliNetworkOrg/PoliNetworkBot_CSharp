using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneReport
{
    public readonly JToken? Extra;
    public readonly string? Message;
    public bool done;
    public int? howManyDone;

    public ActionDoneReport(string message, JToken? extra, bool done, int? i)
    {
        Message = message;
        Extra = extra;
        this.done = done;
        this.howManyDone = i;
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
}