using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects.Log;

public class LogObject
{
    private List<object?> _values;
    private readonly string _stackTrace;
    private readonly IDictionary _enviromentVariables;
    private JObject toLog;

    public LogObject(List<object?> values)
    {
        this._values = values;
        this._stackTrace = Environment.StackTrace;
        this._enviromentVariables = Environment.GetEnvironmentVariables();
        this.toLog = new JObject
        {
            ["stackTrace"] = _stackTrace,
            ["enviromentVariables"] = GetJObject( this._enviromentVariables),
            ["values"] = GetJObject(_values)
        };
    }

    public string getStringToLog()
    {
        return JsonConvert.SerializeObject(this.toLog);
    }

    private static JToken GetJObject(List<object?> list)
    {
        var x = new JArray();
        foreach (var x2 in list)
        {
            try
            {
                x.Add(x2);
            }
            catch
            {
                x.Add(x2?.ToString());
            }
        }
        return x;
    }

    private static JToken GetJObject(IDictionary dictionary)
    {
        var r = new JObject();
        foreach (var x in dictionary.Keys)
        {
            var item = dictionary[x];
            r[x] = GetJToken(item);
        }

        return r;
    }

    private static string? GetJToken(object? item)
    {
        try
        {
            return item == null ? null : JsonConvert.SerializeObject(item);
        }
        catch
        {
            return item?.ToString();
        }
    }
}