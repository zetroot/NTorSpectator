using Microsoft.EntityFrameworkCore;
using NTorSpectator.Services.Models;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Database.Repositories;

internal class SitesRepository : ISitesRepository
{
    private readonly DataContext _context;
    public SitesRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Site>> GetAllSites()
    {
        var sites = await _context.Sites.ToListAsync();
        return sites.Select(x => new Site { SiteUri = x.SiteUri }).ToList();
    }

    public Task<bool> Exists(string siteUri) => _context.Sites.AnyAsync(x => string.Equals(x.SiteUri, siteUri, StringComparison.OrdinalIgnoreCase));

    public async Task AddNewSite(string siteUri)
    {
        var addingSite = new Models.Site { SiteUri = siteUri };
        _context.Add(addingSite);
        await _context.SaveChangesAsync();
    }
}
