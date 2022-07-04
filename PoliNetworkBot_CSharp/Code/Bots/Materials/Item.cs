#region

using System.Xml.Serialization;

#endregion

namespace PoliNetworkBot_CSharp.Code.Bots.Materials;

public class Item
{
    [XmlAttribute] public string? id_document;

    [XmlAttribute] public string? path_document;
}