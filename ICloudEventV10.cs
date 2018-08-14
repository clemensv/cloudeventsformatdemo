namespace cloudeventsformatdemo
{
    using System;

    public interface ICloudEventV10
    {
        string cloudEventsVersion { get; set; }
        string eventTypeVersion { get; set; }
        string eventType { get; set; }
        string source { get; set; }
        string eventID { get; set; }
        DateTime eventTime { get; set; }
        string schemaURL { get; set; }
        string contentType { get; set; }
        object data { get; set; }
    }
            
}