# CloudEvents Extensibility Serialization

The code in this repo exists to illustrate how various different serialization models 
can leverage their respective strengths and deal with their respective constraints 
while the base specification retains a "flat" data model without a specific extensibility 
bucket.

I have not added cases where the JSON and XML objects are read dynamically since those
ought to be obvious. The strength of the flat model is that dynamic code which simply 
reads a JSON object into a dictionary doesn't break with a flat model and does break when
a formar extension is promoted to a top-level construct when using a model with a bag.
If anyone needs to have that clearly illustrated, I can add that.

The code is written in .NET Core C# and uses four different .NET serialization libraries.
For comparability, all libraries use the attribute-declarative model to drive the serializer.

The application defines a [base interface](ICloudEventV10.cs) for CloudEvents 1.0 and a 
derived [base interface]([base interface](ICloudEventV11.cs)) for a fictious version 1.1. The 
latter extends the well-known set of fields by two fields, `extraThing1` and `extraThing2`.

## JSON

The initial object is created as a 1.0 object, with five extensions added into a bag in the
in-memory serializer object for JSON. The object is then serialized to JSON, where the 
extensions show up as flat object members of the envelope. 

When the object is deserialized into 1.0, the elements go back into the bag. That's simply 
a function of the chosen serializer, JSON.NET. 

When the object is deserialized into 1.1, the `extraThing1` and `extraThing2` properties
and populated from the content, and the remaining extensions go into the bag. Again, that's
a function of the serializer.

Because the wire repersentation doesn't make a difference between the extensions in 1.0 and 1.1,
you'll see that serializing the resulting 1.1 in-memory object and reading it back as 1.0 or
1.1 yields the exact same expected results.

## XML

The XML CloudEvents 1.0 in-memory object is created as a deep copy from the JSON 1.1 object,
showing that there's nothing precluding extensions to be copied across different objects tailored
to different serializers.

The rest of the XML flow is exactly equivalent to JSON. The data on the wire shows up flat and
we're using the serializer's facility to deal with unknown objects to handle extensions.

## Thrift and Protobuf

The Thrift and Protobuf handling is practically identical. The Thrift and Protobuf serializers are 
using an attribute driven model and not the respective IDL compiler, but the 
result is wire compatible. 

The strategy here is to make an extension bag and for the in memory respresentation and to also use 
that bag on the wire, because both require schema. 

Specifically noteworthy is the case where a Thrift/Protobuf CloudEvents 1.0 object is being deserialized
into a 1.1 object. That case has a simple post-serialization step (declarative in the Protobuf case),
that generically maps content from the extension bag to the fields added in 1.1, resulting in a clean
upgrade path.

Neither of the two schema-bound models roundtrips such that you can losslessly deserialize a 1.1 object
into a 1.0 object with extra fields being demoted back into extensions as both XML and JSON support. That's
plainly a weakness of the external metadata model.

To deal with that, I suggest that we amend the Content-Type in the transport bindings (not the one for the 
data attribute) with a cloudEventsVersion parameters (e.g. application/cloudevents+protobuf;cloudEventsVersion=1.1)
which allows a schema bound serializer to pick the right base schema.







 


The main file to pay attention to is `Program.cs`. The 

