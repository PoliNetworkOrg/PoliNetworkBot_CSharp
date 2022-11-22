using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects.Files;

namespace PoliNetworkBot_CSharp.Code.Objects.Log;

public class LogObject
{
    private readonly JObject _toLog;

    public LogObject(List<object?> values)
    {
        var stackTrace = Environment.StackTrace;
        _toLog = new JObject
        {
            ["stackTrace"] = GetJArray(stackTrace),
            ["values"] = GetJObject(values)
        };
    }

    private static JToken GetJArray(string stackTrace)
    {
        var jArray = new JArray();
        var s = stackTrace.Split("\n").Select(x => x.Trim()).ToList();
        foreach (var s2 in s) jArray.Add(s2);

        return jArray;
    }

    public string GetStringToLog()
    {
        return JsonConvert.SerializeObject(_toLog);
    }

    private static JToken GetJObject(List<object?> list)
    {
        var x = new JArray();
        foreach (var x2 in list)
            try
            {
                x.Add(x2);
            }
            catch
            {
                x.Add(x2?.ToString());
            }

        return x;
    }

    public TelegramFileContent ToTelegramFileContent(string caption)
    {
        StringJson stringJson = new StringJson(FileTypeJsonEnum.OBJECT, _toLog);
        TelegramFileContent x = new TelegramFileContent(stringJson, caption);
        return x;
    }
}