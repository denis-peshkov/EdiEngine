namespace EdiEngine.Licensing;

/// <summary>
/// Represents a Cross.EdiEngine license extracted from a JWT token.
/// </summary>
internal class License
{
    /// <summary>
    /// Creates a license from an array of JWT claims.
    /// </summary>
    /// <param name="claims">Array of claims from the decoded JWT.</param>
    internal License(params Claim[] claims)
        : this(new ClaimsPrincipal(new ClaimsIdentity(claims)))
    {
    }

    /// <summary>
    /// Creates a license from a principal with claims (sub_id, user_id, iat, nbf, exp, edition, type).
    /// </summary>
    /// <param name="claims">Principal containing license claims.</param>
    public License(ClaimsPrincipal claims)
    {
        if (Guid.TryParse(claims.FindFirst("sub_id")?.Value, out var subscriptionId))
        {
            SubscriptionId = subscriptionId;
        }

        if (Guid.TryParse(claims.FindFirst("user_id")?.Value, out var userId))
        {
            UserId = userId;
        }

        if (long.TryParse(claims.FindFirst("iat")?.Value, out var iat))
        {
            var startedAt = DateTimeOffset.FromUnixTimeSeconds(iat);
            StartDate = startedAt;
        }

        if (long.TryParse(claims.FindFirst("nbf")?.Value, out var nbf))
        {
            var notBefore = DateTimeOffset.FromUnixTimeSeconds(nbf);
            NotBeforeDate = notBefore;
        }

        if (long.TryParse(claims.FindFirst("exp")?.Value, out var exp))
        {
            var expiredAt = DateTimeOffset.FromUnixTimeSeconds(exp);
            ExpirationDate = expiredAt;
        }

        if (Enum.TryParse<EditionEnum>(claims.FindFirst("edition")?.Value, out var edition))
        {
            Edition = edition;
        }

        if (Enum.TryParse<ProductTypeEnum>(claims.FindFirst("type")?.Value, out var productType))
        {
            ProductType = productType;
        }

        IsConfigured = SubscriptionId != null
                       && UserId != null
                       && NotBeforeDate != null
                       && StartDate != null
                       && ExpirationDate != null
                       && Edition != null
                       && ProductType != null;
    }

    /// <summary>User ID from claim user_id.</summary>
    public Guid? UserId { get; }

    /// <summary>Subscription ID from claim sub_id.</summary>
    public Guid? SubscriptionId { get; }

    /// <summary>License start date from claim iat.</summary>
    public DateTimeOffset? StartDate { get; }

    /// <summary>Not-before date from claim nbf.</summary>
    public DateTimeOffset? NotBeforeDate { get; }

    /// <summary>License expiration date from claim exp.</summary>
    public DateTimeOffset? ExpirationDate { get; }

    /// <summary>Product edition from claim edition.</summary>
    public EditionEnum? Edition { get; }

    /// <summary>Product type from claim type.</summary>
    public ProductTypeEnum? ProductType { get; }

    /// <summary>Indicates that the license is fully configured (all required claims are present).</summary>
    public bool IsConfigured { get; }
}
