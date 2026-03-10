namespace EdiEngine.Tests;

public class TestUtils
{
    private static readonly IServiceProvider s_serviceProvider = CreateServiceProvider();

    private static IServiceProvider CreateServiceProvider()
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddEdiEngine(configuration);
        services.AddLogging(builder => builder.AddProvider(new NUnitLoggerProvider()));
        return services.BuildServiceProvider();
    }

    public static IServiceProvider ServiceProvider => s_serviceProvider;

    public static string ReadResourceStream(string resourceName)
    {
        using (var s = typeof(TestUtils).Assembly.GetManifestResourceStream(resourceName))
        {
            if (s == null)
                throw new InvalidDataException("stream is null");

            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public static string WriteEdiEnvelope(EdiTrans t, string functionalCode)
    {
        //create batch
        EdiBatch b = new EdiBatch();
        b.Interchanges.Add(new EdiInterchange());
        b.Interchanges.First().Groups.Add(new EdiGroup(functionalCode));
        b.Interchanges.First().Groups.First().Transactions.Add(t);

        //Add all service segments
        EdiDataWriterSettings settings = new EdiDataWriterSettings(
            new EdiEngine.Standards.X12_004010.Segments.ISA(),
            new EdiEngine.Standards.X12_004010.Segments.IEA(),
            new EdiEngine.Standards.X12_004010.Segments.GS(),
            new EdiEngine.Standards.X12_004010.Segments.GE(),
            new EdiEngine.Standards.X12_004010.Segments.ST(),
            new EdiEngine.Standards.X12_004010.Segments.SE(),
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

        EdiDataWriter w = new EdiDataWriter(settings, s_serviceProvider);
        return w.WriteToString(b);
    }
}
