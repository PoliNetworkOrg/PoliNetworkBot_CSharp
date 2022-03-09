using System.Xml.Serialization;

namespace PoliNetworkBot_CSharp.Code.Bots.Materials
{
    public class item
    {
        [XmlAttribute]
        public string id_document;
        [XmlAttribute]
        public string path_document;
    }
}