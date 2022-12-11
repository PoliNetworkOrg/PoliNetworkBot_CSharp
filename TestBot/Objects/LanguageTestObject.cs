using PoliNetworkBot_CSharp.Code.Objects;

namespace TestBot.Objects;

public class LanguageTestObject
{
    public readonly Language? Language;
    public readonly string? Text;
    public readonly string? SelectedLang;
    public LanguageTestObject(Language l, string text, string? selectedLang)
    {
        this.Language = l;
        this.Text = text;
        this.SelectedLang = selectedLang;
    }
}