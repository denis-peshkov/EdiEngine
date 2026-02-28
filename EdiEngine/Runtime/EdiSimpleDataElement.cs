namespace EdiEngine.Runtime;

[XmlElement(IgnoreElementRoot = true)]
public class EdiSimpleDataElement : DataElementBase
{
    public EdiSimpleDataElement(MapBaseDataElement definition, string val) : base(definition)
    {
        if (definition != null)
        {
            Type = definition.GetType().Name;
        }
        Val = val;
    }
    [JsonIgnore]
    [XmlIgnore]
    public new MapSimpleDataElement Definition => (MapSimpleDataElement)base.Definition;

    [JsonIgnore]
    [XmlIgnore]
    public string Type { get; }

    [JsonPropertyOrder(1)]
    [JsonPropertyName("E")]
    [XmlProperty(Order = 1, PropertyName = "E")]
    public override string Val { get; }
}