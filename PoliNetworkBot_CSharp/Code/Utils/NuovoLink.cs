using PoliNetworkBot_CSharp.Code.Enums;

namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NuovoLink
    {
        public SuccessoGenerazioneLink isNuovo; //se il link è nuovo
        public string link;

        public NuovoLink(SuccessoGenerazioneLink v, string link = null)
        {
            isNuovo = v;
            this.link = link;
        }
    }
}