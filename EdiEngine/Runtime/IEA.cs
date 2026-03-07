namespace EdiEngine.Runtime;

public class IEA : EdiSegment
{
    public IEA(string[] elements) : base(null)
    {
        foreach (string el in elements)
        {
            Content.Add(new EdiSimpleDataElement(null, el));
        }
    }

    public IEA(MapSegment definition, 
        int IEA01_IncludedGroupsCount,
        int IEA02_ControlNumber) : base(definition)
    {
        Content.AddRange(new[] {
            new EdiSimpleDataElement((MapSimpleDataElement)definition.Content[0], IEA01_IncludedGroupsCount.ToString(CultureInfo.InvariantCulture)),
            new EdiSimpleDataElement((MapSimpleDataElement)definition.Content[1], IEA02_ControlNumber.ToString(CultureInfo.InvariantCulture).PadLeft(9, '0'))
        });
    }
}