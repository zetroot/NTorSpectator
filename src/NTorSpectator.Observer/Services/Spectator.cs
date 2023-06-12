using Microsoft.Extensions.Options;
using NTorSpectator.Observer.Mastodon;
using NTorSpectator.Observer.TorIntegration;

namespace NTorSpectator.Observer.Services;

public class Spectator : BackgroundService
{
    private readonly ILogger<Spectator> _logger;
    private readonly TorControlManager _torControl;
    private readonly string _torSitesFile;
    private readonly TimeSpan _cooldown;
    private readonly IReporter _reporter;

    public Spectator(ILogger<Spectator> logger, TorControlManager torControl, IOptions<SpectatorSettings> opts, IReporter reporter)
    {
        _logger = logger;
        _torControl = torControl;
        _reporter = reporter;
        _torSitesFile = opts.Value.SiteList;
        _cooldown = opts.Value.CooldownInterval;
    }

    public bool IsRunning { get; private set; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            IsRunning = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                await Watch();
                await Task.Delay(_cooldown, stoppingToken);
            }
        }
        finally
        {
            IsRunning = false;
        }
    }

    private async Task Watch()
    {
        var sites = await File.ReadAllLinesAsync(_torSitesFile);
        var replies = new List<TorWatchResults>(sites.Length);
        foreach (var site in sites)
        {
            using (_logger.BeginScope(new Dictionary<string, object> { { "HiddenService", site } }))
            {
                replies.Add(await ObserveSite(site));
                _logger.LogDebug("Added one more result");    
            }
        }

        await _reporter.PublishReport(replies);
    }

    private async Task<TorWatchResults> ObserveSite(string site)
    {
        var torReply = await _torControl.HsFetch(site);
        var positive = torReply.Count(x => x.Action == HsDescAction.Received);
        var negative = torReply.Count(x => x.Action == HsDescAction.Failed);
        return new(site, positive, negative);
    }
}

public record TorWatchResults(string Site, int Positive, int Negative)
{
    public bool IsOk => Positive > 0;
}
