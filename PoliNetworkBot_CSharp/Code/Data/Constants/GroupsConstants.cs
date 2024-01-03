using PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket;

namespace PoliNetworkBot_CSharp.Code.Data.Constants;

public static class GroupsConstants
{
    public static readonly ChatIdTgWith100 BackupGroup = new()
        { Id = -653874197, VaAggiuntoMeno100 = false };

    public static readonly ChatIdTgWith100 GroupException = new()
        { Id = 1456960264, VaAggiuntoMeno100 = true };

    public static readonly ChatIdTgWith100 PermittedSpamGroup = new()
        { Id = 1685451643, VaAggiuntoMeno100 = true };

    public static readonly ChatIdTgWith100 BanNotificationGroup = new()
        { Id = 1710276126, VaAggiuntoMeno100 = true };

    public static ChatIdTgWith100 ConsiglioDegliAdminRiservato = new()
        { Id = 1419772154, VaAggiuntoMeno100 = true };

    public static readonly ChatIdTgWith100 PianoDiStudi = new()
        { Id = 1208900229, VaAggiuntoMeno100 = true };

    public static readonly ChatIdTgWith100 AskPolimi = new()
        { Id = 1251460298, VaAggiuntoMeno100 = true };

    public static readonly ChatIdTgWith100 Dsu = new()
        { Id = 1241129618, VaAggiuntoMeno100 = true };

    public static ChatIdTgWith100 Testing = new()
        { Id = 1436937011, VaAggiuntoMeno100 = true };
}