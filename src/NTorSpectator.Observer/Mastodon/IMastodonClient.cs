using Refit;

namespace NTorSpectator.Observer.Mastodon;

/// <summary>
/// Refit mastodon api client
/// </summary>
[Headers("Authorization: Bearer")]
public interface IMastodonClient
{
    /// <summary>
    /// Post a new toot
    /// </summary>
    /// <param name="status">Toot to post</param>
    /// <returns></returns>
    [Post("/api/v1/statuses")]
    Task<IApiResponse> Toot([Body]TootRequest status);
}
