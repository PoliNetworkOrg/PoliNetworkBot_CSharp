#region

using System;
using System.Linq;
using Bot.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils
{
    [Serializable]
    public class Conversation
    {
        private string corsienum;
        private string percorso;
        private string scuolaenum;

        private stati? stato;

        public Conversation()
        {
            stato = stati.start;
        }

        public void setStato(stati? var)
        {
            stato = var;
        }

        public stati? getStato()
        {
            return stato;
        }

        internal void setCorso(string corsienum)
        {
            this.corsienum = corsienum;
        }

        internal void setScuola(string scuolaenum)
        {
            this.scuolaenum = scuolaenum;
        }

        internal string getScuola()
        {
            return scuolaenum;
        }

        internal string getcorso()
        {
            return corsienum;
        }

        internal void scesoDiUnLivello(string text)
        {
            if (string.IsNullOrEmpty(percorso))
            {
                percorso = text;
                return;
            }

            percorso += "/" + text;
        }

        internal void resetPercorso()
        {
            percorso = null;
        }

        internal string getPercorso()
        {
            return percorso;
        }

        internal string getGit()
        {
            return getPercorso().Split(@"/").First();
        }
    }
}