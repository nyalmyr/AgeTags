using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Jellyfin.Plugin.AgeTags.Core;

/// <summary>
/// Provides utilities to normalize and map content ratings/certifications to an age value.
/// </summary>
public static class AgeRules
{
    private static readonly HashSet<string> UnknownCerts = new(StringComparer.OrdinalIgnoreCase)
    {
        "NR",
        "UNRATED",
        "NOT RATED",
        "N/A",
        "NONE",
    };

    private static readonly Dictionary<string, int> TvCommonAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Y"] = 0,
        ["Y7"] = 7,
        ["MA"] = 17,
    };

    private static readonly Dictionary<string, int> MapAu = new(StringComparer.OrdinalIgnoreCase)
    {
        ["G"] = 0,
        ["M"] = 15,
        ["MA15+"] = 15,
        ["PG"] = 10,
        ["R18+"] = 18,
    };

    private static readonly Dictionary<string, int> MapBr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["10"] = 10,
        ["12"] = 12,
        ["14"] = 14,
        ["16"] = 16,
        ["18"] = 18,
        ["L"] = 0,
    };

    private static readonly Dictionary<string, int> MapCa = new(StringComparer.OrdinalIgnoreCase)
    {
        ["14A"] = 14,
        ["18A"] = 18,
        ["G"] = 0,
        ["PG"] = 10,
        ["R"] = 18,
    };

    private static readonly Dictionary<string, int> MapDe = new(StringComparer.OrdinalIgnoreCase)
    {
        ["0"] = 0,
        ["12"] = 12,
        ["16"] = 16,
        ["18"] = 18,
        ["6"] = 6,
        ["FSK 0"] = 0,
        ["FSK 12"] = 12,
        ["FSK 16"] = 16,
        ["FSK 18"] = 18,
        ["FSK 6"] = 6,
    };

    private static readonly Dictionary<string, int> MapDk = new(StringComparer.OrdinalIgnoreCase)
    {
        ["11"] = 11,
        ["15"] = 15,
        ["17"] = 17,
        ["18"] = 18,
        ["7"] = 7,
        ["A"] = 0,
    };

    private static readonly Dictionary<string, int> MapEs = new(StringComparer.OrdinalIgnoreCase)
    {
        ["12"] = 12,
        ["16"] = 16,
        ["18"] = 18,
        ["7"] = 7,
        ["7I"] = 7,
        ["A"] = 0,
        ["APTA"] = 0,
        ["TP"] = 0,
    };

    private static readonly Dictionary<string, int> MapFi = new(StringComparer.OrdinalIgnoreCase)
    {
        ["12"] = 12,
        ["16"] = 16,
        ["18"] = 18,
        ["7"] = 7,
        ["S"] = 0,
    };

    private static readonly Dictionary<string, int> MapFr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["10"] = 10,
        ["12"] = 12,
        ["13"] = 13,
        ["14"] = 14,
        ["15"] = 15,
        ["16"] = 16,
        ["18"] = 18,
        ["6"] = 6,
        ["7"] = 7,
        ["TP"] = 0,
        ["U"] = 0,
    };

    private static readonly Dictionary<string, int> MapGb = new(StringComparer.OrdinalIgnoreCase)
    {
        ["12"] = 12,
        ["12A"] = 12,
        ["15"] = 15,
        ["18"] = 18,
        ["PG"] = 10,
        ["R18"] = 18,
        ["U"] = 0,
    };

    private static readonly Dictionary<string, int> MapIn = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = 18,
        ["U"] = 0,
        ["UA"] = 12,
    };

    private static readonly Dictionary<string, int> MapIt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["T"] = 0,
        ["VM12"] = 12,
        ["VM14"] = 14,
        ["VM18"] = 18,
    };

    private static readonly Dictionary<string, int> MapJp = new(StringComparer.OrdinalIgnoreCase)
    {
        ["G"] = 0,
        ["PG12"] = 12,
        ["R15+"] = 15,
        ["R18+"] = 18,
    };

    private static readonly Dictionary<string, int> MapKr = new(StringComparer.OrdinalIgnoreCase)
    {
        ["12"] = 12,
        ["15"] = 15,
        ["19"] = 18,
        ["7"] = 7,
        ["ALL"] = 0,
    };

    private static readonly Dictionary<string, int> MapMx = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = 0,
        ["AA"] = 0,
        ["B"] = 12,
        ["B15"] = 15,
        ["C"] = 18,
        ["D"] = 18,
    };

    private static readonly Dictionary<string, int> MapNl = new(StringComparer.OrdinalIgnoreCase)
    {
        ["12"] = 12,
        ["16"] = 16,
        ["6"] = 6,
        ["9"] = 9,
        ["AL"] = 0,
    };

    private static readonly Dictionary<string, int> MapNo = new(StringComparer.OrdinalIgnoreCase)
    {
        ["12"] = 12,
        ["15"] = 15,
        ["18"] = 18,
        ["6"] = 6,
        ["9"] = 9,
        ["A"] = 0,
    };

    private static readonly Dictionary<string, int> MapPt = new(StringComparer.OrdinalIgnoreCase)
    {
        ["M/12"] = 12,
        ["M/14"] = 14,
        ["M/16"] = 16,
        ["M/18"] = 18,
        ["M/6"] = 6,
        ["T"] = 0,
    };

    private static readonly Dictionary<string, int> MapRu = new(StringComparer.OrdinalIgnoreCase)
    {
        ["0+"] = 0,
        ["12+"] = 12,
        ["16+"] = 16,
        ["18+"] = 18,
        ["6+"] = 6,
    };

    private static readonly Dictionary<string, int> MapSe = new(StringComparer.OrdinalIgnoreCase)
    {
        ["11"] = 11,
        ["15"] = 15,
        ["18"] = 18,
        ["7"] = 7,
        ["BTL"] = 0,
    };

    private static readonly Dictionary<string, int> MapUsMovie = new(StringComparer.OrdinalIgnoreCase)
    {
        ["13"] = 13,
        ["G"] = 0,
        ["NC-17"] = 18,
        ["PG"] = 10,
        ["PG-13"] = 13,
        ["R"] = 17,
    };

    private static readonly Dictionary<string, int> MapUsTv = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TV-14"] = 14,
        ["TV-G"] = 0,
        ["TV-MA"] = 17,
        ["TV-PG"] = 10,
        ["TV-Y"] = 0,
        ["TV-Y7"] = 7,
    };

    private static readonly Dictionary<string, Dictionary<string, int>> RegionMapsMovie = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FR"] = MapFr,
        ["US"] = MapUsMovie,
        ["GB"] = MapGb,
        ["DE"] = MapDe,
        ["CA"] = MapCa,
        ["AU"] = MapAu,
        ["ES"] = MapEs,
        ["IT"] = MapIt,
        ["NL"] = MapNl,
        ["PT"] = MapPt,
        ["BR"] = MapBr,
        ["KR"] = MapKr,
        ["SE"] = MapSe,
        ["RU"] = MapRu,
        ["DK"] = MapDk,
        ["NO"] = MapNo,
        ["FI"] = MapFi,
        ["IN"] = MapIn,
        ["JP"] = MapJp,
        ["MX"] = MapMx,
    };

    private static readonly Dictionary<string, Dictionary<string, int>> RegionMapsTv = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FR"] = MapFr,
        ["US"] = MapUsTv,
        ["GB"] = MapGb,
        ["DE"] = MapDe,
        ["CA"] = MapCa,
        ["AU"] = MapAu,
        ["ES"] = MapEs,
        ["IT"] = MapIt,
        ["NL"] = MapNl,
        ["PT"] = MapPt,
        ["BR"] = MapBr,
        ["KR"] = MapKr,
        ["SE"] = MapSe,
        ["RU"] = MapRu,
        ["DK"] = MapDk,
        ["NO"] = MapNo,
        ["FI"] = MapFi,
        ["IN"] = MapIn,
        ["JP"] = MapJp,
        ["MX"] = MapMx,
    };

    /// <summary>
    /// Normalizes a certification string (e.g. "FR-16" -&gt; "16", "tv_14" -&gt; "TV-14").
    /// </summary>
    /// <param name="cert">The raw certification string.</param>
    /// <returns>A normalized certification string, or empty if input is null/blank.</returns>
    public static string NormalizeCert(string? cert)
    {
        if (string.IsNullOrWhiteSpace(cert))
        {
            return string.Empty;
        }

        var c = cert.Trim().ToUpperInvariant();

        // Remove common "XX-" prefix.
        c = Regex.Replace(
            c,
            @"^(FR|US|GB|DE|ES|IT|NL|SE|DK|NO|FI|PT|BR|MX|CA|AU|JP|KR|IN)\s*[-:]\s*",
            string.Empty,
            RegexOptions.IgnoreCase);

        // Normalize separators.
        c = c.Replace("_", "-", StringComparison.Ordinal).Trim();

        return c;
    }

    /// <summary>
    /// Maps a certification to an age value for a given region.
    /// </summary>
    /// <param name="region">Region code (e.g. FR, US, GB).</param>
    /// <param name="cert">Certification string.</param>
    /// <param name="isTv">True for TV content ratings; false for movie certifications.</param>
    /// <returns>The mapped age, or null if unknown.</returns>
    public static int? MapToAge(string region, string? cert, bool isTv)
    {
        var r = string.IsNullOrWhiteSpace(region) ? string.Empty : region.Trim().ToUpperInvariant();
        var c = NormalizeCert(cert);

        if (string.IsNullOrWhiteSpace(r) || string.IsNullOrWhiteSpace(c))
        {
            return null;
        }

        if (UnknownCerts.Contains(c))
        {
            return null;
        }

        if (isTv && TvCommonAliases.TryGetValue(c, out var tvAliasAge))
        {
            return tvAliasAge;
        }

        var dict = isTv ? RegionMapsTv : RegionMapsMovie;

        if (!dict.TryGetValue(r, out var map))
        {
            return null;
        }

        if (map.TryGetValue(c, out var age))
        {
            return age;
        }

        // Numeric fallback (e.g. "12", "16", "18", "0+").
        var digits = Regex.Replace(c, @"\D", string.Empty);
        if (int.TryParse(digits, out var n) && n is >= 0 and <= 21)
        {
            return n;
        }

        return null;
    }

    /// <summary>
    /// Parses a CSV region priority string and ensures the home region is first.
    /// </summary>
    /// <param name="csv">Comma-separated region codes.</param>
    /// <param name="homeRegion">Home region code.</param>
    /// <returns>Ordered list of unique region codes.</returns>
    public static IReadOnlyList<string> ParseRegionPriority(string csv, string homeRegion)
    {
        var list = (csv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToUpperInvariant())
            .Where(x => x.Length is >= 2 and <= 3)
            .Distinct()
            .ToList();

        var hr = (homeRegion ?? string.Empty).Trim().ToUpperInvariant();
        if (!string.IsNullOrWhiteSpace(hr))
        {
            list.Remove(hr);
            list.Insert(0, hr);
        }

        return list;
    }
}
