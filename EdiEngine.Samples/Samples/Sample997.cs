namespace EdiEngine.Samples.Samples;

internal static class Sample997
{
    public static void Save997Edi()
    {
        Common.EnsureOutputDirectory();

        var map = new M_850();
        var trans = Sample850.CreateSamplePurchaseOrder(map);

        string poEdi = Common.WriteEdiEnvelope(trans, "PO");
        var poPath = Common.GetPath("850_for_997.edi");
        File.WriteAllText(poPath, poEdi);

        var ediReader = new EdiDataReader();
        EdiBatch original = ediReader.FromString(poEdi);

        var ackSettings = new AckBuilderSettings(
            AckValidationErrorBehavour.AcceptAll,
            alwaysGenerateAk2Loop: true,
            isaFirstControlNumber: 1,
            gsFirstControlNumber: 1);

        var ackBuilder = new AckBuilder(ackSettings);
        string ackEdi = ackBuilder.WriteToString(original);

        var ackPath = Common.GetPath("997_ack.edi");
        File.WriteAllText(ackPath, ackEdi);

        Console.WriteLine("Saved 850 for 997 to " + poPath);
        Console.WriteLine("Saved 997 Ack EDI to " + ackPath);
    }

    public static void Read997EdiToJson()
    {
        Common.EnsureOutputDirectory();

        var ackPath = Common.GetPath("997_ack.edi");
        if (!File.Exists(ackPath))
        {
            Console.WriteLine("997 EDI file not found: " + ackPath);
            return;
        }

        string ackEdi = File.ReadAllText(ackPath);
        var ediReader = new EdiDataReader();
        EdiBatch ackBatch = ediReader.FromString(ackEdi);

        var jsonWriter = new JsonDataWriter();
        string json = jsonWriter.WriteToString(ackBatch);

        var jsonPath = Common.GetPath("997_ack.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 997 Ack JSON to " + jsonPath);
    }

    public static void Read997JsonToXml()
    {
        Common.EnsureOutputDirectory();

        var jsonPath = Common.GetPath("997_ack.json");
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine("997 JSON file not found: " + jsonPath);
            return;
        }

        string json = File.ReadAllText(jsonPath);
        var map = new M_997();
        var reader = new JsonMapReader(map);
        EdiTrans trans = reader.ReadToEnd(json);

        string xml = Common.WriteTransToXml(trans, "FA");
        var xmlPath = Common.GetPath("997_ack.xml");
        File.WriteAllText(xmlPath, xml);

        Console.WriteLine("Saved 997 XML to " + xmlPath);
    }

    public static void Read997XmlToJson()
    {
        Common.EnsureOutputDirectory();

        var xmlPath = Common.GetPath("997_ack.xml");
        if (!File.Exists(xmlPath))
        {
            Console.WriteLine("997 XML file not found: " + xmlPath);
            return;
        }

        string xml = File.ReadAllText(xmlPath);
        var map = new M_997();
        var reader = new XmlMapReader(map);
        EdiTrans trans = reader.ReadToEnd(xml);

        string json = Common.WriteTransToJson(trans, "FA");
        var jsonPath = Common.GetPath("997_ack.json");
        File.WriteAllText(jsonPath, json);

        Console.WriteLine("Saved 997 JSON (from XML) to " + jsonPath);
    }
}
