namespace cloudeventsformatdemo
{
    using System.Xml.Serialization;

    [XmlRoot("CloudEvent", Namespace = "")]
    public class XmlCloudEventV11 : XmlCloudEventV10, ICloudEventV11
    {
        [XmlElement]
        public string extraThing1 { get; set; }
        [XmlElement]
        public string extraThing2 { get; set; }

        public XmlCloudEventV11()
        {

        }

        public XmlCloudEventV11(ICloudEventV11 ce) : base(ce)
        {

        }

    }
}