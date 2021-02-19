using Newtonsoft.Json.Linq;
using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal class GruppoTG
    {
        public string idLink;
        public string nome;
        public long? id;

        public GruppoTG(JToken idLink, JToken nome)
        {
            this.idLink = idLink.ToString();
            this.nome = nome.ToString();
        }

        internal void UpdateID(long value)
        {
            this.id = value;
        }
    }
}