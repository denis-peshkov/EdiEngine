namespace EdiEngine.Samples.Samples;

internal static class Sample810
{
    public static void SaveInvoiceEdi()
    {
        Common.EnsureOutputDirectory();

        var map = new M_810();
        var trans = CreateSampleInvoice(map);

        string edi = Common.WriteEdiEnvelope(trans, "IN");
        var path = Common.GetPath("810_invoice.edi");
        File.WriteAllText(path, edi);

        Console.WriteLine("Saved 810 EDI to " + path);
    }

    public static void ReadInvoiceEdiToJson()
    {
        Common.EnsureOutputDirectory();

        var ediPath = Common.GetPath("810_invoice.edi");
        if (!File.Exists(ediPath))
        {
            Console.WriteLine("810 EDI file not found: " + ediPath);
            return;
        }

        string edi = File.ReadAllText(ediPath);
        var ediReader = new EdiDataReader();
        EdiBatch batch = ediReader.FromString(edi);

        var jsonWriter = new JsonDataWriter();
        string json = jsonWriter.WriteToString(batch);

        var jsonPath = Common.GetPath("810_invoice.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 810 JSON to " + jsonPath);
    }

    internal static EdiTrans CreateSampleInvoice(M_810 map)
    {
        var trans = new EdiTrans(map);

        var bigDef = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "BIG");
        var big = new EdiSegment(bigDef);
        big.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(bigDef.Content[0], DateTime.Today.ToString("yyyyMMdd")),
            new EdiSimpleDataElement(bigDef.Content[1], "INV-1001"),
            new EdiSimpleDataElement(bigDef.Content[3], "PO-2001")
        });
        trans.Content.Add(big);

        var dtmDef = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "DTM");
        var dtm = new EdiSegment(dtmDef);
        dtm.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(dtmDef.Content[0], "003"),
            new EdiSimpleDataElement(dtmDef.Content[1], DateTime.Today.AddDays(30).ToString("yyyyMMdd"))
        });
        trans.Content.Add(dtm);

        var n1LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_N1");
        trans.Content.Add(CreateN1Party(n1LoopDef, "BT", "Buyer Company", "91", "BUYER", null, null, null, null));
        trans.Content.Add(CreateN1Party(n1LoopDef, "ST", "Ship To Warehouse", "91", "WH-01", "123 Warehouse St", "Los Angeles", "CA", "90001"));

        var it1LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_IT1");
        trans.Content.Add(CreateIt1Line(it1LoopDef, 1, "10", "EA", "12.34", "SKU-001", "Blue widgets"));
        trans.Content.Add(CreateIt1Line(it1LoopDef, 2, "5", "EA", "99.99", "SKU-002", "Premium gadget"));

        return trans;
    }

    private static EdiLoop CreateIt1Line(
        MapLoop it1LoopDef,
        int lineNo,
        string quantity,
        string uom,
        string price,
        string sku,
        string description)
    {
        var loop = new EdiLoop(it1LoopDef, null);

        var it1Def = (MapSegment)it1LoopDef.Content.First(s => s.Name == "IT1");
        var it1 = new EdiSegment(it1Def);
        it1.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(it1Def.Content[0], lineNo.ToString()),
            new EdiSimpleDataElement(it1Def.Content[1], quantity),
            new EdiSimpleDataElement(it1Def.Content[2], uom),
            new EdiSimpleDataElement(it1Def.Content[3], price),
            new EdiSimpleDataElement(it1Def.Content[5], "VN"),
            new EdiSimpleDataElement(it1Def.Content[6], sku)
        });
        loop.Content.Add(it1);

        var pidLoopDef = (MapLoop)it1LoopDef.Content.First(s => s.Name == "L_PID");
        var pidLoop = new EdiLoop(pidLoopDef, null);
        loop.Content.Add(pidLoop);

        var pidDef = (MapSegment)pidLoopDef.Content.First(s => s.Name == "PID");
        var pid = new EdiSegment(pidDef);
        pid.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(pidDef.Content[0], "F"),
            new EdiSimpleDataElement(pidDef.Content[4], description)
        });
        pidLoop.Content.Add(pid);

        return loop;
    }

    private static EdiLoop CreateN1Party(
        MapLoop n1LoopDef,
        string entityIdCode,
        string name,
        string? idCodeQualifier,
        string? idCode,
        string? address1,
        string? city,
        string? state,
        string? postalCode)
    {
        var loop = new EdiLoop(n1LoopDef, null);

        var n1Def = (MapSegment)n1LoopDef.Content.First(s => s.Name == "N1");
        var n1 = new EdiSegment(n1Def);
        n1.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(n1Def.Content[0], entityIdCode),
            new EdiSimpleDataElement(n1Def.Content[1], name),
            idCodeQualifier != null ? new EdiSimpleDataElement(n1Def.Content[2], idCodeQualifier) : null,
            idCode != null ? new EdiSimpleDataElement(n1Def.Content[3], idCode) : null
        }.Where(e => e != null)!.Cast<EdiSimpleDataElement>());
        loop.Content.Add(n1);

        if (!string.IsNullOrEmpty(address1) || !string.IsNullOrEmpty(city))
        {
            var n3Def = (MapSegment)n1LoopDef.Content.First(s => s.Name == "N3");
            var n3 = new EdiSegment(n3Def);
            n3.Content.Add(new EdiSimpleDataElement(n3Def.Content[0], address1 ?? string.Empty));
            loop.Content.Add(n3);

            var n4Def = (MapSegment)n1LoopDef.Content.First(s => s.Name == "N4");
            var n4 = new EdiSegment(n4Def);
            n4.Content.AddRange(new[]
            {
                new EdiSimpleDataElement(n4Def.Content[0], city ?? string.Empty),
                new EdiSimpleDataElement(n4Def.Content[1], state ?? string.Empty),
                new EdiSimpleDataElement(n4Def.Content[2], postalCode ?? string.Empty)
            });
            loop.Content.Add(n4);
        }

        return loop;
    }
}
