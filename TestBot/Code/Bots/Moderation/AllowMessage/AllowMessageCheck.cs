using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using Telegram.Bot.Types;

namespace TestBot.Code.Bots.Moderation.AllowMessage;

public class AllowMessageCheck
{
    private readonly List<Tuple<string, ActionDoneObject>> _tuples = new(); 
    
    [SetUp]
    public void Setup()
    {
        _tuples.Add(new Tuple<string, ActionDoneObject>("to be allowed", new ActionDoneObject(ActionDoneEnum.NONE, null, null)));
    }

    [Test]
    public async Task TestAllowMessage()
    {
        foreach (var (text, item2) in _tuples)
        {
            await TestSingleAllowMessage(text, item2);
        }
    }

    private static async Task TestSingleAllowMessage(string text, ActionDoneObject item2)
    {
        var message = new Message {ReplyToMessage = new Message() {Text = text}};
        var message2 = new Message(){Text = text};
        var e = new MessageEventArgs(message);
        await PoliNetworkBot_CSharp.Code.Utils.Assoc.AllowMessage(e, null);

        var e2 = new MessageEventArgs(message2);
        var actionDone = await PoliNetworkBot_CSharp.Code.Bots.Moderation.Main.MainMethod2(new object(), e2);
        Assert.That(actionDone, Is.EqualTo(item2));
    }
}