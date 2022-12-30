﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Objects.Action;

public class ActionDoneReport
{
    public readonly JToken? Extra;
    public readonly string? Message;

    public ActionDoneReport(string message, JToken? extra)
    {
        Message = message;
        Extra = extra;
    }

    public static JArray GetJArrayOfExceptions(IEnumerable<Exception> exceptions)
    {
        var r = new JArray();
        foreach (var jToken in exceptions.Select(variable => variable.ToString())) r.Add(jToken);

        return r;
    }
}