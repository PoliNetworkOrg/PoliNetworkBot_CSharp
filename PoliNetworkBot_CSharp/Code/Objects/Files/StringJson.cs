#region

using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Objects.Files;

public class StringJson
{
    private readonly string? _stringValue;
    private readonly object? _value;
    private readonly string? _simpleString;

    public StringJson(FileTypeJsonEnum jsoned, object? value)
    {
        _value = value;
        switch (jsoned)
        {
            case FileTypeJsonEnum.STRING_JSONED:
                _stringValue = value?.ToString();
                this._simpleString = value?.ToString();
                break;
            case FileTypeJsonEnum.OBJECT:
                _stringValue = JsonConvert.SerializeObject(value);
                this._simpleString = value?.ToString();
                break;
            case FileTypeJsonEnum.SIMPLE_STRING:
                _stringValue = JsonConvert.SerializeObject(value);
                this._simpleString = value?.ToString();
                break;
            default:
                _stringValue = JsonConvert.SerializeObject(value);
                this._simpleString = value?.ToString();
                break;
        }
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(_stringValue) || string.IsNullOrEmpty(this._simpleString);
    }

    public string? GetStringAsJson()
    {
        return _stringValue;
    }

    public object? Get(FileTypeJsonEnum? whatWeWant)
    {
        return whatWeWant switch
        {
            FileTypeJsonEnum.STRING_JSONED => this._stringValue,
            FileTypeJsonEnum.OBJECT => this._value,
            FileTypeJsonEnum.SIMPLE_STRING => this._simpleString,
            _ => null
        };
    }
}