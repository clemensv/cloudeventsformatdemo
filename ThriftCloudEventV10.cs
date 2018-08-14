namespace cloudeventsformatdemo
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using ThriftSharp;

    [ThriftStruct("CloudEvent")]
    public class ThriftCloudEventV10 : ICloudEventV10, CloudEventsExtensions
    {
        [ThriftField(1, true, nameof(cloudEventsVersion))]
        public virtual string cloudEventsVersion { get; set; }
        [ThriftField(2, true, nameof(eventTypeVersion))]
        public virtual string eventTypeVersion { get; set; }
        [ThriftField(3, true, nameof(eventType))]
        public virtual string eventType { get; set; }
        [ThriftField(4, true, nameof(source))]
        public virtual string source { get; set; }
        [ThriftField(5, true, nameof(eventID))]
        public virtual string eventID { get; set; }
        [ThriftField(6, true, nameof(eventTime))]
        public virtual string _eventTime
        {
            get { return eventTime.ToString();} set { eventTime = DateTime.Parse(value);}
        }
        public virtual DateTime eventTime { get; set; }
        [ThriftField(7, false, nameof(schemaURL))]
        public virtual string schemaURL { get; set; }
        [ThriftField(8, false, nameof(contentType))]
        public virtual string contentType { get; set; }

        public virtual object data { get; set; }

        //[ThriftField(9, false, nameof(data))]
        public virtual byte[] _data { get; set; }

        [ThriftField(10, false, nameof(extensions)), DontCopy]
        public virtual Dictionary<string, string> extensions { get; set; }


        public ThriftCloudEventV10()
        {

        }
        public ThriftCloudEventV10(ICloudEventV10 ce)
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
                        p.SetValue(this, propertyInfo.GetValue(ce)??string.Empty);
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

        IDictionary<string, object> CloudEventsExtensions.GetExtensions()
        {
            if (extensions == null)
            {
                return null;
            }
            Dictionary<string, object> ceExtensions = new Dictionary<string, object>();
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