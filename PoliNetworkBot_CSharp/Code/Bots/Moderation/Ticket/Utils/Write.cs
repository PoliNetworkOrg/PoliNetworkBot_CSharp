using PoliNetworkBot_CSharp.Code.Data.Constants;
using PoliNetworkBot_CSharp.Code.Data.Variables;
using PoliNetworkBot_CSharp.Code.Utils;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation.Ticket.Utils;

public static class Write
{
    public static void WriteThreadsToFile()
    {
        try
        {
            FileSerialization.WriteToBinaryFile(
                Paths.Bin.MessagesThread,
                GlobalVariables.Threads
            );
        }
        catch
        {
            // ignored
        }
    }
}