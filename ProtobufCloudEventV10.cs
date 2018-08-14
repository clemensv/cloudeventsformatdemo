namespace cloudeventsformatdemo
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using ProtoBuf;

    [ProtoContract()]
    public class ProtobufCloudEventV10 : ICloudEventV10, CloudEventsExtensions
    {
        [ProtoMember(1, Name = nameof(cloudEventsVersion))]
        public virtual string cloudEventsVersion { get; set; }
        [ProtoMember(2, Name = nameof(eventTypeVersion))]
        public virtual string eventTypeVersion { get; set; }
        [ProtoMember(3, Name = nameof(eventType))]
        public virtual string eventType { get; set; }
        [ProtoMember(4, Name = nameof(source))]
        public virtual string source { get; set; }
        [ProtoMember(5, Name = nameof(eventID))]
        public virtual string eventID { get; set; }
        [ProtoMember(6, Name = nameof(eventTime))]
        public virtual DateTime eventTime { get; set; }
        [ProtoMember(7, Name= nameof(schemaURL))]
        public virtual string schemaURL { get; set; }
        [ProtoMember(8, Name = nameof(contentType))]
        public virtual string contentType { get; set; }

        public virtual object data { get; set; }

        [ProtoMember(9, Name = nameof(data))]
        public virtual byte[] _data { get; set; }

        [ProtoMember(10, Name = nameof(extensions), DynamicType = true), DontCopy]
        public virtual Dictionary<string,string> extensions { get; set; }


        public ProtobufCloudEventV10()
        {

        }
        public ProtobufCloudEventV10(ICloudEventV10 ce)
        {
            extensions = new Dictionary<string,string>();
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

        IDictionary<string,object> CloudEventsExtensions.GetExtensions()
        {
            if (extensions == null)
            {
                return null;
            }
            Dictionary<string,object> ceExtensions = new Dictionary<string,object>();
            foreach (var extension in extensions)
            {
                ceExtensions.Add(extension.Key, extension.Value);
            }
            return ceExtensions;
        }

        void CloudEventsExtensions.CopyFrom(CloudEventsExtensions otherExtensions)
        {
            if (otherExtensions == null)
            {
                return;
            }

            foreach (var extension in otherExtensions.GetExtensions())
            {
                var p = this.GetType().GetProperty(extension.Key);
                if (p != null)
                {
                    p.SetValue(this, extension.Value);
                }
                else
                {
                    this.extensions.Add(extension.Key, extension.Value?.ToString());
                }
            }
        }

    }
}