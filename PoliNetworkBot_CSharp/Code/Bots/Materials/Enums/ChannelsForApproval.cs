using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Enums
{
    class ChannelsForApproval
    {
        static long matNanoChannel = -1001227167038;
        static long aesChannel = -1001413802045;
        static long infoChannel = -1001422638605;
        static long mobilityMDChannel = -1001401676534;
        static long electronicsChannel = -1001165704108;
        static long automationChannel = -1001541302296;
        static long chimicaChannel = -1001510001243;
        static long debug = -1001403617749;

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
}
