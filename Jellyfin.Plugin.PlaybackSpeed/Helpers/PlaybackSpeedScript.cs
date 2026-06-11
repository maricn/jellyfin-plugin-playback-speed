namespace Jellyfin.Plugin.PlaybackSpeed.Helpers;

/// <summary>
/// Builds the Jellyfin Web script injection block.
/// </summary>
public static class PlaybackSpeedScript
{
    /// <summary>
    /// Start marker for idempotent injection.
    /// </summary>
    public const string StartComment = "<!-- BEGIN Playback Speed Plugin -->";

    /// <summary>
    /// End marker for idempotent injection.
    /// </summary>
    public const string EndComment = "<!-- END Playback Speed Plugin -->";

    /// <summary>
    /// Builds the script block inserted into Jellyfin Web.
    /// </summary>
    /// <returns>The HTML script block.</returns>
    public static string BuildInjectionBlock()
    {
        var cacheBust = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"""
            {StartComment}
            <script defer src="../PlaybackSpeed/client.js?v={cacheBust}"></script>
            {EndComment}
            """;
    }
}
