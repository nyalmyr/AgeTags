using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.AgeTags.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        TmdbApiKey = string.Empty;
        TmdbBaseUrl = "https://api.themoviedb.org/3";
        CountryPriority = "FR,US";
        EnableWrite = false;
    }

    /// <summary>
    /// Gets or sets the TMDB API key (v3).
    /// </summary>
    public string TmdbApiKey { get; set; }

    /// <summary>
    /// Gets or sets the TMDB base url.
    /// </summary>
    public string TmdbBaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the ISO-3166-1 alpha-2 country priority list (comma-separated).
    /// </summary>
    public string CountryPriority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is allowed to write changes (disable dry-run).
    /// </summary>
    public bool EnableWrite { get; set; }
}
