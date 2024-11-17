
namespace EdiEngine;

public abstract class DataWriter
{
    public abstract Stream WriteToStream(EdiBatch batch);

    public abstract string WriteToString(EdiBatch batch);
}