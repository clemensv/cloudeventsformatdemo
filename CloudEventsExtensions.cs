namespace cloudeventsformatdemo
{
    using System.Collections.Generic;

    public interface CloudEventsExtensions
    {
        void CopyFrom(CloudEventsExtensions extensions);
        IDictionary<string, object> GetExtensions();
    }
}