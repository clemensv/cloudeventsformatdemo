namespace cloudeventsformatdemo
{
    using Newtonsoft.Json;

    class JsonCloudEventV11 : JsonCloudEventV10, ICloudEventV11
    {
        [JsonProperty]
        public string extraThing1 { get; set; }

        [JsonProperty]
        public string extraThing2 { get; set; }
    }
}