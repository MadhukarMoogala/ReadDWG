using Microsoft.AspNetCore.Mvc;
using Serilog.Context;


namespace WebApi;

[ApiController]
[Route("api/[controller]")]
public class DrawingsController : ControllerBase
{
    private readonly IJobQueue _jobQueue;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DrawingsController> _logger;
    public DrawingsController(IJobQueue jobQueue, IWebHostEnvironment env, ILogger<DrawingsController> logger)
    {
        _jobQueue = jobQueue;
        _env = env;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null)
        {
            _logger.LogWarning("No file received in upload.");
            return BadRequest("No file provided.");
        }

        _logger.LogInformation("Received file: {Filename}, Size: {Size}", file.FileName, file.Length);

        if (!Path.GetExtension(file.FileName).Equals(".dwg", StringComparison.CurrentCultureIgnoreCase))
        {
            _logger.LogWarning("Unsupported file type received: {Extension}", Path.GetExtension(file.FileName));
            return BadRequest("Only .dwg files are supported.");
        }

        var jobId = Guid.NewGuid().ToString(); // Generate JobId
        var folder = Path.Combine(_env.ContentRootPath, "Temp");
        Directory.CreateDirectory(folder);

        var inputPath = Path.Combine(folder, jobId + ".dwg");
        var outputPath = Path.Combine(folder, jobId + ".txt");

        try
        {
            using var stream = System.IO.File.Create(inputPath);
            await file.CopyToAsync(stream).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false); // Just to be extra safe
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save uploaded file for job {JobId}", jobId);
            return StatusCode(500, "Failed to save uploaded file.");
        }

        var job = new DrawingJob
        {
            Id = jobId,
            InputPath = inputPath,
            OutputPath = outputPath,
        };

        // Use LogContext to add JobId to the log context for this request
        using (LogContext.PushProperty("JobId", jobId))
        {
            await Task.Delay(100).ConfigureAwait(false); // Slight cushion before handing over to the processor
            _jobQueue.Enqueue(job);

            if (!System.IO.File.Exists(inputPath))
            {
                _logger.LogError("File not found after saving: {InputPath}", inputPath);
                return StatusCode(500, "File save failed unexpectedly.");
            }

            _logger.LogInformation("Enqueued job {JobId} with input at {InputPath}", jobId, inputPath);
        }

        return Ok(new { jobId = jobId });
    }


    [HttpGet("status/{id}")]
    public IActionResult Status(string id)
    {
        var job = _jobQueue.GetJob(id);
        if (job == null)
        {
            _logger.LogWarning("Status check for unknown job ID: {JobId}", id);
            return NotFound();
        }

        _logger.LogInformation("Status check for job {JobId}: {Status}", id, job.Status);
        return Ok(new JobStatusResponse
        {
            Status = job.Status.ToString(),
            ErrorMessage = job.ErrorMessage
        });
    }

    [HttpGet("result/{id}")]
    public IActionResult Result(string id)
    {
        var job = _jobQueue.GetJob(id);
        if (job == null)
        {
            _logger.LogWarning("Requested job ID {JobId} not found.", id);
            return NotFound();
        }

        if (job.Status != JobStatus.Completed)
        {
            _logger.LogInformation("Job {JobId} is not completed. Current status: {Status}", id, job.Status);
            return BadRequest("Job not yet completed.");
        }

        if (!System.IO.File.Exists(job.OutputPath))
        {
            _logger.LogError("Output file for completed job {JobId} not found at path: {Path}", id, job.OutputPath);
            return StatusCode(404, "Job marked completed, but result file not found.");
        }

        try
        {
            var content = System.IO.File.ReadAllText(job.OutputPath);
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("Result file for job {JobId} is empty.", id);
                return NotFound("Result file is empty.");
            }

            // Parse and structure the raw content
            var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
            var layers = new List<string>();
            var blocks = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("Layer:"))
                {
                    layers.Add(line["Layer:".Length..].Trim());
                }
                else if (line.StartsWith("Block:"))
                {
                    blocks.Add(line["Block:".Length..].Trim());
                }
            }

            _logger.LogInformation("Successfully read result file for job {JobId}", id);

            return Ok(new JobResultResponse
            {
                Output = new JobOutput
                {
                    Layers = layers,
                    Blocks = blocks
                }
            });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read result file for job {JobId} at {Path}", id, job.OutputPath);
            return StatusCode(500, $"Failed to read result: {ex.Message}");
        }
    }

    public class JobStatusResponse
    {
        public string? Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class JobOutput
    {
        public List<string>? Layers { get; set; }
        public List<string>? Blocks { get; set; }
    }

    public class JobResultResponse
    {
        public JobOutput? Output { get; set; }
    }



}