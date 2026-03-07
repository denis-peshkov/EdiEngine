namespace EdiEngine.Samples.Samples;

internal static class Sample322
{
    public static void Save322Edi()
    {
        Common.EnsureOutputDirectory();

        var map = new M_322();
        var trans = CreateSampleTerminalActivity(map);

        string edi = Common.WriteEdiEnvelope(trans, "IW");
        var path = Common.GetPath("322_terminal.edi");
        File.WriteAllText(path, edi);

        Console.WriteLine("Saved 322 terminal EDI to " + path);
    }

    public static void Read322EdiToJson()
    {
        Common.EnsureOutputDirectory();

        var ediPath = Common.GetPath("322_terminal.edi");
        if (!File.Exists(ediPath))
        {
            Console.WriteLine("322 EDI file not found: " + ediPath);
            return;
        }

        string edi = File.ReadAllText(ediPath);
        var ediReader = new EdiDataReader();
        EdiBatch batch = ediReader.FromString(edi);

        var jsonWriter = new JsonDataWriter();
        string json = jsonWriter.WriteToString(batch);

        var jsonPath = Common.GetPath("322_terminal.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 322 JSON to " + jsonPath);
    }

    public static void Read322JsonToXml()
    {
        Common.EnsureOutputDirectory();

        var jsonPath = Common.GetPath("322_terminal.json");
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("322 JSON file not found: " + jsonPath);
            return;
        }

        string json = File.ReadAllText(jsonPath);
        var map = new M_322();
        var reader = new JsonMapReader(map);
        EdiTrans trans = reader.ReadToEnd(json);

        string xml = Common.WriteTransToXml(trans, "IW");
        var xmlPath = Common.GetPath("322_terminal.xml");
        File.WriteAllText(xmlPath, xml);

        Console.WriteLine("Saved 322 XML to " + xmlPath);
    }

    public static void Read322XmlToJson()
    {
        Common.EnsureOutputDirectory();

        var xmlPath = Common.GetPath("322_terminal.xml");
        if (!File.Exists(xmlPath))
        {
            Console.WriteLine("322 XML file not found: " + xmlPath);
            return;
        }

        string xml = File.ReadAllText(xmlPath);
        var map = new M_322();
        var reader = new XmlMapReader(map);
        EdiTrans trans = reader.ReadToEnd(xml);

        string json = Common.WriteTransToJson(trans, "IW");
        var jsonPath = Common.GetPath("322_terminal.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 322 JSON (from XML) to " + jsonPath);
    }

    private static EdiTrans CreateSampleTerminalActivity(M_322 map)
    {
        var trans = new EdiTrans(map);

        // Shipment status / event
        var q5Def = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "Q5");
        var q5 = new EdiSegment(q5Def);
        q5.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(q5Def.Content[0], "AA"),                  // status code
            new EdiSimpleDataElement(q5Def.Content[1], DateTime.Today.ToString("yyyyMMdd")), // date
            new EdiSimpleDataElement(q5Def.Content[2], "L")                    // time code (local)
        });
        trans.Content.Add(q5);

        // Equipment / container loop
        var n7LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_N7");
        var n7Loop = new EdiLoop(n7LoopDef, null);
        trans.Content.Add(n7Loop);

        var n7Def = (MapSegment)Enumerable.First<MapBaseEntity>(n7LoopDef.Content, s => s.Name == "N7");
        var n7 = new EdiSegment(n7Def);
        n7.Content.Add(new EdiSimpleDataElement(n7Def.Content[0], "CONT1"));   // equipment id
        n7Loop.Content.Add(n7);

        // Cargo details loop
        var l0LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(n7LoopDef.Content, s => s.Name == "L_L0");
        var l0Loop = new EdiLoop(l0LoopDef, null);
        n7Loop.Content.Add(l0Loop);

        var l0Def = (MapSegment)Enumerable.First<MapBaseEntity>(l0LoopDef.Content, s => s.Name == "L0");
        var l0 = new EdiSegment(l0Def);
        l0.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(l0Def.Content[0], "1"),     // number of loads
            new EdiSimpleDataElement(l0Def.Content[1], "100"),   // weight
            new EdiSimpleDataElement(l0Def.Content[2], "KG"),    // weight unit
            new EdiSimpleDataElement(l0Def.Content[3], "2"),     // number of units
            new EdiSimpleDataElement(l0Def.Content[4], "PLT")    // unit of measure
        });
        l0Loop.Content.Add(l0);

        var l5Def = (MapSegment)Enumerable.First<MapBaseEntity>(l0LoopDef.Content, s => s.Name == "L5");
        var l5 = new EdiSegment(l5Def);
        l5.Content.Add(new EdiSimpleDataElement(l5Def.Content[0], "FROZEN FISH FILLETS"));
        l0Loop.Content.Add(l5);

        return trans;
    }
}
