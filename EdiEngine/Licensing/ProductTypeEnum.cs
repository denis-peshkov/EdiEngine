namespace EdiEngine.Licensing;

/// <summary>
/// Peshkov software product type.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
internal enum ProductTypeEnum
{
    /// <summary>Cross.CQRS.EF</summary>
    Cross_CQRS_EF = 0,

    /// <summary>Cross.CQRS</summary>
    Cross_CQRS = 1,

    /// <summary>Bundle (includes multiple products)</summary>
    Bundle = 2,

    /// <summary>Cross.EdiEngine</summary>
    Cross_EdiEngine = 3,
}
