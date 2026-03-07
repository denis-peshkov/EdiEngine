namespace EdiEngine.Samples.Samples;

internal static class Sample210
{
    public static void Save210Edi()
    {
        Common.EnsureOutputDirectory();

        var map = new M_210();
        var trans = CreateSampleFreightInvoice(map);

        string edi = Common.WriteEdiEnvelope(trans, "IN");
        var path = Common.GetPath("210_freight_invoice.edi");
        File.WriteAllText(path, edi);

        Console.WriteLine("Saved 210 EDI to " + path);
    }

    public static void Read210EdiToJson()
    {
        Common.EnsureOutputDirectory();

        var ediPath = Common.GetPath("210_freight_invoice.edi");
        if (!File.Exists(ediPath))
        {
            Console.WriteLine("210 EDI file not found: " + ediPath);
            return;
        }

        string edi = File.ReadAllText(ediPath);
        var ediReader = new EdiDataReader();
        EdiBatch batch = ediReader.FromString(edi);

        var jsonWriter = new JsonDataWriter();
        string json = jsonWriter.WriteToString(batch);

        var jsonPath = Common.GetPath("210_freight_invoice.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 210 JSON to " + jsonPath);
    }

    public static void Read210JsonToXml()
    {
        Common.EnsureOutputDirectory();

        var jsonPath = Common.GetPath("210_freight_invoice.json");
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("210 JSON file not found: " + jsonPath);
            return;
        }

        string json = File.ReadAllText(jsonPath);
        var map = new M_210();
        var reader = new JsonMapReader(map);
        EdiTrans trans = reader.ReadToEnd(json);

        string xml = Common.WriteTransToXml(trans, "IN");
        var xmlPath = Common.GetPath("210_freight_invoice.xml");
        File.WriteAllText(xmlPath, xml);

        Console.WriteLine("Saved 210 XML to " + xmlPath);
    }

    public static void Read210XmlToJson()
    {
        Common.EnsureOutputDirectory();

        var xmlPath = Common.GetPath("210_freight_invoice.xml");
        if (!File.Exists(xmlPath))
        {
            Console.WriteLine("210 XML file not found: " + xmlPath);
            return;
        }

        string xml = File.ReadAllText(xmlPath);
        var map = new M_210();
        var reader = new XmlMapReader(map);
        EdiTrans trans = reader.ReadToEnd(xml);

        string json = Common.WriteTransToJson(trans, "IN");
        var jsonPath = Common.GetPath("210_freight_invoice.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 210 JSON (from XML) to " + jsonPath);
    }

    private static EdiTrans CreateSampleFreightInvoice(M_210 map)
    {
        var trans = new EdiTrans(map);

        // B3 - beginning segment for carrier's invoice
        var b3Def = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "B3");
        var b3 = new EdiSegment(b3Def);
        b3.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(b3Def.Content[0], "ABCD"),      // invoice number
            new EdiSimpleDataElement(b3Def.Content[1], "C"),         // shipment method
            new EdiSimpleDataElement(b3Def.Content[2], "CARR01"),    // carrier code
            new EdiSimpleDataElement(b3Def.Content[3], DateTime.Today.ToString("yyyyMMdd")), // invoice date
            new EdiSimpleDataElement(b3Def.Content[8], "123456"),    // pro number
        });
        trans.Content.Add(b3);

        // G62 - dates (pickup / delivery)
        var g62Def = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "G62");
        var g62Pickup = new EdiSegment(g62Def);
        g62Pickup.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(g62Def.Content[0], "86"), // pickup date
            new EdiSimpleDataElement(g62Def.Content[1], DateTime.Today.AddDays(-1).ToString("yyyyMMdd"))
        });
        trans.Content.Add(g62Pickup);

        var g62Delivery = new EdiSegment(g62Def);
        g62Delivery.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(g62Def.Content[0], "17"), // delivery date
            new EdiSimpleDataElement(g62Def.Content[1], DateTime.Today.ToString("yyyyMMdd"))
        });
        trans.Content.Add(g62Delivery);

        // N1 loop - shipper and consignee
        var n1LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_N1");
        trans.Content.Add(CreateN1Party(n1LoopDef, "SH", "Shipper Company", "94", "SHIP01"));
        trans.Content.Add(CreateN1Party(n1LoopDef, "CN", "Consignee Company", "94", "CONS01"));

        // LX loop - one freight line with weight, rate and amount
        var lxLoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_LX");
        var lxLoop = new EdiLoop(lxLoopDef, null);
        trans.Content.Add(lxLoop);

        var lxDef = (MapSegment)Enumerable.First<MapBaseEntity>(lxLoopDef.Content, s => s.Name == "LX");
        var lx = new EdiSegment(lxDef);
        lx.Content.Add(new EdiSimpleDataElement(lxDef.Content[0], "1")); // line number
        lxLoop.Content.Add(lx);

        var l0Def = (MapSegment)Enumerable.First<MapBaseEntity>(lxLoopDef.Content, s => s.Name == "L0");
        var l0 = new EdiSegment(l0Def);
        l0.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(l0Def.Content[0], "1"),        // lading line
            new EdiSimpleDataElement(l0Def.Content[1], "1000"),     // weight
            new EdiSimpleDataElement(l0Def.Content[2], "LB"),       // weight unit
        });
        lxLoop.Content.Add(l0);

        var l1Def = (MapSegment)Enumerable.First<MapBaseEntity>(lxLoopDef.Content, s => s.Name == "L1");
        var l1 = new EdiSegment(l1Def);
        l1.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(l1Def.Content[0], "250.00"),   // charge
            new EdiSimpleDataElement(l1Def.Content[2], "RC"),       // rate/charge basis
        });
        lxLoop.Content.Add(l1);

        var l5Def = (MapSegment)Enumerable.First<MapBaseEntity>(lxLoopDef.Content, s => s.Name == "L5");
        var l5 = new EdiSegment(l5Def);
        l5.Content.Add(new EdiSimpleDataElement(l5Def.Content[0], "FREIGHT CHARGES FOR LTL SHIPMENT"));
        lxLoop.Content.Add(l5);

        return trans;
    }

    private static EdiLoop CreateN1Party(MapLoop n1LoopDef, string entityIdCode, string name, string idCodeQualifier, string idCode)
    {
        var loop = new EdiLoop(n1LoopDef, null);

        var n1Def = (MapSegment)Enumerable.First<MapBaseEntity>(n1LoopDef.Content, s => s.Name == "N1");
        var n1 = new EdiSegment(n1Def);
        n1.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(n1Def.Content[0], entityIdCode),
            new EdiSimpleDataElement(n1Def.Content[1], name),
            new EdiSimpleDataElement(n1Def.Content[2], idCodeQualifier),
            new EdiSimpleDataElement(n1Def.Content[3], idCode)
        });
        loop.Content.Add(n1);

        return loop;
    }
}

