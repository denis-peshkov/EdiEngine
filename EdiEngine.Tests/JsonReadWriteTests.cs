namespace EdiEngine.Tests;

[TestFixture]
public class JsonReadWriteTests
{
    [Test]
    public void JsonReadWrite_JsonSerializationTest()
    {
        using (Stream s = GetType().Assembly.GetManifestResourceStream("EdiEngine.Tests.TestData.940.OK.edi"))
        {
            EdiDataReader r = new EdiDataReader(TestUtils.ServiceProvider);
            EdiBatch b = r.FromStream(s);

            //Write Json
            //check no exception
            JsonSerializer.Serialize(b);
            JsonSerializer.Serialize(b.Interchanges[0].Groups[0].Transactions[0]);

            //or use writer to write to string or stream
            JsonDataWriter w = new JsonDataWriter(TestUtils.ServiceProvider);
            string str = w.WriteToString(b);
            Stream stream = w.WriteToStream(b);

            Assert.IsNotNull(str);

            Assert.IsNotNull(stream);
            Assert.AreEqual(0, stream.Position);
            Assert.IsTrue(stream.CanRead);

            Assert.AreEqual(str.Length, stream.Length);
        }
    }

    [Test]
    public void JsonReadWrite_DeserializeJsonOK()
    {
        string json = TestUtils.ReadResourceStream("EdiEngine.Tests.TestData.940.OK.json");

        M_940 map = new M_940();
        JsonMapReader r = new JsonMapReader(map);

        EdiTrans t = r.ReadToEnd(json);

        Assert.AreEqual(0, t.ValidationErrors.Count);
    }


    [Test]
    public void JsonReadWrite_DeserializeJsonWithValidationErrors()
    {
        string json = TestUtils.ReadResourceStream("EdiEngine.Tests.TestData.940.ERR.json");

        M_940 map = new M_940();
        JsonMapReader r = new JsonMapReader(map);

        EdiTrans t = r.ReadToEnd(json);

        Assert.AreEqual(2, t.ValidationErrors.Count);
    }

    [Test]
    public void JsonReadWrite_JsonSerializationHlLoopTest()
    {
        using (Stream s = GetType().Assembly.GetManifestResourceStream("EdiEngine.Tests.TestData.856.Crossdock.OK.edi"))
        {
            EdiDataReader r = new EdiDataReader(TestUtils.ServiceProvider);
            EdiBatch b = r.FromStream(s);

            JsonDataWriter jsonWriter = new JsonDataWriter(TestUtils.ServiceProvider);
            jsonWriter.WriteToString(b);
        }
    }

    [Test]
    public void JsonReadWrite_DeserializeJsonHlLoopOk()
    {
        string json = TestUtils.ReadResourceStream("EdiEngine.Tests.TestData.856.Crossdock.OK.json");

        M_856 map = new M_856();
        JsonMapReader r = new JsonMapReader(map);

        EdiTrans t = r.ReadToEnd(json);

        Assert.AreEqual(0, t.ValidationErrors.Count);

        string edi = TestUtils.WriteEdiEnvelope(t, "SH");
        Assert.IsNotNull(edi);
        Assert.IsTrue(edi.Contains("ST"));
        Assert.IsTrue(edi.Contains("SE"));
    }

    [Test]
    public void JsonReadWrite_SerializeComposite()
    {
        using (Stream s = GetType().Assembly.GetManifestResourceStream("EdiEngine.Tests.TestData.850.Composite.SLN.OK.edi"))
        {
            EdiDataReader r = new EdiDataReader(TestUtils.ServiceProvider);
            EdiBatch b = r.FromStream(s);

            JsonDataWriter jsonWriter = new JsonDataWriter(TestUtils.ServiceProvider);
            jsonWriter.WriteToString(b);
        }
    }

    [Test]
    public void JsonReadWrite_DeserializeComposite()
    {
        string json = TestUtils.ReadResourceStream("EdiEngine.Tests.TestData.850.Composite.SLN.OK.json");

        M_850 map = new M_850();
        JsonMapReader r = new JsonMapReader(map);

        EdiTrans t = r.ReadToEnd(json);

        Assert.AreEqual(0, t.ValidationErrors.Count);

        var lPo1 = (EdiLoop)t.Content.First(c => c is EdiLoop);
        var lSln = (EdiLoop)lPo1.Content.First(c => c is EdiLoop && ((EdiLoop)c).Name == "L_SLN");
        var sln = (EdiSegment)lSln.Content.First();
        Assert.IsTrue(sln.Content[4] is EdiCompositeDataElement);
        Assert.AreEqual(6, ((EdiCompositeDataElement)sln.Content[4]).Content.Count);
    }
}
