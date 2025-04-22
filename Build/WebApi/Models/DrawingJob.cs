namespace WebApi {
    public enum JobStatus { Pending, Processing, Completed, Failed }

    public class DrawingJob
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string InputPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public string? ErrorMessage { get; set; }
    }
}

