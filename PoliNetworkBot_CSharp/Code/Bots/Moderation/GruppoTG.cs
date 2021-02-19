using Newtonsoft.Json.Linq;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal class GruppoTG
    {
        public JToken idLink;
        public JToken nome;

        public GruppoTG(JToken idLink, JToken nome)
        {
            this.idLink = idLink;
            this.nome = nome;
        }
    }
}