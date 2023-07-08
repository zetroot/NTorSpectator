using Microsoft.EntityFrameworkCore;
using NTorSpectator.Services.Models;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Database.Repositories;

internal class ObservesRepository : IObservesRepository
{
    private readonly DataContext _context;
    public ObservesRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Observation?> GetLastObservationForSite(string siteUri)
    {
        var observation = await _context.Observations
            .Include(x => x.Site)
            .Where(x => x.Site.SiteUri == siteUri)
            .OrderByDescending(x => x.ObservedAt)
            .FirstOrDefaultAsync();
        
        return observation is null ?
            null :
            new Observation { ObservedAt = observation.ObservedAt, IsAvailable = observation.IsAvailable, SiteUri = observation.Site.SiteUri};
    }

    public async Task AddNewObservation(string siteUri, bool isAvailable, DateTime timestamp)
    {
        var siteId = await _context.Sites
            .Where(x => x.SiteUri == siteUri)
            .Select(x => x.Id)
            .SingleAsync();

        var addingObservation = new Models.Observation { ObservedAt = timestamp, SiteId = siteId, IsAvailable = isAvailable };
        _context.Observations.Add(addingObservation);
        await _context.SaveChangesAsync();
    }
}
