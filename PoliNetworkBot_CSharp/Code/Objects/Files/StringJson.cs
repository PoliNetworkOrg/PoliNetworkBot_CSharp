using Newtonsoft.Json;
using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Objects.Files;

public class StringJson
{
    private readonly object? _value;
    private readonly string? _stringValue;

    public StringJson(FileTypeJsonEnum jsoned, object? value)
    {
        this._value = value;
        this._stringValue = jsoned switch
        {
            FileTypeJsonEnum.STRING_JSONED => value?.ToString(),
            FileTypeJsonEnum.OBJECT => JsonConvert.SerializeObject(value),
            FileTypeJsonEnum.SIMPLE_STRING => JsonConvert.SerializeObject(value),
            _ => this._stringValue
        };
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(_stringValue);
    }

    public string? GetStringAsJson()
    {
        return this._stringValue;
    }
}