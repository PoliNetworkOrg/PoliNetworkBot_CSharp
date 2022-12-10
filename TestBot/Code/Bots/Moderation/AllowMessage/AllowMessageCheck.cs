using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Utils;
using Telegram.Bot.Types;

namespace TestBot.Code.Bots.Moderation.AllowMessage;

public class AllowMessageCheck
{
    private readonly List<Tuple<Message, ActionDoneObject, bool>> _tuples = new();

    [SetUp]
    public void Setup()
    {
        _tuples.Add(
            new Tuple<Message, ActionDoneObject, bool>(
                new Message(){Text = "test 1", From = new User(){Id = 123456}},
                new ActionDoneObject(
                    ActionDoneEnum.NONE,
                    null,
                    null),
                true
            )
        );

        _tuples.Add(
            new Tuple<Message, ActionDoneObject, bool>(
                new Message(){ Text = "test 2", From = new User(){Id = 123456}},
                new ActionDoneObject(
                    ActionDoneEnum.NONE,
                    null,
                    null
                ),
                false
            )
        );
    }

    [Test]
    public async Task TestAllowMessage()
    {
        foreach (var (text, item2, toAllow) in _tuples)
            await TestSingleAllowMessage(text, item2, toAllow);
    }

    private static async Task TestSingleAllowMessage(Message tMessage, ActionDoneObject actionDoneObject, bool toAllow)
    {
 

        if (toAllow)
        {
            var message = new Message { ReplyToMessage = tMessage };
            var e = new MessageEventArgs(message);
            await Assoc.AllowMessage(e, null);
        }

        var e2 = new MessageEventArgs(tMessage);
        var actionDone = await Main.MainMethod2(new object(), e2);
        Assert.That(actionDone, Is.EqualTo(actionDoneObject));
    }
}