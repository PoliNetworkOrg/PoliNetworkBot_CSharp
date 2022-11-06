using System.Threading.Tasks;
using PoliNetworkBot_CSharp.Code.Objects;

namespace PoliNetworkBot_CSharp.Code.Utils;

public static class AssocCommands
{
    public static async Task<bool> AssocWrite(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_SendAsync(sender, e);
        return false;
    }

    public static async Task<bool> AssocWriteDry(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_SendAsync(sender, e, true);
        return false;
    }

    public static async Task AssocPublish(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_Publish(sender, e);
    }

    public static async Task<bool> AssocRead(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_Read(sender, e, false);
        return false;
    }

    public static async Task<bool> AssocReadAll(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_ReadAll(sender, e);
        return false;
    }

    public static async Task<bool> AssocDelete(MessageEventArgs e, TelegramBotAbstract? sender)
    {
        _ = await Assoc.Assoc_Delete(sender, e);
        return false;
    }
}