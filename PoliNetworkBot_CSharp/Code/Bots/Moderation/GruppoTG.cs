using Newtonsoft.Json.Linq;
using System;

namespace PoliNetworkBot_CSharp.Code.Bots.Moderation
{
    internal class GruppoTG
    {
        public string idLink;
        public string nome;
        public long? id;
        public string newLink;

        public GruppoTG(JToken idLink, JToken nome)
        {
            this.idLink = idLink.ToString();
            this.nome = nome.ToString();
        }

        internal void UpdateID(long value)
        {
            this.id = value;
        }

        internal void UpdateNewLink(string link)
        {
            this.newLink = link;
        }
    }
}