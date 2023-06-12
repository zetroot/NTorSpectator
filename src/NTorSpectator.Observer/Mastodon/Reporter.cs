using System.Text;
using Microsoft.Extensions.Options;
using NTorSpectator.Observer.Services;
using NTorSpectator.Services;

namespace NTorSpectator.Observer.Mastodon;

/// <summary>
/// Reporter service - publishes report based on tor watch results
/// </summary>
public class Reporter : IReporter
{
    private readonly ILogger<Reporter> _logger;
    private readonly IMastodonClient _mastodonClient;
    private readonly int _messageLimit;
    private const string TRUNCATED_TAIL = "\n[TRUNCATED]";

    public Reporter(ILogger<Reporter> logger, IMastodonClient mastodonClient, IOptions<MastodonSettings> mastodonSettings)
    {
        _logger = logger;
        _mastodonClient = mastodonClient;
        _messageLimit = mastodonSettings.Value.MessageLimit;
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

            sb.AppendLine().AppendLine().AppendLine("Failed sites:");
            foreach (var failResult in watchResults.Where(x => !x.IsOk))
            {
                sb.AppendFormat("\U0001F4A5 http://{0} ", failResult.Site).AppendLine();
            }
        }

        if (sb.Length > _messageLimit)
        {
            var tailBegin = _messageLimit - TRUNCATED_TAIL.Length;
            var tailLength = sb.Length - tailBegin;
            sb.Remove(tailBegin, tailLength).Append(TRUNCATED_TAIL);
        }
        
        await _mastodonClient.Toot(new(sb.ToString()));
        _logger.LogInformation("Posted a new status");
    }

    public async Task ReportWentDown(string siteUri, DateTime lastSeen)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("\u274c {0} gone down. \nLast seen {1}", siteUri, lastSeen);
        await _mastodonClient.Toot(new(sb.ToString()));
        _logger.LogInformation("Posted a new status about going down");
    }

    public async Task ReportCameUp(string siteUri)
    {
        var sb = new StringBuilder();
        sb.AppendFormat("\u2705 {0} came up! \n\U0001F4A5 \U0001F4A5 \U0001F4A5", siteUri);
        await _mastodonClient.Toot(new(sb.ToString()));
        _logger.LogInformation("Posted a new status about came up site");
    }
}
