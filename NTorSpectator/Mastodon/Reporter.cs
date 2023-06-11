using System.Text;
using NTorSpectator.Services;

namespace NTorSpectator.Mastodon;

/// <summary>
/// Reporter service - publishes report based on tor watch results
/// </summary>
public class Reporter : IReporter
{
    private readonly ILogger<Reporter> _logger;
    private readonly IMastodonClient _mastodonClient;

    public Reporter(ILogger<Reporter> logger, IMastodonClient mastodonClient)
    {
        _logger = logger;
        _mastodonClient = mastodonClient;
    }

    public async Task PublishReport(IReadOnlyCollection<TorWatchResults> watchResults)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Requested {0} sites. Alive: {1}. Down: {2}", watchResults.Count, watchResults.Count(x => x.IsOk), watchResults.Count(x => !x.IsOk));
        await _mastodonClient.Toot(new(sb.ToString()));
        _logger.LogInformation("Posted a new status");
    }
}
