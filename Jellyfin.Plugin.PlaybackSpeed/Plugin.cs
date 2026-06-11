using System.Globalization;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.PlaybackSpeed.Configuration;
using Jellyfin.Plugin.PlaybackSpeed.Helpers;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaybackSpeed;

/// <summary>
/// Playback Speed plugin.
/// </summary>
public partial class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly IApplicationPaths _applicationPaths;
    private readonly ILogger<Plugin> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Application paths.</param>
    /// <param name="xmlSerializer">XML serializer.</param>
    /// <param name="logger">Logger.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILogger<Plugin> logger)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        _applicationPaths = applicationPaths;
        _logger = logger;
    }

    /// <inheritdoc />
    public override string Name => "Playback Speed";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("e8d45e34-b36a-44e6-aecb-b44a9ff6be20");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets Jellyfin Web's index file path.
    /// </summary>
    public string IndexHtmlPath => Path.Combine(_applicationPaths.WebPath, "index.html");

    /// <summary>
    /// Injects the plugin script tag into Jellyfin Web.
    /// </summary>
    public void InjectScript()
    {
        if (!File.Exists(IndexHtmlPath))
        {
            LogIndexNotFound(_logger, IndexHtmlPath);
            return;
        }

        try
        {
            var content = File.ReadAllText(IndexHtmlPath);
            content = RemoveInjectionBlock(content);

            if (!content.Contains("</body>", StringComparison.OrdinalIgnoreCase))
            {
                LogBodyTagNotFound(_logger, IndexHtmlPath);
                return;
            }

            content = Regex.Replace(
                content,
                "</body>",
                $"{PlaybackSpeedScript.BuildInjectionBlock()}{Environment.NewLine}</body>",
                RegexOptions.IgnoreCase,
                TimeSpan.FromSeconds(1));

            File.WriteAllText(IndexHtmlPath, content);
            LogScriptInjected(_logger);
        }
        catch (Exception ex)
        {
            LogScriptInjectFailed(_logger, ex, IndexHtmlPath);
        }
    }

    /// <inheritdoc />
    public override void OnUninstalling()
    {
        RemoveScript();
        base.OnUninstalling();
    }

    /// <summary>
    /// Removes the plugin script tag from Jellyfin Web.
    /// </summary>
    public void RemoveScript()
    {
        if (!File.Exists(IndexHtmlPath))
        {
            return;
        }

        try
        {
            var content = File.ReadAllText(IndexHtmlPath);
            var updated = RemoveInjectionBlock(content);
            if (!string.Equals(content, updated, StringComparison.Ordinal))
            {
                File.WriteAllText(IndexHtmlPath, updated);
                LogScriptRemoved(_logger);
            }
        }
        catch (Exception ex)
        {
            LogScriptRemoveFailed(_logger, ex, IndexHtmlPath);
        }
    }

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return
        [
            new PluginPageInfo
            {
                Name = Name,
                DisplayName = Name,
                EmbeddedResourcePath = string.Format(CultureInfo.InvariantCulture, "{0}.Configuration.configPage.html", GetType().Namespace)
            }
        ];
    }

    private static string RemoveInjectionBlock(string content)
    {
        var pattern = $"{Regex.Escape(PlaybackSpeedScript.StartComment)}[\\s\\S]*?{Regex.Escape(PlaybackSpeedScript.EndComment)}\\s*";
        return Regex.Replace(content, pattern, string.Empty, RegexOptions.Multiline, TimeSpan.FromSeconds(1));
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Jellyfin Web index.html was not found at {Path}; playback speed controls were not injected.")]
    private static partial void LogIndexNotFound(ILogger logger, string path);

    [LoggerMessage(EventId = 2, Level = LogLevel.Warning, Message = "Could not find closing body tag in {Path}; playback speed controls were not injected.")]
    private static partial void LogBodyTagNotFound(ILogger logger, string path);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Injected Playback Speed script into Jellyfin Web.")]
    private static partial void LogScriptInjected(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Unable to inject Playback Speed script into {Path}.")]
    private static partial void LogScriptInjectFailed(ILogger logger, Exception exception, string path);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Removed Playback Speed script from Jellyfin Web.")]
    private static partial void LogScriptRemoved(ILogger logger);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Unable to remove Playback Speed script from {Path}.")]
    private static partial void LogScriptRemoveFailed(ILogger logger, Exception exception, string path);
}
