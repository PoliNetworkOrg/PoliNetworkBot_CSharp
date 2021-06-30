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

        public GruppoTG(JToken idLink, JToken nome, JToken id)
        {
            this.idLink = idLink.ToString();
            this.nome = nome.ToString();
            try
            {
                this.id = Convert.ToInt64( id.ToString());
            }
            catch
            {
                ;
            }
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