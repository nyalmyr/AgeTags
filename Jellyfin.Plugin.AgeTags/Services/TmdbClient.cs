using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.AgeTags.Core;

namespace Jellyfin.Plugin.AgeTags.Services;

/// <summary>
/// Minimal TMDB client for reading certifications.
/// </summary>
public sealed class TmdbClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="TmdbClient"/> class.
    /// </summary>
    /// <param name="http">The HTTP client.</param>
    /// <param name="apiKey">The TMDB API key (v3).</param>
    /// <param name="baseUrl">The TMDB base url (e.g. https://api.themoviedb.org/3).</param>
    public TmdbClient(HttpClient http, string apiKey, string baseUrl)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _apiKey = apiKey ?? string.Empty;
        _baseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://api.themoviedb.org/3" : baseUrl.Trim();
    }

    /// <summary>
    /// Gets TV content rating certs per region for a TMDB TV id.
    /// Endpoint: /tv/{id}/content_ratings.
    /// </summary>
    /// <param name="tmdbTvId">The TMDB TV id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary region -&gt; normalized certification.</returns>
    public async Task<IReadOnlyDictionary<string, string>> GetTvCertsByRegionAsync(int tmdbTvId, CancellationToken ct)
    {
        this.EnsureApiKey();

        var url = $"{_baseUrl}/tv/{tmdbTvId}/content_ratings?api_key={Uri.EscapeDataString(_apiKey)}";
        var dto = await _http.GetFromJsonAsync<TvContentRatingsResponse>(url, cancellationToken: ct).ConfigureAwait(false);

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (dto?.Results == null)
        {
            return dict;
        }

        foreach (var r in dto.Results)
        {
            if (!string.IsNullOrWhiteSpace(r?.Iso3166_1) && !string.IsNullOrWhiteSpace(r?.Rating))
            {
                dict[r.Iso3166_1.ToUpperInvariant()] = AgeRules.NormalizeCert(r.Rating);
            }
        }

        return dict;
    }

    /// <summary>
    /// Gets movie release certifications per region for a TMDB movie id.
    /// Endpoint: /movie/{id}/release_dates.
    /// </summary>
    /// <param name="tmdbMovieId">The TMDB movie id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary region -&gt; list of (type, normalized certification).</returns>
    public async Task<IReadOnlyDictionary<string, List<(int Type, string Cert)>>> GetMovieReleaseCertsByRegionAsync(int tmdbMovieId, CancellationToken ct)
    {
        this.EnsureApiKey();

        var url = $"{_baseUrl}/movie/{tmdbMovieId}/release_dates?api_key={Uri.EscapeDataString(_apiKey)}";
        var dto = await _http.GetFromJsonAsync<MovieReleaseDatesResponse>(url, cancellationToken: ct).ConfigureAwait(false);

        var dict = new Dictionary<string, List<(int Type, string Cert)>>(StringComparer.OrdinalIgnoreCase);
        if (dto?.Results == null)
        {
            return dict;
        }

        foreach (var r in dto.Results)
        {
            if (string.IsNullOrWhiteSpace(r?.Iso3166_1) || r.ReleaseDates == null)
            {
                continue;
            }

            var region = r.Iso3166_1.ToUpperInvariant();
            if (!dict.TryGetValue(region, out var list))
            {
                list = new List<(int Type, string Cert)>();
                dict[region] = list;
            }

            foreach (var rd in r.ReleaseDates)
            {
                if (rd == null)
                {
                    continue;
                }

                var cert = AgeRules.NormalizeCert(rd.Certification);
                if (string.IsNullOrWhiteSpace(cert))
                {
                    continue;
                }

                list.Add((rd.Type, cert));
            }
        }

        return dict;
    }

    private void EnsureApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException("TMDB API key is missing. Configure it in the plugin settings.");
        }
    }

    private sealed class TvContentRatingsResponse
    {
        [JsonPropertyName("results")]
        public List<TvContentRatingsItem>? Results { get; set; }
    }

    private sealed class TvContentRatingsItem
    {
        [JsonPropertyName("iso_3166_1")]
        public string? Iso3166_1 { get; set; }

        [JsonPropertyName("rating")]
        public string? Rating { get; set; }
    }

    private sealed class MovieReleaseDatesResponse
    {
        [JsonPropertyName("results")]
        public List<MovieReleaseDatesRegion>? Results { get; set; }
    }

    private sealed class MovieReleaseDatesRegion
    {
        [JsonPropertyName("iso_3166_1")]
        public string? Iso3166_1 { get; set; }

        [JsonPropertyName("release_dates")]
        public List<MovieReleaseDateItem>? ReleaseDates { get; set; }
    }

    private sealed class MovieReleaseDateItem
    {
        [JsonPropertyName("certification")]
        public string? Certification { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }
    }
}
