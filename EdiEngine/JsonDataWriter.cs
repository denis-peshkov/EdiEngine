namespace EdiEngine;

public class JsonDataWriter : DataWriter
{
    public JsonDataWriter(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override Stream WriteToStream(EdiBatch batch)
    {
        _serviceProvider.CheckLicense();

        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, batch);
        stream.Position = 0;
        return stream;
    }

    public override string WriteToString(EdiBatch batch)
    {
        _serviceProvider.CheckLicense();

        return JsonSerializer.Serialize(batch);
    }
}
