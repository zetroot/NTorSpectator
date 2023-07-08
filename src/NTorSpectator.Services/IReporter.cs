using NTorSpectator.Services.Models;

namespace NTorSpectator.Services;

/// <summary>
/// Mastodon toots sender
/// </summary>
public interface IReporter
{
    /// <summary>
    /// Publish toot with report data
    /// </summary>
    /// <param name="report">Report to publish</param>
    Task PublishReportData(Report report);
}
