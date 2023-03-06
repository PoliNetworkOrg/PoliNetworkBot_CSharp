using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Enums.Action;
using PoliNetworkBot_CSharp.Code.Objects;
using PoliNetworkBot_CSharp.Code.Objects.Action;
using PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;
using PoliNetworkBot_CSharp.Code.Utils.Assoc;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TestBot.CodeTest.Bots.Moderation.AllowMessage;

public class AllowMessageCheck
{
    private readonly List<Tuple<Message, ActionDoneObject, bool>> _tuples = new();

    [SetUp]
    public void Setup()
    {
        var from = new User { Id = 123456 };
        var chat = new Chat { Type = ChatType.Supergroup };
        _tuples.Add(
            new Tuple<Message, ActionDoneObject, bool>(
                new Message { Text = "test 1", From = from, Chat = chat },
                new ActionDoneObject(
                    ActionDoneEnum.CHECK_SPAM,
                    false,
                    SpamType.SPAM_PERMITTED),
                true
            )
        );

        _tuples.Add(
            new Tuple<Message, ActionDoneObject, bool>(
                new Message { Text = "test 2", From = from, Chat = chat },
                new ActionDoneObject(
                    ActionDoneEnum.GROUP_MESSAGE_HANDLED_NONE,
                    null,
                    SpamType.ALL_GOOD
                ),
                false
            )
        );


        _tuples.Add(
            new Tuple<Message, ActionDoneObject, bool>(
                new Message { Text = "https://docs.google.com/", From = from, Chat = chat },
                new ActionDoneObject(
                    ActionDoneEnum.CHECK_SPAM,
                    true,
                    SpamType.SPAM_LINK
                ),
                false
            )
        );

        _tuples.Add(
            new Tuple<Message, ActionDoneObject, bool>(
                new Message { Text = "https://docs.google.com/", From = from, Chat = chat },
                new ActionDoneObject(
                    ActionDoneEnum.CHECK_SPAM,
                    false,
                    SpamType.SPAM_PERMITTED
                ),
                true
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
            var message = new Message { ReplyToMessage = tMessage, From = tMessage.From };
            var e = new MessageEventArgs(message);
            await AssocGeneric.AllowMessage(e, null, TimeSpan.Zero);
        }

        var e2 = new MessageEventArgs(tMessage);
        var actionDone = await Main.MainMethod2(new TelegramBotParam(null, true), e2);

        Console.WriteLine(tMessage.Text);
        Console.WriteLine("Allowed " + (toAllow ? "true" : "false"));
        Console.WriteLine(actionDoneObject.ToString());

        Assert.That(actionDone, Is.EqualTo(actionDoneObject));
    }
}