```

--- JSON 1.0
{
  "cloudEventsVersion": "1.0",
  "eventTypeVersion": "1.0",
  "eventType": "test.type",
  "source": "urn:test",
  "eventID": "12345678",
  "eventTime": "2018-08-14T17:28:11.0470159Z",
  "schemaURL": null,
  "contentType": null,
  "data": null,
  "myextension": "abcdef",
  "yourextension": "ghijkl",
  "theirextension": "mnopqr",
  "extraThing1": "stuvwx",
  "extraThing2": "yzabcd"
}
JSON: ----- Deserialize as 1.0 > 1.0
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
1.0 extension extraThing1: stuvwx
1.0 extension extraThing2: yzabcd
JSON: ----- Deserialize 1.0 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.1 extension myextension: abcdef
1.1 extension yourextension: ghijkl
1.1 extension theirextension: mnopqr
--- JSON 1.1
{
  "extraThing1": "stuvwx",
  "extraThing2": "yzabcd",
  "cloudEventsVersion": "1.0",
  "eventTypeVersion": "1.0",
  "eventType": "test.type",
  "source": "urn:test",
  "eventID": "12345678",
  "eventTime": "2018-08-14T17:28:11.0470159Z",
  "schemaURL": null,
  "contentType": null,
  "data": null,
  "myextension": "abcdef",
  "yourextension": "ghijkl",
  "theirextension": "mnopqr"
}
JSON: ----- Deserialize as 1.1 > 1.0
1.0 extension extraThing1: stuvwx
1.0 extension extraThing2: yzabcd
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
JSON: ----- Deserialize 1.1 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.1 extension myextension: abcdef
1.1 extension yourextension: ghijkl
1.1 extension theirextension: mnopqr
--- XML 1.0
<?xml version="1.0" encoding="utf-8"?>
<CloudEvent xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <cloudEventsVersion>1.0</cloudEventsVersion>
  <eventTypeVersion>1.0</eventTypeVersion>
  <eventType>test.type</eventType>
  <source>urn:test</source>
  <eventID>12345678</eventID>
  <eventTime>2018-08-14T17:28:11.0470159Z</eventTime>
  <extraThing1>stuvwx</extraThing1>
  <extraThing2>yzabcd</extraThing2>
  <myextension>abcdef</myextension>
  <yourextension>ghijkl</yourextension>
  <theirextension>mnopqr</theirextension>
</CloudEvent>
XML: ----- Deserialize 1.0 > 1.0
1.0 extension extraThing1: stuvwx
1.0 extension extraThing2: yzabcd
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
XML: ----- Deserialize 1.0 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.1 extension myextension: abcdef
1.1 extension yourextension: ghijkl
1.1 extension theirextension: mnopqr
--- XML 1.1
<?xml version="1.0" encoding="utf-8"?>
<CloudEvent xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <cloudEventsVersion>1.0</cloudEventsVersion>
  <eventTypeVersion>1.0</eventTypeVersion>
  <eventType>test.type</eventType>
  <source>urn:test</source>
  <eventID>12345678</eventID>
  <eventTime>2018-08-14T17:28:11.0470159Z</eventTime>
  <myextension>abcdef</myextension>
  <yourextension>ghijkl</yourextension>
  <theirextension>mnopqr</theirextension>
  <extraThing1>stuvwx</extraThing1>
  <extraThing2>yzabcd</extraThing2>
</CloudEvent>
XML: ----- Deserialize 1.1 > 1.0
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
1.0 extension extraThing1: stuvwx
1.0 extension extraThing2: yzabcd
XML: ----- Deserialize 1.1 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.1 extension myextension: abcdef
1.1 extension yourextension: ghijkl
1.1 extension theirextension: mnopqr
--- Thrift 1.0
0B 00 01 00 00 00 03 31 2E 30 0B 00 02 00 00 00  .......1.0......
03 31 2E 30 0B 00 03 00 00 00 09 74 65 73 74 2E  .1.0.......test.
74 79 70 65 0B 00 04 00 00 00 08 75 72 6E 3A 74  type.......urn:t
65 73 74 0B 00 05 00 00 00 08 31 32 33 34 35 36  est.......123456
37 38 0B 00 07 00 00 00 00 0B 00 08 00 00 00 00  78..............
0B 00 06 00 00 00 13 31 34 2E 30 38 2E 32 30 31  .......14.08.201
38 20 31 37 3A 32 38 3A 31 31 0D 00 0A 0B 0B 00  8 17:28:11......
00 00 05 00 00 00 0B 65 78 74 72 61 54 68 69 6E  .......extraThin
67 31 00 00 00 06 73 74 75 76 77 78 00 00 00 0B  g1....stuvwx....
65 78 74 72 61 54 68 69 6E 67 32 00 00 00 06 79  extraThing2....y
7A 61 62 63 64 00 00 00 0B 6D 79 65 78 74 65 6E  zabcd....myexten
73 69 6F 6E 00 00 00 06 61 62 63 64 65 66 00 00  sion....abcdef..
00 0D 79 6F 75 72 65 78 74 65 6E 73 69 6F 6E 00  ..yourextension.
00 00 06 67 68 69 6A 6B 6C 00 00 00 0E 74 68 65  ...ghijkl....the
69 72 65 78 74 65 6E 73 69 6F 6E 00 00 00 06 6D  irextension....m
6E 6F 70 71 72 00 00 00 00 00 00 00 00 00 00 00  nopqr...........

THRIFT: ----- Deserialize 1.0 > 1.0
1.0 extension extraThing1: stuvwx
1.0 extension extraThing2: yzabcd
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
THRIFT: ----- Deserialize 1.0 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
--- Thrift 1.1
0B 00 01 00 00 00 03 31 2E 30 0B 00 02 00 00 00  .......1.0......
03 31 2E 30 0B 00 03 00 00 00 09 74 65 73 74 2E  .1.0.......test.
74 79 70 65 0B 00 04 00 00 00 08 75 72 6E 3A 74  type.......urn:t
65 73 74 0B 00 05 00 00 00 08 31 32 33 34 35 36  est.......123456
37 38 0B 00 06 00 00 00 13 31 34 2E 30 38 2E 32  78.......14.08.2
30 31 38 20 31 37 3A 32 38 3A 31 31 0B 00 07 00  018 17:28:11....
00 00 00 0B 00 08 00 00 00 00 0D 00 0A 0B 0B 00  ................
00 00 03 00 00 00 0B 6D 79 65 78 74 65 6E 73 69  .......myextensi
6F 6E 00 00 00 06 61 62 63 64 65 66 00 00 00 0D  on....abcdef....
79 6F 75 72 65 78 74 65 6E 73 69 6F 6E 00 00 00  yourextension...
06 67 68 69 6A 6B 6C 00 00 00 0E 74 68 65 69 72  .ghijkl....their
65 78 74 65 6E 73 69 6F 6E 00 00 00 06 6D 6E 6F  extension....mno
70 71 72 0B 00 0B 00 00 00 06 73 74 75 76 77 78  pqr.......stuvwx
0B 00 0C 00 00 00 06 79 7A 61 62 63 64 00 00 00  .......yzabcd...
00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................
00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  ................

THRIFT: ----- Deserialize 1.1 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.1 extension myextension: abcdef
1.1 extension yourextension: ghijkl
1.1 extension theirextension: mnopqr
--- Protobuf 1.0
0A 03 31 2E 30 12 03 31 2E 30 1A 09 74 65 73 74  ..1.0..1.0..test
2E 74 79 70 65 22 08 75 72 6E 3A 74 65 73 74 2A  .type".urn:test*
08 31 32 33 34 35 36 37 38 32 0B 08 9E D0 B7 F2  .123456782...Ð·ò
AA 85 C1 36 10 05 3A 00 42 00 52 15 0A 0B 65 78  ª.Á6..:.B.R...ex
74 72 61 54 68 69 6E 67 31 12 06 73 74 75 76 77  traThing1..stuvw
78 52 15 0A 0B 65 78 74 72 61 54 68 69 6E 67 32  xR...extraThing2
12 06 79 7A 61 62 63 64 52 15 0A 0B 6D 79 65 78  ..yzabcdR...myex
74 65 6E 73 69 6F 6E 12 06 61 62 63 64 65 66 52  tension..abcdefR
17 0A 0D 79 6F 75 72 65 78 74 65 6E 73 69 6F 6E  ...yourextension
12 06 67 68 69 6A 6B 6C 52 18 0A 0E 74 68 65 69  ..ghijklR...thei
72 65 78 74 65 6E 73 69 6F 6E 12 06 6D 6E 6F 70  rextension..mnop
71 72  qr
PROTOBUF: ----- Deserialize 1.0 > 1.0
1.0 extension extraThing1: stuvwx
1.0 extension extraThing2: yzabcd
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
PROTOBUF: ----- Deserialize 1.0 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.0 extension myextension: abcdef
1.0 extension yourextension: ghijkl
1.0 extension theirextension: mnopqr
--- Protobuf 1.1
0A 03 31 2E 30 12 03 31 2E 30 1A 09 74 65 73 74  ..1.0..1.0..test
2E 74 79 70 65 22 08 75 72 6E 3A 74 65 73 74 2A  .type".urn:test*
08 31 32 33 34 35 36 37 38 32 0B 08 9E D0 B7 F2  .123456782...Ð·ò
AA 85 C1 36 10 05 3A 00 42 00 52 15 0A 0B 6D 79  ª.Á6..:.B.R...my
65 78 74 65 6E 73 69 6F 6E 12 06 61 62 63 64 65  extension..abcde
66 52 17 0A 0D 79 6F 75 72 65 78 74 65 6E 73 69  fR...yourextensi
6F 6E 12 06 67 68 69 6A 6B 6C 52 18 0A 0E 74 68  on..ghijklR...th
65 69 72 65 78 74 65 6E 73 69 6F 6E 12 06 6D 6E  eirextension..mn
6F 70 71 72 5A 06 73 74 75 76 77 78 62 06 79 7A  opqrZ.stuvwxb.yz
61 62 63 64  abcd
PROTOBUF: ----- Deserialize 1.1 > 1.1
1.1 builtin extraThing1 stuvwx
1.1 builtin extraThing2 yzabcd
1.1 extension myextension: abcdef
1.1 extension yourextension: ghijkl
1.1 extension theirextension: mnopqr

```