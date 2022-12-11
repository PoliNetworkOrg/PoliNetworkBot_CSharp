using PoliNetworkBot_CSharp.Code.Objects;

namespace TestBot.Objects;

public class LanguageTestObject
{
    public readonly Language? Language;
    public readonly string? SelectedLang;
    public readonly string? Text;

    public LanguageTestObject(Language l, string text, string? selectedLang)
    {
        Language = l;
        Text = text;
        SelectedLang = selectedLang;
    }
}