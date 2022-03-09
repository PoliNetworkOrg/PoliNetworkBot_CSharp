using Bot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils
{
    [System.Serializable]
    public class Conversation
    {

        stati? stato;
        string corsienum;
        string scuolaenum;
        string percorso;

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
            return this.scuolaenum;
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
            return this.percorso;
        }

        internal string getGit()
        {
            return getPercorso().Split(@"/").First();
        }
    }
}
