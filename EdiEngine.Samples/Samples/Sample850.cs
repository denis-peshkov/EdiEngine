namespace EdiEngine.Samples.Samples;

internal static class Sample850
{
    public static void SavePurchaseOrderEdi()
    {
        Common.EnsureOutputDirectory();

        var map = new M_850();
        var trans = CreateSamplePurchaseOrder(map);

        string edi = Common.WriteEdiEnvelope(trans, "PO");
        var path = Common.GetPath("850_po.edi");
        File.WriteAllText(path, edi);

        Console.WriteLine("Saved 850 EDI to " + path);
    }

    public static void ReadPurchaseOrderEdiToJson()
    {
        Common.EnsureOutputDirectory();

        var ediPath = Common.GetPath("850_po.edi");
        if (!File.Exists(ediPath))
        {
            Console.WriteLine("850 EDI file not found: " + ediPath);
            return;
        }

        string edi = File.ReadAllText(ediPath);
        var ediReader = new EdiDataReader();
        EdiBatch batch = ediReader.FromString(edi);

        var jsonWriter = new JsonDataWriter();
        string json = jsonWriter.WriteToString(batch);

        var jsonPath = Common.GetPath("850_po.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 850 JSON to " + jsonPath);
    }

    internal static EdiTrans CreateSamplePurchaseOrder(M_850 map)
    {
        var trans = new EdiTrans(map);

        var begDef = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "BEG");
        var beg = new EdiSegment(begDef);
        beg.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(begDef.Content[0], "00"),
            new EdiSimpleDataElement(begDef.Content[1], "SA"),
            new EdiSimpleDataElement(begDef.Content[2], "PO-5001"),
            new EdiSimpleDataElement(begDef.Content[4], DateTime.Today.ToString("yyyyMMdd"))
        });
        trans.Content.Add(beg);

        var dtmDef = (MapSegment)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "DTM");
        var dtm = new EdiSegment(dtmDef);
        dtm.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(dtmDef.Content[0], "002"),
            new EdiSimpleDataElement(dtmDef.Content[1], DateTime.Today.AddDays(7).ToString("yyyyMMdd"))
        });
        trans.Content.Add(dtm);

        var n1LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_N1");
        trans.Content.Add(CreateN1Party(
            n1LoopDef,
            "BT",
            "Buyer Company",
            "91",
            "BUYER",
            "1 Buyer Plaza",
            "Seattle",
            "WA",
            "98101"));
        trans.Content.Add(CreateN1Party(
            n1LoopDef,
            "SU",
            "Supplier Inc.",
            "92",
            "SUPPLIER",
            "500 Supplier Ave",
            "Portland",
            "OR",
            "97201"));

        var po1LoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_PO1");
        trans.Content.Add(CreatePo1Line(po1LoopDef, 1, "20", "EA", "15.50", "ITEM-001", "Steel bolts M10"));
        trans.Content.Add(CreatePo1Line(po1LoopDef, 2, "50", "EA", "2.10", "ITEM-002", "Nuts M10"));

        var cttLoopDef = (MapLoop)Enumerable.First<MapBaseEntity>(map.Content, s => s.Name == "L_CTT");
        var cttLoop = new EdiLoop(cttLoopDef, null);
        trans.Content.Add(cttLoop);

        var cttDef = (MapSegment)cttLoopDef.Content.First(s => s.Name == "CTT");
        var ctt = new EdiSegment(cttDef);
        ctt.Content.Add(new EdiSimpleDataElement(cttDef.Content[0], "2"));
        cttLoop.Content.Add(ctt);

        return trans;
    }

    private static EdiLoop CreatePo1Line(
        MapLoop po1LoopDef,
        int lineNo,
        string quantity,
        string uom,
        string price,
        string sku,
        string description)
    {
        var loop = new EdiLoop(po1LoopDef, null);

        var po1Def = (MapSegment)po1LoopDef.Content.First(s => s.Name == "PO1");
        var po1 = new EdiSegment(po1Def);
        po1.Content.AddRange(new[]
        {
            new EdiSimpleDataElement(po1Def.Content[0], lineNo.ToString()),
            new EdiSimpleDataElement(po1Def.Content[1], quantity),
            new EdiSimpleDataElement(po1Def.Content[2], uom),
            new EdiSimpleDataElement(po1Def.Content[3], price),
            new EdiSimpleDataElement(po1Def.Content[5], "BP"),
            new EdiSimpleDataElement(po1Def.Content[6], sku)
        });
        loop.Content.Add(po1);

        var pidLoopDef = (MapLoop)po1LoopDef.Content.First(s => s.Name == "L_PID");
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

