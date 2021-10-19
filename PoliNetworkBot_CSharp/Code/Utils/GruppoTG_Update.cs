using System;
using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class GruppoTG_Update
    {
        public GruppoTG gruppoTG;
        public SuccessoGenerazioneLink successoGenerazioneLink;

        public GruppoTG_Update(GruppoTG p, SuccessoGenerazioneLink eRRORE)
        {
            gruppoTG = p;
            successoGenerazioneLink = eRRORE;
        }

        public string ExceptionMessage { get; internal set; }
        public Exception ExceptionObject { get; internal set; }
        public bool? query1Fallita { get; internal set; }
        public bool? query2Fallita { get; internal set; }
        public bool? createInviteLinkFallita { get; internal set; }
    }
}