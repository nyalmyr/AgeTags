using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.AgeTags.Tasks;

/// <summary>
/// Scheduled task entrypoint.
/// </summary>
public sealed class AgeTagsScheduledTask : IScheduledTask
{
    private readonly ILogger<AgeTagsScheduledTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgeTagsScheduledTask"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    public AgeTagsScheduledTask(ILogger<AgeTagsScheduledTask> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Age Tags (Dry-run)";

    /// <inheritdoc />
    public string Key => "AgeTagsDryRun";

    /// <inheritdoc />
    public string Description => "Computes age tags from TMDB and logs planned changes. Does not write anything.";

    /// <inheritdoc />
    public string Category => "Library";

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // No automatic triggers by default (manual only for safety).
        return Array.Empty<TaskTriggerInfo>();
    }

    /// <inheritdoc />
    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AgeTags Dry-run task started. (No changes will be written.)");
        progress.Report(100);
        _logger.LogInformation("AgeTags Dry-run task finished.");
        return Task.CompletedTask;
    }
}
