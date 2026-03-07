namespace EdiEngine.Tests;

using System.Globalization;
using EdiEngine;
using EdiEngine.Standards.X12_004010.Segments;

[TestFixture]
public class NonPrintableCharactersTests
{
    private const string TestDataPrefix = "EdiEngine.Tests.TestData.";

    /// <summary>
    /// All EDI test resources that can be read and written back to EDI/XML/JSON (valid documents with all segment types).
    /// Covers 940, 850, 856, 997 and multiple interchanges/groups so that all possible fields (ISA, GS, ST, SE, IEA, GE and transaction content) are exercised.
    /// </summary>
    private static readonly string[] EdiResources =
    {
        "940.OK.edi",
        "940.2.OK.edi",
        "850.OK.edi",
        "850.Composite.SLN.OK.edi",
        "856.Crossdock.OK.edi",
        "AckTest.edi",
        "MultipleInterchangesAndGroups.edi",
    };
    /// <summary>
    /// Returns the index of the first character that is considered non-printable (e.g. NNBSP, NBSP, control chars except \t \n \r).
    /// Returns -1 if no such character is found.
    /// </summary>
    private static int GetFirstNonPrintableIndex(string s)
    {
        if (string.IsNullOrEmpty(s))
            return -1;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            var cat = Char.GetUnicodeCategory(c);

            // Control characters except tab, newline, carriage return
            if (cat == UnicodeCategory.Control)
            {
                if (c != '\t' && c != '\n' && c != '\r')
                    return i;
                continue;
            }

            // Format category (includes NBSP U+00A0, NNBSP U+202F, and other non-printable space-like chars)
            if (cat == UnicodeCategory.Format)
                return i;

            // DEL
            if (c == 0x7F)
                return i;
        }

        return -1;
    }

    private static void AssertNoNonPrintableCharacters(string output, string formatName)
    {
        int idx = GetFirstNonPrintableIndex(output);
        if (idx >= 0)
        {
            char bad = output[idx];
            Assert.Fail(
                "{0} output contains non-printable character at index {1}: U+{2:X4} (category: {3}). Use InvariantCulture for numbers and dates.",
                formatName,
                idx,
                (int)bad,
                Char.GetUnicodeCategory(bad));
        }
    }

    private static EdiDataWriter CreateEdiDataWriter()
    {
        var settings = new EdiDataWriterSettings(
            new ISA(),
            new IEA(),
            new GS(),
            new GE(),
            new ST(),
            new SE(),
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
        return new EdiDataWriter(settings);
    }

    [Test]
    [TestCaseSource(nameof(EdiResources))]
    public void NonPrintable_EdiXmlJsonOutput_FromEdiResource_ContainsNoNonPrintableCharacters(string resourceFileName)
    {
        string resourceName = TestDataPrefix + resourceFileName;
        using (Stream s = GetType().Assembly.GetManifestResourceStream(resourceName))
        {
            Assert.IsNotNull(s, "Missing resource: " + resourceName);
            var reader = new EdiDataReader();
            EdiBatch b = reader.FromStream(s);

            var ediWriter = CreateEdiDataWriter();
            string edi = ediWriter.WriteToString(b);
            AssertNoNonPrintableCharacters(edi, "EDI (" + resourceFileName + ")");

            var xmlWriter = new XmlDataWriter();
            string xml = xmlWriter.WriteToString(b);
            AssertNoNonPrintableCharacters(xml, "XML (" + resourceFileName + ")");

            var jsonWriter = new JsonDataWriter();
            string json = jsonWriter.WriteToString(b);
            AssertNoNonPrintableCharacters(json, "JSON (" + resourceFileName + ")");
        }
    }

    [Test]
    public void NonPrintable_EdiOutput_FromWrittenEnvelope_940_ContainsNoNonPrintableCharacters()
    {
        string json = TestUtils.ReadResourceStream(TestDataPrefix + "940.OK.json");
        var map = new M_940();
        var jsonReader = new JsonMapReader(map);
        EdiTrans t = jsonReader.ReadToEnd(json);

        string edi = TestUtils.WriteEdiEnvelope(t, "SH");
        AssertNoNonPrintableCharacters(edi, "EDI envelope (940)");
    }

    [Test]
    public void NonPrintable_EdiOutput_FromWrittenEnvelope_850_ContainsNoNonPrintableCharacters()
    {
        string json = TestUtils.ReadResourceStream(TestDataPrefix + "850.Composite.SLN.OK.json");
        var map = new M_850();
        var jsonReader = new JsonMapReader(map);
        EdiTrans t = jsonReader.ReadToEnd(json);

        string edi = TestUtils.WriteEdiEnvelope(t, "PO");
        AssertNoNonPrintableCharacters(edi, "EDI envelope (850)");
    }

    [Test]
    public void NonPrintable_EdiOutput_FromWrittenEnvelope_856_ContainsNoNonPrintableCharacters()
    {
        string json = TestUtils.ReadResourceStream(TestDataPrefix + "856.Crossdock.OK.json");
        var map = new M_856();
        var jsonReader = new JsonMapReader(map);
        EdiTrans t = jsonReader.ReadToEnd(json);

        string edi = TestUtils.WriteEdiEnvelope(t, "SH");
        AssertNoNonPrintableCharacters(edi, "EDI envelope (856)");
    }
}
