namespace EdiEngine.Licensing;

/// <summary>
/// Validates the license: checks expiration date, product type, and logs the result.
/// </summary>
internal class LicenseValidator
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a license validator instance.
    /// </summary>
    /// <param name="loggerFactory">Logger factory for license-related messages.</param>
    public LicenseValidator(ILoggerFactory loggerFactory)
        => _logger = loggerFactory.CreateLogger("Cross.EdiEngine.License");

    /// <summary>
    /// Validates the license: checks key presence, expiration date, and Cross.EdiEngine product eligibility.
    /// </summary>
    /// <param name="license">License to validate.</param>
    public void Validate(License license)
    {
        _logger.LogDebug("The Peshkov software license key details: {@License}", license);

        var errors = new List<string>();

        if (license is not { IsConfigured: true })
        {
            var message = "You do not have a valid license key for the Peshkov software Cross.EdiEngine. " +
                          "This is allowed for development and testing scenarios. " +
                          "If you are running in production you are required to have a licensed version. " +
                          "Please visit https://peshkov.biz to obtain a valid license.";

            _logger.LogCritical(message);
            return;
        }

        var diff = DateTime.UtcNow.Date.Subtract(license.ExpirationDate!.Value.Date).TotalDays;
        if (diff > 0)
        {
            errors.Add($"Your license for the Peshkov software Cross.EdiEngine expired {diff} days ago.");
        }

        if (license.ProductType!.Value != ProductTypeEnum.Cross_EdiEngine
            && license.ProductType.Value != ProductTypeEnum.Bundle)
        {
            errors.Add("Your Peshkov software license does not include Cross.EdiEngine.");
        }

        if (errors.Count > 0)
        {
            foreach (var err in errors)
            {
                _logger.LogError(err);
            }

            _logger.LogCritical("Please visit https://peshkov.biz to obtain a valid license for the Peshkov software Cross.EdiEngine.");
        }
        else
        {
            _logger.LogInformation("You have a valid license key for the Peshkov software {Type} {Edition} edition. The license expires on {LicenseExpiration}.",
                license.ProductType,
                license.Edition,
                license.ExpirationDate);
        }
    }
}
