#region

using PoliNetworkBot_CSharp.Code.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.Utils;

internal class NuovoLink
{
    public readonly SuccessoGenerazioneLink IsNuovo; //se il link è nuovo
    public readonly string? Link;

    public NuovoLink(SuccessoGenerazioneLink v, string? link = null)
    {
        IsNuovo = v;
        Link = link;
    }
}