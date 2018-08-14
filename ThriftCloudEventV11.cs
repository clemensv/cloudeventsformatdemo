namespace cloudeventsformatdemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ThriftSharp;

    [ThriftStruct("CloudEvent")]
    public class ThriftCloudEventV11 : ThriftCloudEventV10, ICloudEventV11
    {
        [ThriftField(1, true, nameof(cloudEventsVersion))]
        public override string cloudEventsVersion { get; set; }
        [ThriftField(2, true, nameof(eventTypeVersion))]
        public override string eventTypeVersion { get; set; }
        [ThriftField(3, true, nameof(eventType))]
        public override string eventType { get; set; }
        [ThriftField(4, true, nameof(source))]
        public override string source { get; set; }
        [ThriftField(5, true, nameof(eventID))]
        public override string eventID { get; set; }
        [ThriftField(6, true, nameof(eventTime))]
        public override string _eventTime
        {
            get { return eventTime.ToString(); }
            set { eventTime = DateTime.Parse(value); }
        }
        public override DateTime eventTime { get; set; }
        [ThriftField(7, false, nameof(schemaURL))]
        public override string schemaURL { get; set; }
        [ThriftField(8, false, nameof(contentType))]
        public override string contentType { get; set; }

        public override object data { get; set; }

        //[ThriftField(9, false, nameof(data))]
        public override byte[] _data { get; set; }

        [ThriftField(10, false, nameof(extensions)), DontCopy]
        public override Dictionary<string, string> extensions { get; set; }

        [ThriftField(11, false, nameof(extraThing1))]
        public virtual string extraThing1 { get; set; }
        [ThriftField(12, false, nameof(extraThing1))]
        public virtual string extraThing2 { get; set; }

        public ThriftCloudEventV11(ICloudEventV10 ce)
        {
            extensions = new Dictionary<string, string>();
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
                    if (p.PropertyType == typeof(string))
                    {
                        p.SetValue(this, propertyInfo.GetValue(ce) ?? string.Empty);
                    }
                    else
                    {
                        p.SetValue(this, propertyInfo.GetValue(ce));
                    }
                }
                else
                {
                    extensions.Add(propertyInfo.Name, propertyInfo.GetValue(ce).ToString());
                }
            }
            ((CloudEventsExtensions)this).CopyFrom(ce as CloudEventsExtensions);
        }

        public ThriftCloudEventV11()
        {                                
        }

        internal void AfterDeserialization()
        {
            if (extensions != null)
            {
                foreach (var extension in extensions.ToArray())
                {
                    var p = GetType().GetProperty(extension.Key);
                    if (p != null)
                    {
                        p.SetValue(this, extension.Value);
                        extensions.Remove(extension.Key);
                    }
                }
            }

        }
    }
}