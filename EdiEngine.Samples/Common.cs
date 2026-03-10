using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EdiEngine.Samples;

internal static class Common
{
    internal const string OutputDirectory = "./../../../SamplesOutput";

    private static readonly IServiceProvider s_serviceProvider = CreateServiceProvider();

    private static IServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddEdiEngine(configuration);
        return services.BuildServiceProvider();
    }

    internal static IServiceProvider ServiceProvider => s_serviceProvider;

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
        var batch = BuildBatchFromTrans(t, functionalCode);

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

        var ediWriter = new EdiDataWriter(settings, s_serviceProvider);
        return ediWriter.WriteToString(batch);
    }

    internal static string WriteTransToJson(EdiTrans t, string functionalCode)
    {
        var batch = BuildBatchFromTrans(t, functionalCode);
        var jsonWriter = new JsonDataWriter(s_serviceProvider);
        return jsonWriter.WriteToString(batch);
    }

    internal static string WriteTransToXml(EdiTrans t, string functionalCode)
    {
        var batch = BuildBatchFromTrans(t, functionalCode);
        var xmlWriter = new XmlDataWriter(s_serviceProvider);
        return xmlWriter.WriteToString(batch);
    }

    private static EdiBatch BuildBatchFromTrans(EdiTrans t, string functionalCode)
    {
        var batch = new EdiBatch();
        var interchange = new EdiInterchange();
        batch.Interchanges.Add(interchange);

        var group = new EdiGroup(functionalCode);
        interchange.Groups.Add(group);
        group.Transactions.Add(t);

        return batch;
    }
}
