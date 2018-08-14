using System;

namespace cloudeventsformatdemo
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ThriftSharp;
    using Formatting = Newtonsoft.Json.Formatting;

    class Program
    {
       
        static void Main(string[] args)
        {
            
            ICloudEventV10 cloudEvent = new JsonCloudEventV10
            {
                cloudEventsVersion = "1.0",
                contentType = null,
                eventID = "12345678",
                eventTime = DateTime.UtcNow,
                eventType = "test.type",
                eventTypeVersion = "1.0",
                source = "urn:test",
                Extensions = new Dictionary<string, JToken>
                {
                    { "myextension", "abcdef"},
                    { "yourextension", "ghijkl"},
                    { "theirextension", "mnopqr"},
                    { "extraThing1", "stuvwx" },
                    { "extraThing2", "yzabcd" }
                }
            };

            Console.WriteLine("--- JSON 1.0");
            MemoryStream memoryStream = new MemoryStream();
            JsonSerialize(cloudEvent, memoryStream);
            memoryStream.Position = 0;
            DumpTextStream(memoryStream);

            Console.WriteLine("JSON: ----- Deserialize as 1.0 > 1.0");
            memoryStream.Position = 0;
            var jsonCloudEventV10 = JsonDeserialize<JsonCloudEventV10>(memoryStream);
            foreach (var extension in jsonCloudEventV10.Extensions)
            {
                Console.WriteLine($"1.0 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("JSON: ----- Deserialize 1.0 > 1.1");
            memoryStream.Position = 0;
            var jsonCloudEventV11 = JsonDeserialize<JsonCloudEventV11>(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {jsonCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {jsonCloudEventV11.extraThing2}");
            foreach (var extension in jsonCloudEventV11.Extensions)
            {
                Console.WriteLine($"1.1 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("--- JSON 1.1");
            memoryStream = new MemoryStream();
            JsonSerialize(jsonCloudEventV11, memoryStream);
            memoryStream.Position = 0;
            DumpTextStream(memoryStream);

            Console.WriteLine("JSON: ----- Deserialize as 1.1 > 1.0");
            memoryStream.Position = 0;
            jsonCloudEventV10 = JsonDeserialize<JsonCloudEventV10>(memoryStream);
            foreach (var extension in jsonCloudEventV10.Extensions)
            {
                Console.WriteLine($"1.0 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("JSON: ----- Deserialize 1.1 > 1.1");
            memoryStream.Position = 0;
            jsonCloudEventV11 = JsonDeserialize<JsonCloudEventV11>(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {jsonCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {jsonCloudEventV11.extraThing2}");
            foreach (var extension in jsonCloudEventV11.Extensions)
            {
                Console.WriteLine($"1.1 extension {extension.Key}: {extension.Value}");
            }


            // copy JSON 1.1 object to XML 1.0 object
            var xmlCloudEvent = new XmlCloudEventV10(jsonCloudEventV11);

            Console.WriteLine("--- XML 1.0");
            memoryStream = new MemoryStream();
            XmlSerializeV10(xmlCloudEvent, memoryStream);
            memoryStream.Position = 0;
            DumpTextStream(memoryStream);

            Console.WriteLine("XML: ----- Deserialize 1.0 > 1.0");
            memoryStream.Position = 0;
            var xmlCloudEventV10 = XmlDeserializeV10(memoryStream);
            foreach (var extension in xmlCloudEventV10.AllElements)
            {
                Console.WriteLine($"1.0 extension {extension.Name}: {extension.InnerText}");
            }

            Console.WriteLine("XML: ----- Deserialize 1.0 > 1.1");
            memoryStream.Position = 0;
            var xmlCloudEventV11 = XmlDeserializeV11(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {xmlCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {xmlCloudEventV11.extraThing2}");
            foreach (var extension in xmlCloudEventV11.AllElements)
            {
                Console.WriteLine($"1.1 extension {extension.Name}: {extension.InnerText}");
            }

            Console.WriteLine("--- XML 1.1");
            memoryStream = new MemoryStream();
            XmlSerializeV11(xmlCloudEventV11, memoryStream);
            memoryStream.Position = 0;
            DumpTextStream(memoryStream);

            Console.WriteLine("XML: ----- Deserialize 1.1 > 1.0");
            memoryStream.Position = 0;
            xmlCloudEventV10 = XmlDeserializeV10(memoryStream);
            foreach (var extension in xmlCloudEventV10.AllElements)
            {
                Console.WriteLine($"1.0 extension {extension.Name}: {extension.InnerText}");
            }

            Console.WriteLine("XML: ----- Deserialize 1.1 > 1.1");
            memoryStream.Position = 0;
            xmlCloudEventV11 = XmlDeserializeV11(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {xmlCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {xmlCloudEventV11.extraThing2}");
            foreach (var extension in xmlCloudEventV11.AllElements)
            {
                Console.WriteLine($"1.1 extension {extension.Name}: {extension.InnerText}");
            }

            Console.WriteLine("--- Thrift 1.0");
            var thriftCloudEvent = new ThriftCloudEventV10(xmlCloudEventV11);
            memoryStream = new MemoryStream();
            ThriftSerializeV10(thriftCloudEvent, memoryStream);
            memoryStream.Position = 0;
            DumpBinaryStream(memoryStream);

            Console.WriteLine("THRIFT: ----- Deserialize 1.0 > 1.0");
            memoryStream.Position = 0;
            var thriftCloudEventV10 = ThriftDeserializeV10(memoryStream);
            foreach (var extension in thriftCloudEventV10.extensions)
            {
                Console.WriteLine($"1.0 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("THRIFT: ----- Deserialize 1.0 > 1.1");
            memoryStream.Position = 0;
            var thriftCloudEventV11 = ThriftDeserializeV11(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {thriftCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {thriftCloudEventV11.extraThing2}");
            foreach (var extension in thriftCloudEventV11.extensions)
            {
                Console.WriteLine($"1.0 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("--- Thrift 1.1");
            thriftCloudEventV11 = new ThriftCloudEventV11(thriftCloudEventV10);
            memoryStream = new MemoryStream();
            ThriftSerializeV11(thriftCloudEventV11, memoryStream);
            memoryStream.Position = 0;
            DumpBinaryStream(memoryStream);

            Console.WriteLine("THRIFT: ----- Deserialize 1.1 > 1.1");
            memoryStream.Position = 0;
            thriftCloudEventV11 = ThriftDeserializeV11(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {thriftCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {thriftCloudEventV11.extraThing2}");
            foreach (var extension in thriftCloudEventV11.extensions)
            {
                Console.WriteLine($"1.1 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("--- Protobuf 1.0");
            var protobufCloudEvent = new ProtobufCloudEventV10(xmlCloudEventV11);
            memoryStream = new MemoryStream();
            ProtobufSerializeV10(protobufCloudEvent, memoryStream);
            memoryStream.Position = 0;
            DumpBinaryStream(memoryStream);

            Console.WriteLine("PROTOBUF: ----- Deserialize 1.0 > 1.0");
            memoryStream.Position = 0;
            var protobufCloudEventV10 = ProtobufDeserializeV10(memoryStream);
            foreach (var extension in protobufCloudEventV10.extensions)
            {
                Console.WriteLine($"1.0 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("PROTOBUF: ----- Deserialize 1.0 > 1.1");
            memoryStream.Position = 0;
            var protobufCloudEventV11 = ProtobufDeserializeV11(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {protobufCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {protobufCloudEventV11.extraThing2}");
            foreach (var extension in protobufCloudEventV11.extensions)
            {
                Console.WriteLine($"1.0 extension {extension.Key}: {extension.Value}");
            }

            Console.WriteLine("--- Protobuf 1.1");
            protobufCloudEventV11 = new ProtobufCloudEventV11(protobufCloudEventV10);
            memoryStream = new MemoryStream();
            ProtobufSerializeV11(protobufCloudEventV11, memoryStream);
            memoryStream.Position = 0;
            DumpBinaryStream(memoryStream);

            Console.WriteLine("PROTOBUF: ----- Deserialize 1.1 > 1.1");
            memoryStream.Position = 0;
            protobufCloudEventV11 = ProtobufDeserializeV11(memoryStream);
            Console.WriteLine($"1.1 builtin extraThing1 {protobufCloudEventV11.extraThing1}");
            Console.WriteLine($"1.1 builtin extraThing2 {protobufCloudEventV11.extraThing2}");
            foreach (var extension in protobufCloudEventV11.extensions)
            {
                Console.WriteLine($"1.1 extension {extension.Key}: {extension.Value}");
            }

        }

        private static void DumpBinaryStream(MemoryStream memoryStream)
        {
            string hexLine = string.Empty;
            string textLine = string.Empty;

            while (memoryStream.Position < memoryStream.Length)
            {
                int data = memoryStream.ReadByte();
                hexLine += data.ToString("X2") + " ";
                if (char.IsControl((char) data))
                {
                    textLine += ".";
                }
                else
                {
                    textLine += (char) data;
                }

                if (memoryStream.Position % 16 == 0)
                {
                    Console.WriteLine($"{hexLine} {textLine}");
                    hexLine = string.Empty;
                    textLine = String.Empty;
                }
            }
            Console.WriteLine($"{hexLine} {textLine}");
        }

        private static void DumpTextStream(MemoryStream memoryStream)
        {
            using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8, false, 8192, true))
            {
                 Console.WriteLine(streamReader.ReadToEnd());
            }
        }

        static JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });
        static void JsonSerialize<T>(T ev, Stream stream) where T : ICloudEventV10
        {
            using (var sw = new JsonTextWriter(new StreamWriter(stream, Encoding.UTF8, 8192, true)))
            {
                jsonSerializer.Serialize(sw, ev);
                sw.Flush();
            }
        }

        static T JsonDeserialize<T>(Stream stream) where T : ICloudEventV10
        {
            using (var sr = new JsonTextReader(new StreamReader(stream, Encoding.UTF8, false, 8192, true)))
            {
                return jsonSerializer.Deserialize<T>(sr);
            }
        }

        static XmlSerializer xmlSerializerV10 = new XmlSerializer(typeof(XmlCloudEventV10));
        static void XmlSerializeV10(XmlCloudEventV10 ev, Stream stream) 
        {
            using (var sw = new XmlTextWriter(new StreamWriter(stream, Encoding.UTF8, 8192, true)))
            {
                sw.Formatting = System.Xml.Formatting.Indented;
                xmlSerializerV10.Serialize(sw, ev);
                sw.Flush();
            }
        }

        static XmlCloudEventV10 XmlDeserializeV10 (Stream stream)
        {
            using (var sr = new XmlTextReader(new StreamReader(stream, Encoding.UTF8, false, 8192, true)))
            {
                return (XmlCloudEventV10)(xmlSerializerV10.Deserialize(sr));
            }
        }

        static XmlSerializer xmlSerializerV11 = new XmlSerializer(typeof(XmlCloudEventV11));
        static void XmlSerializeV11(XmlCloudEventV11 ev, Stream stream)
        {
            using (var sw = new XmlTextWriter(new StreamWriter(stream, Encoding.UTF8, 8192, true)))
            {
                sw.Formatting = System.Xml.Formatting.Indented;
                xmlSerializerV11.Serialize(sw, ev);
                sw.Flush();
            }
        }

        static XmlCloudEventV11 XmlDeserializeV11(Stream stream)
        {
            using (var sr = new XmlTextReader(new StreamReader(stream, Encoding.UTF8, false, 8192, true)))
            {
                return (XmlCloudEventV11)(xmlSerializerV11.Deserialize(sr));
            }
        }

        static void ThriftSerializeV10(ThriftCloudEventV10 ev, Stream stream)
        {
            using (var sw = new BinaryWriter(stream, Encoding.UTF8, true))
            {                                                      
                sw.Write(ThriftSerializer.Serialize(ev));
                sw.Flush();
            }
        }

        static ThriftCloudEventV10 ThriftDeserializeV10(Stream stream)
        {
            using (var sr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                return ThriftSerializer.Deserialize<ThriftCloudEventV10>(sr.ReadBytes((int)stream.Length));
            }
        }

        static void ThriftSerializeV11(ThriftCloudEventV11 ev, Stream stream)
        {
            using (var sw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                sw.Write(ThriftSerializer.Serialize(ev));
                sw.Flush();
            }
        }

        static ThriftCloudEventV11 ThriftDeserializeV11(Stream stream)
        {
            using (var sr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var result = ThriftSerializer.Deserialize<ThriftCloudEventV11>(sr.ReadBytes((int)stream.Length));
                result.AfterDeserialization();
                return result;
            }
        }

        static void ProtobufSerializeV10(ProtobufCloudEventV10 ev, Stream stream)
        {
            using (var sw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                ProtoBuf.Serializer.Serialize(stream, ev);
                sw.Flush();
            }
        }

        static ProtobufCloudEventV10 ProtobufDeserializeV10(Stream stream)
        {
            using (var sr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                return ProtoBuf.Serializer.Deserialize<ProtobufCloudEventV10>(stream);
            }
        }

        static void ProtobufSerializeV11(ProtobufCloudEventV11 ev, Stream stream)
        {
            using (var sw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                ProtoBuf.Serializer.Serialize(stream, ev);
                sw.Flush();
            }
        }

        static ProtobufCloudEventV11 ProtobufDeserializeV11(Stream stream)
        {
            using (var sr = new BinaryReader(stream, Encoding.UTF8, true))
            {
                return ProtoBuf.Serializer.Deserialize<ProtobufCloudEventV11>(stream);
            }
        }
    }
}
