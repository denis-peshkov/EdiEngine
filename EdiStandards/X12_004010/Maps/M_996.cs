namespace EdiEngine.Standards.X12_004010.Maps;

public class M_996 : MapLoop
{
	public M_996() : base(null)
	{
		Content.AddRange(new MapBaseEntity[] {
			new BGF() { ReqDes = RequirementDesignator.Mandatory, MaxOccurs = 1 },
			new K3() { ReqDes = RequirementDesignator.Mandatory, MaxOccurs = 999999 },
		});
	}

}
