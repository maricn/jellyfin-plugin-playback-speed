using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.PlaybackSpeed.Controllers;

/// <summary>
/// Serves the client script used by Jellyfin Web.
/// </summary>
[ApiController]
[Route("PlaybackSpeed")]
public class PlaybackSpeedController : ControllerBase
{
    /// <summary>
    /// Gets the Jellyfin Web client script.
    /// </summary>
    /// <returns>JavaScript content.</returns>
    [HttpGet("client.js")]
    [AllowAnonymous]
    [Produces("application/javascript")]
    public ActionResult GetClientScript()
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null)
        {
            return Content("console.warn('Playback Speed plugin configuration is unavailable.');", "application/javascript");
        }

        var payload = JsonSerializer.Serialize(new
        {
            config.Enabled,
            config.ShowFloatingControl,
            config.DefaultSpeed,
            Speeds = ParseSpeeds(config.AvailableSpeeds)
        });

        var script = ReadEmbeddedClientScript();
        return Content($"window.JellyfinPlaybackSpeedConfig = {payload};{Environment.NewLine}{script}", "application/javascript");
    }

    private static double[] ParseSpeeds(string availableSpeeds)
    {
        var speeds = availableSpeeds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => double.TryParse(value, out var speed) ? speed : 0)
            .Where(value => value > 0 && value <= 16)
            .Distinct()
            .OrderBy(value => value)
            .ToArray();

        return speeds.Length == 0 ? [1, 1.25, 1.5, 2] : speeds;
    }

    private static string ReadEmbeddedClientScript()
    {
        const string ResourceName = "Jellyfin.Plugin.PlaybackSpeed.Web.client.js";
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            return "console.error('Playback Speed plugin client script is missing.');";
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
