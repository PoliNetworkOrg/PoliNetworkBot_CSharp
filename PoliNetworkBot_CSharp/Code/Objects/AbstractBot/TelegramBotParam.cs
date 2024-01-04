using Telegram.Bot;

namespace PoliNetworkBot_CSharp.Code.Objects.AbstractBot;

public class TelegramBotParam
{
    private readonly object? _sender;
    private readonly bool _test;


    public TelegramBotParam(object? sender, bool test)
    {
        _sender = sender;
        _test = test;
    }

    public AbstractBot.TelegramBotAbstract? GetTelegramBot()
    {
        TelegramBotClient? telegramBotClientBot = null;
        if (_sender is TelegramBotClient tmp) telegramBotClientBot = tmp;

        if (telegramBotClientBot == null)
        {
            if (!_test)
                return null;

            var botClient = new TelegramBotClient("");
            return new AbstractBot.TelegramBotAbstract(botClient, null);
        }

        var telegramBotClient = AbstractBot.TelegramBotAbstract.GetFromRam(telegramBotClientBot);
        return telegramBotClient;
    }
}