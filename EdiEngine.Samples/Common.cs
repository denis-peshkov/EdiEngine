namespace EdiEngine.Samples;

internal static class Common
{
    internal const string OutputDirectory = "./../../../SamplesOutput";

    internal static void EnsureOutputDirectory()
    {
        Directory.CreateDirectory(OutputDirectory);
    }

    internal static string GetPath(string fileName)
    {
        return Path.Combine(OutputDirectory, fileName);
    }

    internal static string WriteEdiEnvelope(EdiTrans t, string functionalCode)
    {
        var batch = new EdiBatch();
        var interchange = new EdiInterchange();
        batch.Interchanges.Add(interchange);

        var group = new EdiGroup(functionalCode);
        interchange.Groups.Add(group);
        group.Transactions.Add(t);

        var isaDef = new EdiEngine.Standards.X12_004010.Segments.ISA();
        var ieaDef = new EdiEngine.Standards.X12_004010.Segments.IEA();
        var gsDef = new EdiEngine.Standards.X12_004010.Segments.GS();
        var geDef = new EdiEngine.Standards.X12_004010.Segments.GE();
        var stDef = new EdiEngine.Standards.X12_004010.Segments.ST();
        var seDef = new EdiEngine.Standards.X12_004010.Segments.SE();

        var settings = new EdiDataWriterSettings(
            isaDef,
            ieaDef,
            gsDef,
            geDef,
            stDef,
            seDef,
            "ZZ",
            "SENDER",
            "ZZ",
            "RECEIVER",
            "GSSENDER",
            "GSRECEIVER",
            "00401",
            "004010",
            "T",
            100,
            200,
            false,
            "\r\n",
            "*");

        var writer = new EdiDataWriter(settings);
        return writer.WriteToString(batch);
    }
}
