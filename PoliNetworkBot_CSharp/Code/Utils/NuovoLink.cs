namespace PoliNetworkBot_CSharp.Code.Utils
{
    internal class NuovoLink
    {
        public bool isNuovo; //se il link è nuovo
        public string link;

        public NuovoLink(bool v, string link = null)
        {
            this.isNuovo = v;
            this.link = link;
        }
    }
}