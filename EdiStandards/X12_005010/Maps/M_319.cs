namespace EdiEngine.Standards.X12_005010.Maps;

public class M_319 : MapLoop
{
	public M_319() : base(null)
	{
		Content.AddRange(new MapBaseEntity[] {
			new BA2() { ReqDes = RequirementDesignator.Mandatory, MaxOccurs = 1 },
			new CD1() { ReqDes = RequirementDesignator.Mandatory, MaxOccurs = 999 },
		});
	}

}
