namespace EdiEngine.Licensing;

/// <summary>
/// Retrieves and caches the current license by decoding JWT from configuration or environment variable.
/// </summary>
internal class LicenseAccessor
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates an instance for license access.
    /// </summary>
    /// <param name="loggerFactory">Logger factory for decoding error messages.</param>
    public LicenseAccessor(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("Cross.EdiEngine.License");
    }

    private License? _license;
    private readonly object _lock = new();

    /// <summary>
    /// Current license. Loaded and decoded from the key on first access.
    /// </summary>
    public License Current => _license ??= Initialize();

    private License Initialize()
    {
        lock (_lock)
        {
            if (_license != null)
            {
                return _license;
            }

            var key = LicensingExtensions.GetLicenseKey();
            if (key == null)
            {
                return new License();
            }

            var licenseClaims = ValidateKey(key);
            return licenseClaims.Any()
                ? new License(new ClaimsPrincipal(new ClaimsIdentity(licenseClaims)))
                : new License();
        }
    }

    private Claim[] ValidateKey(string licenseKey)
    {
        if (!IsValidJwtFormat(licenseKey))
        {
            _logger.LogError(
                "Invalid Peshkov software license key. The token needs to be in JWS or JWE Compact Serialization Format. " +
                "(JWS): 'EncodedHeader.EncodedPayload.EncodedSignature'. " +
                "(JWE): 'EncodedProtectedHeader.EncodedEncryptedKey.EncodedInitializationVector.EncodedCiphertext.EncodedAuthenticationTag'. " +
                "Please visit https://peshkov.biz to obtain a valid license.");
            return Array.Empty<Claim>();
        }

        var handler = new JsonWebTokenHandler();

        var rsa = new RSAParameters
        {
            Exponent = Convert.FromBase64String("AQAB"),
            Modulus = Convert.FromBase64String("LWWXccoyaqk6RVn1kDNSX6WNJDtuOB2Lpu5Kh1q3ENDzkieia2xDlffpvo14XoI1JJOunY1k11XDg0HfRxVC2FwdcrouCDZKDQp87jvnY2vsxIZVAIYQ5wUetNOD4GVAoLAGYUhc647nyRgasC4ATIxCbH0XKjJZdWwb9BIKK9OCbqcDwHHX3IKK7v0sbiw/OOQQHhUZ7EeiPzZavnu8ZWwA1M4bsk9s/2qc5t+fFC0EWVhuGlV7U3dtwRKJ3/rvqbpo9MHUT4HzsZPMA6+/uNcZhjZLADjKsNrGs7vIDoaizneg1TUyiIiy+0K50C2vs/vbSNiz49JOTcr81RjFw==")
        };

        var key = new RsaSecurityKey(rsa)
        {
            KeyId = "PeshkovSoftwareLicenseKey/bbb13acb59904d89b43b1c85f088ccf9"
        };

        var parms = new TokenValidationParameters
        {
            ValidIssuer = "https://peshkov.biz",
            ValidAudience = "Peshkov software",
            IssuerSigningKey = key,
            ValidateLifetime = false
        };

        var validateResult = handler.ValidateTokenAsync(licenseKey, parms).Result;
        if (!validateResult.IsValid)
        {
            _logger.LogError(validateResult.Exception, "Invalid Peshkov software license key. Please visit https://peshkov.biz to obtain a valid license.");
        }

        return validateResult.ClaimsIdentity?.Claims.ToArray() ?? Array.Empty<Claim>();
    }

    /// <summary>
    /// Validates that the token is in JWS (3 parts) or JWE (5 parts) Compact Serialization Format.
    /// </summary>
    private static bool IsValidJwtFormat(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.');
        // JWS: EncodedHeader.EncodedPayload.EncodedSignature
        // JWE: EncodedProtectedHeader.EncodedEncryptedKey.EncodedInitializationVector.EncodedCiphertext.EncodedAuthenticationTag
        return parts.Length == 3 || parts.Length == 5;
    }
}
