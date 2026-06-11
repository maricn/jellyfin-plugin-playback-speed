using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.PlaybackSpeed.Configuration;

/// <summary>
/// Playback Speed plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        Enabled = true;
        ShowFloatingControl = true;
        DefaultSpeed = 1.0;
        AvailableSpeeds = "0.75,1,1.25,1.5,1.75,2,2.5,3";
    }

    /// <summary>
    /// Gets or sets a value indicating whether playback speed controls are enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show the floating speed control.
    /// </summary>
    public bool ShowFloatingControl { get; set; }

    /// <summary>
    /// Gets or sets the initial playback speed.
    /// </summary>
    public double DefaultSpeed { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of selectable speeds.
    /// </summary>
    public string AvailableSpeeds { get; set; }
}
