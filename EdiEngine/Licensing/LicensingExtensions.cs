namespace EdiEngine.Licensing;

/// <summary>
/// Configuration for Cross.EdiEngine.
/// </summary>
public static class LicensingExtensions
{
    private const string LicenseKeyEnvironmentVariable = "CROSS_EDIENGINE_LICENSE_KEY";
    private const string ConfigKeyEdiEngine = "EdiEngine:LicenseKey";

    private static IConfiguration _configuration;

    /// <summary>
    /// Registers Cross.EdiEngine services for licensing in DI.
    /// Registers <see cref="LicenseAccessor"/>, <see cref="LicenseValidator"/>, and logging.
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="configuration">Application configuration for reading the license key.</param>
    /// <returns>Service collection for method chaining.</returns>
    public static IServiceCollection AddEdiEngine(this IServiceCollection services, IConfiguration configuration)
    {
        _configuration = configuration;

        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<LicenseAccessor>();
        services.AddSingleton<LicenseValidator>();

        return services;
    }

    /// <summary>
    /// Returns the license key from IConfiguration or environment variable CROSS_EDIENGINE_LICENSE_KEY.
    /// </summary>
    /// <returns>License key in JWT format, or null if not set.</returns>
    internal static string? GetLicenseKey()
        => _configuration[ConfigKeyEdiEngine]
           ?? _configuration[LicenseKeyEnvironmentVariable]
           ?? Environment.GetEnvironmentVariable(LicenseKeyEnvironmentVariable);

    /// <summary>
    /// Validates the license. Called on each library usage.
    /// </summary>
    /// <param name="serviceProvider">DI service provider containing <see cref="LicenseAccessor"/> and <see cref="LicenseValidator"/>.</param>
    internal static void CheckLicense(this IServiceProvider serviceProvider)
    {
        var licenseAccessor = serviceProvider.GetRequiredService<LicenseAccessor>();
        var licenseValidator = serviceProvider.GetRequiredService<LicenseValidator>();
        var license = licenseAccessor.Current;
        licenseValidator.Validate(license);
    }
}
