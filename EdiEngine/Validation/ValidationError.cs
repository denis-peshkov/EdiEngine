namespace EdiEngine.Validation;

public class ValidationError
{
    [JsonPropertyOrder(1)]
    public int? SegmentPos { get; set; }

    [JsonPropertyOrder(2)]
    public string SegmentName { get; set; }

    [JsonPropertyOrder(3)]
    public int? ElementPos { get; set; }

    [JsonPropertyOrder(4)]
    public string Message { get; set; }
}