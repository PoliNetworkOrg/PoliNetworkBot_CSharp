using PoliNetworkBot_CSharp.Code.Enums;
using PoliNetworkBot_CSharp.Code.Objects;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TestBot.Code.Bots.Moderation.Spam;

public class CheckSpam
{
    private List<Tuple<Message, SpamType>>? _tuples;

    [SetUp]
    public void Setup()
    {
        _tuples = new List<Tuple<Message, SpamType>>
        {
            new(new Message() { Chat = new Chat() { Type = ChatType.Private, Id = 123456 } }, SpamType.ALL_GOOD),
            new(
                new Message()
                {
                    Text = "https://web.whatsapp.com/", Chat = new Chat() { Type = ChatType.Supergroup, Id = 123456 }
                },
                SpamType.SPAM_LINK)
        };
    }

    [Test]
    public async Task Test1()
    {
        if (_tuples != null)
            foreach (var (message, item2) in _tuples)
            {
                var e = new MessageEventArgs(message);
                var b = await PoliNetworkBot_CSharp.Code.Bots.Moderation.SpamCheck.CheckSpam.CheckSpamAsync(e, null,
                    true);
                Assert.That(b, Is.EqualTo(item2));
            }
    }
}