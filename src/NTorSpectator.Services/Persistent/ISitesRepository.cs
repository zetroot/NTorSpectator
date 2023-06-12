using NTorSpectator.Services.Models;

namespace NTorSpectator.Services.Persistent;

/// <summary>
/// Repository to access observable sites collection
/// </summary>
public interface ISitesRepository
{
    /// <summary>
    /// Dump all sites from the database
    /// </summary>
    /// <returns>collection of sites stored in the databse</returns>
    Task<IReadOnlyCollection<Site>> GetAllSites();  
    
    /// <summary>
    /// does this site exists in the repository
    /// </summary>
    /// <param name="siteUri">Uri for the site</param>
    /// <returns>true - if we have such site, false otherwise</returns>
    Task<bool> Exists(string siteUri);

    /// <summary>
    /// Add new site in database store
    /// </summary>
    /// <param name="siteUri">onion site uri</param>
    Task AddNewSite(string siteUri);
}
