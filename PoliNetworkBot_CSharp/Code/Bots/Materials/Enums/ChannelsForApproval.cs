#region

using System;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Enums;

internal static class ChannelsForApproval
{
    private const long MatNanoChannel = -1001227167038;
    private const long AesChannel = -1001413802045;
    private const long InfoChannel = -1001422638605;
    private const long MobilityMdChannel = -1001401676534;
    private const long ElectronicsChannel = -1001165704108;
    private const long AutomationChannel = -1001541302296;
    private const long ChimicaChannel = -1001510001243;
    private const long ManagementChannel = -1001564464792;
    private const long ArchitectureChannel = -1001668124149;
    private const long AmbientaleChannel = -1001934328516;

    internal static long? GetChannel(string? v)
    {
        if (v != null)
            return v.ToLower() switch
            {
                "matnano" => MatNanoChannel,
                "info" => InfoChannel,
                "mobilitymd" => MobilityMdChannel,
                "aes" => AesChannel,
                "electronics" => ElectronicsChannel,
                "automazione" => AutomationChannel,
                "chimica" => ChimicaChannel,
                "elettrica" => ElectronicsChannel,
                "energetica" => AesChannel,
                "mechanical" => AesChannel,
                "management" => ManagementChannel,
                "architettura" => ArchitectureChannel,
                "bioinformatics" => InfoChannel,
                "design" => ArchitectureChannel,
                "ambientale" => AmbientaleChannel,
                _ => throw new Exception("No such channel: " + v)
            };
        return null;
    }
}