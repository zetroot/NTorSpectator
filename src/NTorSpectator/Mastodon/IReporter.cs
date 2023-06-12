using NTorSpectator.Services;

namespace NTorSpectator.Mastodon;

public interface IReporter
{
    Task PublishReport(IReadOnlyCollection<TorWatchResults> watchResults);
}