namespace cloudeventsformatdemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ProtoBuf;

    [ProtoContract()]
    public class ProtobufCloudEventV11 : ProtobufCloudEventV10, ICloudEventV11
    {
        [ProtoMember(1, Name = nameof(cloudEventsVersion))]
        public override string cloudEventsVersion { get; set; }
        [ProtoMember(2, Name = nameof(eventTypeVersion))]
        public override string eventTypeVersion { get; set; }
        [ProtoMember(3, Name = nameof(eventType))]
        public override string eventType { get; set; }
        [ProtoMember(4, Name = nameof(source))]
        public override string source { get; set; }
        [ProtoMember(5, Name = nameof(eventID))]
        public override string eventID { get; set; }
        [ProtoMember(6, Name = nameof(eventTime))]
        public override DateTime eventTime { get; set; }
        [ProtoMember(7, Name = nameof(schemaURL))]
        public override string schemaURL { get; set; }
        [ProtoMember(8, Name = nameof(contentType))]
        public override string contentType { get; set; }

        public override object data { get; set; }

        [ProtoMember(9, Name = nameof(data))]
        public override byte[] _data { get; set; }

        [ProtoMember(10, Name = nameof(extensions), DynamicType = true), DontCopy]
        public override Dictionary<string, string> extensions { get; set; }

        [ProtoMember(11, Name = nameof(extraThing1))]
        public virtual string extraThing1 { get; set; }
        [ProtoMember(12, Name = nameof(extraThing1))]
        public virtual string extraThing2 { get; set; }

        public ProtobufCloudEventV11(ICloudEventV10 ce)
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

        public ProtobufCloudEventV11()
        {
        }

        [ProtoAfterDeserialization]
        void AfterDeserialization()
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