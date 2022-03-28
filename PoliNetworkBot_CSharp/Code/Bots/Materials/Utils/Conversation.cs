#region

using System;
using System.Linq;
using Bot.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials.Utils;

[Serializable]
public class Conversation
{
    private string corsienum;
    private string percorso;
    private string scuolaenum;

    private Stati? stato;

    public Conversation()
    {
        stato = Stati.start;
    }

    public void SetStato(Stati? var)
    {
        stato = var;
    }

    public Stati? GetStato()
    {
        return stato;
    }

    internal void SetCorso(string corsienum)
    {
        this.corsienum = corsienum;
    }

    internal void SetScuola(string scuolaenum)
    {
        this.scuolaenum = scuolaenum;
    }

    internal string GetScuola()
    {
        return scuolaenum;
    }

    internal string Getcorso()
    {
        return corsienum;
    }

    internal void ScesoDiUnLivello(string text)
    {
        if (string.IsNullOrEmpty(percorso))
        {
            percorso = text;
            return;
        }

        percorso += "/" + text;
    }

    internal void ResetPercorso()
    {
        percorso = null;
    }

    internal string GetPercorso()
    {
        return percorso;
    }

    internal string GetGit()
    {
        return GetPercorso().Split(@"/").First();
    }
}