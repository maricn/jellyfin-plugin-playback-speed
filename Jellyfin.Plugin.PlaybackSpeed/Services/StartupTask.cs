using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.PlaybackSpeed.Services;

/// <summary>
/// Startup task that injects the Playback Speed web script.
/// </summary>
public partial class StartupTask : IScheduledTask
{
    private readonly ILogger<StartupTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StartupTask"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public StartupTask(ILogger<StartupTask> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Playback Speed Startup";

    /// <inheritdoc />
    public string Key => "PlaybackSpeedStartup";

    /// <inheritdoc />
    public string Description => "Injects the Playback Speed control script into Jellyfin Web.";

    /// <inheritdoc />
    public string Category => "Startup Services";

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        if (Plugin.Instance is null)
        {
            LogPluginInstanceUnavailable(_logger);
            return Task.CompletedTask;
        }

        if (Plugin.Instance.Configuration.Enabled)
        {
            Plugin.Instance.InjectScript();
        }
        else
        {
            Plugin.Instance.RemoveScript();
        }

        progress.Report(100);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.StartupTrigger
        };
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Playback Speed plugin instance is not available.")]
    private static partial void LogPluginInstanceUnavailable(ILogger logger);
}
