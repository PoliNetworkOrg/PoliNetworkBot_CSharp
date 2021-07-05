using PoliNetworkBot_CSharp.Code.Bots.Moderation;
using PoliNetworkBot_CSharp.Code.Enums;
using System;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    public class GruppoTG_Update
    {
        public GruppoTG gruppoTG;
        public Enums.SuccessoGenerazioneLink successoGenerazioneLink;

        public GruppoTG_Update(GruppoTG p, SuccessoGenerazioneLink eRRORE)
        {
            this.gruppoTG = p;
            this.successoGenerazioneLink = eRRORE;
        }

        public string ExceptionMessage { get; internal set; }
        public Exception ExceptionObject { get; internal set; }
        public bool? query1Fallita { get; internal set; }
        public bool? query2Fallita { get; internal set; }
        public bool? createInviteLinkFallita { get; internal set; }
    }
}