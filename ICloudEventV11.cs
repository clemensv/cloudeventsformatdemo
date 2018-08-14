namespace cloudeventsformatdemo
{
    public interface ICloudEventV11 : ICloudEventV10
    {
        string extraThing1 { get; set; }
        string extraThing2 { get; set; }
    }
}