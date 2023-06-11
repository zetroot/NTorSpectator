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
        if (watchResults.All(x => x.IsOk))
        {
            sb.AppendFormat("\u2705 All sites up! {0} of {1}", watchResults.Count(x => x.IsOk), watchResults.Count);
        }
        else
        {
            sb.AppendFormat("\u26a0\ufe0f Requested {0} sites", watchResults.Count)
                .AppendLine()
                .AppendFormat("  \u2705 Alive: {0}",  watchResults.Count(x => x.IsOk))
                .AppendLine()
                .AppendFormat("  \u274c Down: {0}", watchResults.Count(x => !x.IsOk));

            sb.AppendLine().AppendLine();
            foreach (var failResult in watchResults.Where(x => !x.IsOk))
            {
                sb.AppendFormat("  -\U0001F4A5 {0} not found", failResult.Site).AppendLine();
            }
        }
        await _mastodonClient.Toot(new(sb.ToString()));
        _logger.LogInformation("Posted a new status");
    }
}
