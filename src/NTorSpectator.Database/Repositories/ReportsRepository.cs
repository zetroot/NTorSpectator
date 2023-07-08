using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NTorSpectator.Database.Models;
using NTorSpectator.Services.Models;
using NTorSpectator.Services.Persistent;

namespace NTorSpectator.Database.Repositories;

internal class ReportsRepository : IReportsRepository
{
    private readonly DataContext _context;
    private readonly ILogger<ReportsRepository> _logger;

    public ReportsRepository(DataContext context, ILogger<ReportsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SaveReport(Report report)
    {
        var siteList = await _context.Sites.ToListAsync();
        var reportDal = new ObservationReport();
        foreach (var observation in report.Observations)
        {
            var site = siteList.SingleOrDefault(x => x.SiteUri == observation.SiteUri);
            if (site is null)
            {
                _logger.LogWarning("Site is not found in database, won't save observation for uri {SiteUri}", observation.SiteUri);
                continue;
            }
            reportDal.Observations.Add(new(){Site = site, ObservedAt = observation.ObservedAt, IsAvailable = observation.IsAvailable});
        }

        foreach (var siteEvent in report.Events)
        {
            var site = siteList.SingleOrDefault(x => x.SiteUri == siteEvent.SiteUri);
            if (site is null)
            {
                _logger.LogWarning("Site is not found in database, won't save event for uri {SiteUri}", siteEvent.SiteUri);
                continue;
            }
            reportDal.Events.Add(new(){Site = site, OccuredAt = siteEvent.OccuredAt, EventType = (int)siteEvent.Kind});
        }

        _context.Reports.Add(reportDal);
        await _context.SaveChangesAsync();
    }
}
