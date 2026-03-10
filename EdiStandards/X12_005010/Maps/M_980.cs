namespace EdiEngine.Standards.X12_005010.Maps;

public class M_980 : MapLoop
{
	public M_980() : base(null)
	{
		Content.AddRange(new MapBaseEntity[] {
			new BT1() { ReqDes = RequirementDesignator.Mandatory, MaxOccurs = 10 },
		});
	}

}
