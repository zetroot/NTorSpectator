namespace NTorSpectator.Services;

/// <summary>
/// Interface for sites catalogue
/// </summary>
public interface ISitesCatalogue
{
    /// <summary>
    /// Add site to database if it does not exists
    /// </summary>
    /// <param name="siteUri">onion site uri</param>
    /// <returns>true if the site was added, otherwise - false</returns>
    Task<bool> AddIfNotExists(string siteUri);
}
