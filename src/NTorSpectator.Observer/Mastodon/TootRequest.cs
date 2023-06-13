using System.Text.Json.Serialization;

namespace NTorSpectator.Observer.Mastodon;

/// <summary>
/// Request model to create a new toot
/// </summary>
/// <param name="Text">Text to toot</param>
public record TootRequest([property: JsonPropertyName("status")] string Text);