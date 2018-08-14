namespace cloudeventsformatdemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Metadata.Ecma335;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class JsonCloudEventV10 : ICloudEventV10, CloudEventsExtensions
    {
        public JsonCloudEventV10()
        {                          
        }

        public JsonCloudEventV10(ICloudEventV10 ce)
        {
            foreach (var propertyInfo in ce.GetType().GetProperties())
            {
                this.GetType().GetProperty(propertyInfo.Name).SetValue(this, propertyInfo.GetValue(ce));
            }                                                                      
            ((CloudEventsExtensions)this).CopyFrom(ce as CloudEventsExtensions);
        }

        [JsonProperty]
        public string cloudEventsVersion { get; set; }
        [JsonProperty]
        public string eventTypeVersion { get; set; }
        [JsonProperty]
        public string eventType { get; set; }
        [JsonProperty]
        public string source { get; set; }
        [JsonProperty]
        public string eventID { get; set; }
        [JsonProperty]
        public DateTime eventTime { get; set; }
        [JsonProperty]
        public string schemaURL { get; set; }
        [JsonProperty]
        public string contentType { get; set; }
        [JsonProperty]
        public object data { get; set; }

        [JsonExtensionData, DontCopy]
        public IDictionary<string, JToken> Extensions { get; set; }      

        IDictionary<string, object> CloudEventsExtensions.GetExtensions()
        {
            Dictionary<string, object> ceExtensions = new Dictionary<string, object>();
            foreach (var extension in Extensions)
            {
                ceExtensions.Add(extension.Key, extension.Value.ToObject<object>());
            }                                                                   
            return ceExtensions;
        }

        void CloudEventsExtensions.CopyFrom(CloudEventsExtensions extensions)
        {
            if (extensions == null)
            {
                this.Extensions = null;
                return;
            }
            this.Extensions = new Dictionary<string, JToken>();
            foreach (var extension in extensions.GetExtensions())
            {
               this.Extensions.Add(extension.Key, new JObject(extension.Value)); 
            }
        }


    }
}