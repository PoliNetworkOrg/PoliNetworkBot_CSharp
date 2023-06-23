using Telegram.Bot;

namespace PoliNetworkBot_CSharp.Code.Objects.TelegramBotAbstract;

public class TelegramBotParam
{
    private readonly object? _sender;
    private readonly bool _test;


    public TelegramBotParam(object? sender, bool test)
    {
        _sender = sender;
        _test = test;
    }

    public SampleNuGet.Objects.TelegramBotAbstract? GetTelegramBot()
    {
        TelegramBotClient? telegramBotClientBot = null;
        if (_sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

        if (telegramBotClientBot == null)
        {
            if (!_test)
                return null;

            var botClient = new TelegramBotClient("");
            return new SampleNuGet.Objects.TelegramBotAbstract(botClient);
        }

        var telegramBotClient = SampleNuGet.Objects.TelegramBotAbstract.GetFromRam(telegramBotClientBot);
        return telegramBotClient;
    }
}