#region

using System;
using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.Files;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class StringJson
{
    private readonly string? _simpleString;
    private readonly string? _stringValue;
    private readonly object? _value;

    public StringJson(FileTypeJsonEnum jsoned, object? value)
    {
        _value = value;
        switch (jsoned)
        {
            case FileTypeJsonEnum.STRING_JSONED:
                _stringValue = value?.ToString();
                _simpleString = value?.ToString();
                break;
            case FileTypeJsonEnum.OBJECT:
                _stringValue = JsonConvert.SerializeObject(value);
                _simpleString = value?.ToString();
                break;
            case FileTypeJsonEnum.SIMPLE_STRING:
                _stringValue = JsonConvert.SerializeObject(value);
                _simpleString = value?.ToString();
                break;
            default:
                _stringValue = JsonConvert.SerializeObject(value);
                _simpleString = value?.ToString();
                break;
        }
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(_stringValue) || string.IsNullOrEmpty(_simpleString);
    }

    public string? GetStringAsJson()
    {
        return _stringValue;
    }

    public object? Get(FileTypeJsonEnum? whatWeWant)
    {
        return whatWeWant switch
        {
            FileTypeJsonEnum.STRING_JSONED => _stringValue,
            FileTypeJsonEnum.OBJECT => _value,
            FileTypeJsonEnum.SIMPLE_STRING => _simpleString,
            _ => null
        };
    }
}