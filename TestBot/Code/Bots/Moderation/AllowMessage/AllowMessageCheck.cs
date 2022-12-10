using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using Telegram.Bot.Types;

namespace TestBot.Code.Bots.Moderation.AllowMessage;

public class AllowMessageCheck
{
    private readonly List<Tuple<string, ActionDoneEnum>> _tuples = new(); 
    
    [SetUp]
    public void Setup()
    {
        _tuples.Add(new Tuple<string, ActionDoneEnum>("to be allowed", ActionDoneEnum.NONE));
    }

    [Test]
    public async Task TestAllowMessage()
    {
        foreach (var (text, item2) in _tuples)
        {
            var message = new Message {ReplyToMessage = new Message() {Text = text}};
            var message2 = new Message(){Text = text};
            var e = new MessageEventArgs(message);
            await PoliNetworkBot_CSharp.Code.Utils.Assoc.AllowMessage(e, null);

            var e2 = new MessageEventArgs(message2);
            var actionDone = await PoliNetworkBot_CSharp.Code.Bots.Moderation.Main.MainMethod2(new object(), e2);
            Assert.AreEqual(actionDone.ActionDoneEnum,item2);
        }
       
    
    }
}