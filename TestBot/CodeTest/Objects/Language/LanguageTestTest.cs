using PoliNetworkBot_CSharp.Code.Objects;
using TestBot.Objects;

namespace TestBot.CodeTest.Objects.Language;

public class LanguageTestTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestLanguage()
    {
        const string text = "ciao";
        const string langx = "langx";
        const string langy = "langy";
        var tuples =
            new List<LanguageTestObject>
            {
                new(new L(text), text, null),
                new(new L(langx, text), text, null),
                new(new L(langx, text), text, langy)
            };
        foreach (var item in tuples)
        {
            var x2 = item.Language?.Select(item.SelectedLang);
            Assert.That(x2, Is.EqualTo(item.Text));
        }
    }
}