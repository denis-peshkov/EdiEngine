using W05 = EdiEngine.Tests.Segments.W05;

namespace EdiEngine.Tests.Maps;

public class M_940 : MapLoop
{
    public M_940() : base(null)
    {
        Content.AddRange(new MapBaseEntity[] {
            new W05() { ReqDes = RequirementDesignator.Mandatory, MaxOccurs = 1 },
        });
    }
}