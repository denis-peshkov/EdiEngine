namespace EdiEngine;

public abstract class DataWriter
{
    protected readonly IServiceProvider _serviceProvider;

    public DataWriter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public abstract Stream WriteToStream(EdiBatch batch);

    public abstract string WriteToString(EdiBatch batch);
}
