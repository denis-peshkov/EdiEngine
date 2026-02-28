namespace EdiEngine;

public class JsonDataWriter : DataWriter
{
    public override Stream WriteToStream(EdiBatch batch)
    {
        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, batch);
        stream.Position = 0;
        return stream;
    }

    public override string WriteToString(EdiBatch batch)
    {
        return JsonSerializer.Serialize(batch);
    }
}