namespace cloudeventsformatdemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;
    using Newtonsoft.Json.Linq;

    [XmlRoot("CloudEvent", Namespace = "")]
    public class XmlCloudEventV10 : ICloudEventV10, CloudEventsExtensions
    {
        public static XmlDocument XmlFactory = new XmlDocument();

        public XmlCloudEventV10()
        {

        }

        public XmlCloudEventV10(ICloudEventV10 ce)
        {
            AllElements = new List<XmlElement>();
            foreach (var propertyInfo in ce.GetType().GetProperties())
            {
                bool dontCopy = false;
                foreach (var customAttribute in propertyInfo.CustomAttributes)
                {
                    if (customAttribute.AttributeType == typeof(DontCopyAttribute))
                    {
                        dontCopy = true;
                        break;
                    }
                }

                if (dontCopy)
                {
                    continue;
                }

                var p = this.GetType().GetProperty(propertyInfo.Name);
                if (p != null)
                {
                    p.SetValue(this, propertyInfo.GetValue(ce));
                }
                else
                {
                    var element = XmlFactory.CreateElement(propertyInfo.Name);
                    element.InnerText = propertyInfo.GetValue(ce).ToString();
                    AllElements.Add(element);
                }                          
            }
            ((CloudEventsExtensions)this).CopyFrom(ce as CloudEventsExtensions);
        }

        [XmlElement]
        public string cloudEventsVersion { get; set; }
        [XmlElement]
        public string eventTypeVersion { get; set; }
        [XmlElement]
        public string eventType { get; set; }
        [XmlElement]
        public string source { get; set; }
        [XmlElement]
        public string eventID { get; set; }
        [XmlElement]
        public DateTime eventTime { get; set; }
        [XmlElement]
        public string schemaURL { get; set; }
        [XmlElement]
        public string contentType { get; set; }
        [XmlElement]
        public object data { get; set; }

        [XmlAnyElement, DontCopy]
        public List<XmlElement> AllElements { get; set; }

        IDictionary<string, object> CloudEventsExtensions.GetExtensions()
        {
            if (AllElements == null)
            {
                return null;
            }
            Dictionary<string, object> ceExtensions = new Dictionary<string, object>();
            foreach (var extension in AllElements)
            {
                ceExtensions.Add(extension.LocalName, extension.InnerXml);
            }
            return ceExtensions;
        }

        void CloudEventsExtensions.CopyFrom(CloudEventsExtensions extensions)
        {
            if (extensions == null)
            {
                return;
            }

            foreach (var extension in extensions.GetExtensions())
            {
                XmlElement item = XmlFactory.CreateElement(extension.Key);
                item.InnerText = extension.Value.ToString();
                AllElements.Add(item);
            }
        }
    }
}