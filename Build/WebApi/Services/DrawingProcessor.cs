using System.Diagnostics;

namespace WebApi;

public class DrawingProcessor : BackgroundService
{
    private readonly IJobQueue _queue;
    private readonly ILogger<DrawingProcessor> _logger;
    private readonly SemaphoreSlim _concurrencyLimiter = new(8); // Set concurrency

    public DrawingProcessor(IJobQueue queue, ILogger<DrawingProcessor> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var dequeueStart = DateTime.UtcNow;

            if ((_queue.TryDequeue(out DrawingJob? job) is true) && job is not null)
            {
                var dequeueDelay = DateTime.UtcNow - dequeueStart;
                _logger.LogInformation("Dequeued job {JobId} after waiting {WaitMs} ms", job.Id, dequeueDelay.TotalMilliseconds);

                await _concurrencyLimiter.WaitAsync(stoppingToken).ConfigureAwait(false);
                _logger.LogInformation("Job {JobId} acquired slot. Slots remaining: {SlotsLeft}", job.Id, _concurrencyLimiter.CurrentCount);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessJobAsync(job, stoppingToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while processing job {JobId}", job.Id);
                        job.Status = JobStatus.Failed;
                        job.ErrorMessage = ex.Message;
                    }
                    finally
                    {
                        _concurrencyLimiter.Release();
                        _logger.LogInformation("Job {JobId} released slot. Slots remaining: {SlotsLeft}", job.Id, _concurrencyLimiter.CurrentCount);
                    }
                }, stoppingToken);
            }
            else
            {
                await Task.Delay(500, stoppingToken).ConfigureAwait(false); // Back off if queue is empty
            }
        }
    }

    private async Task ProcessJobAsync(DrawingJob job, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Job {JobId} status: Processing", job.Id);
        job.Status = JobStatus.Processing;

        const int maxAttempts = 3;
        int attempt = 0;
        bool success = false;

        while (attempt < maxAttempts && !success && !stoppingToken.IsCancellationRequested)
        {
            attempt++;
            var attemptStart = DateTime.UtcNow;

            try
            {
                var fileInfo = new FileInfo(job.InputPath);
                if (!fileInfo.Exists || fileInfo.Length == 0)
                {
                    _logger.LogWarning("Attempt {Attempt}: Job {JobId} has invalid input file at {Path}", attempt, job.Id, job.InputPath);
                    await Task.Delay(100, stoppingToken).ConfigureAwait(false);
                    continue;
                }

                _logger.LogInformation("▶️ Attempt {Attempt}: Launching DWGExtractor for Job {JobId}", attempt, job.Id);

                var psi = new ProcessStartInfo
                {
                    FileName = "DWGExtractor.exe",
                    Arguments = $"-i \"{job.InputPath}\" -o \"{job.OutputPath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                var process = Process.Start(psi);
                if (process == null)
                    throw new InvalidOperationException("Failed to start DWGExtractor process.");

                var stopwatch = Stopwatch.StartNew();
                await process.WaitForExitAsync(stoppingToken).ConfigureAwait(false);
                stopwatch.Stop();

                _logger.LogInformation("Job {JobId} exited with code {ExitCode} after {ElapsedMs} ms",
                    job.Id, process.ExitCode, stopwatch.ElapsedMilliseconds);

                bool outputValid = File.Exists(job.OutputPath);
                if (process.ExitCode == 0 && outputValid)
                {
                    job.Status = JobStatus.Completed;
                    _logger.LogInformation("Job {JobId} completed successfully on attempt {Attempt}.", job.Id, attempt);
                    success = true;
                }
                else
                {
                    _logger.LogWarning("Attempt {Attempt}: Job {JobId} failed. Exit code: {ExitCode}, Output exists: {Exists}",
                        attempt, job.Id, process.ExitCode, outputValid);
                    if (attempt < maxAttempts)
                        await Task.Delay(200, stoppingToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Exception on attempt {Attempt} for Job {JobId}", attempt, job.Id);
            }
            finally
            {
                var duration = DateTime.UtcNow - attemptStart;
                _logger.LogInformation("Attempt {Attempt} for Job {JobId} took {DurationMs} ms", attempt, job.Id, duration.TotalMilliseconds);
            }
        }

        if (!success)
        {
            job.Status = JobStatus.Failed;
            _logger.LogError("Job {JobId} failed after {MaxAttempts} attempts.", job.Id, maxAttempts);
        }
    }
}
