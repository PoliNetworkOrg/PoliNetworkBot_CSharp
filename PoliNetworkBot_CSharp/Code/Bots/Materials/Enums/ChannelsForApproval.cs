#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;

internal class ChannelsForApproval
{
    private static readonly long matNanoChannel = -1001227167038;
    private static readonly long aesChannel = -1001413802045;
    private static readonly long infoChannel = -1001422638605;
    private static readonly long mobilityMDChannel = -1001401676534;
    private static readonly long electronicsChannel = -1001165704108;
    private static readonly long automationChannel = -1001541302296;
    private static readonly long chimicaChannel = -1001510001243;
#pragma warning disable IDE0051 // Rimuovi i membri privati inutilizzati
    private static readonly long debug = -1001403617749;
#pragma warning restore IDE0051 // Rimuovi i membri privati inutilizzati

    internal static long GetChannel(string v)
    {
        return v.ToLower() switch
        {
            "matnano" => matNanoChannel,
            "info" => infoChannel,
            "mobilitymd" => mobilityMDChannel,
            "aes" => aesChannel,
            "electronics" => electronicsChannel,
            "automazione" => automationChannel,
            "chimica" => chimicaChannel,
            "elettrica" => electronicsChannel,
            _ => throw new Exception("No such channel")
        };
    }
}