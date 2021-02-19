using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal class GruppoTG
    {
        public string idLink;
        public string nome;

        public GruppoTG(JToken idLink, JToken nome)
        {
            this.idLink = idLink.ToString();
            this.nome = nome.ToString();
        }
    }
}