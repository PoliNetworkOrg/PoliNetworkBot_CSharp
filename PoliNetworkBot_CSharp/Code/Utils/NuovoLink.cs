namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NuovoLink
    {
        public Enums.SuccessoGenerazioneLink isNuovo; //se il link è nuovo
        public string link;

        public NuovoLink(Enums.SuccessoGenerazioneLink v, string link = null)
        {
            this.isNuovo = v;
            this.link = link;
        }
    }
}