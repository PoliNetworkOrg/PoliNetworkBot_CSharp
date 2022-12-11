using PoliNetworkBot_CSharp.Code.Objects;

namespace TestBot.Code.Objects.Language;

public class LanguageTest
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
        var tuples =
            new List<Tuple<PoliNetworkBot_CSharp.Code.Objects.Language, string>>
            {
                new(new L(text), text),
                new(new L(langx, text), text)
            };
        foreach (var item in tuples)
        {
            var x2 = item.Item1.Select();
            Assert.That(x2, Is.EqualTo(item.Item2));
        }
    }
}