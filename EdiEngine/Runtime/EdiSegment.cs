namespace EdiEngine.Runtime;

public class EdiSegment : EdiBaseEntity
{
    public EdiSegment(MapBaseEntity definition) : base(definition)
    {
        Content = new List<DataElementBase>();
    }

    [JsonPropertyOrder(0)]
    [XmlIgnore]
    public override string Type => "S";

    [JsonPropertyOrder(10)]
    [XmlProperty(Order = 10)]
    public List<DataElementBase> Content { get; }

    public override string ToString()
    {
        var res = Content.Aggregate(string.Empty, (curr, next) => curr + "*" + next.ToString());
        return $"{Name}*{res.Remove(0, 1)}";
    }
}