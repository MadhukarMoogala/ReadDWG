

namespace WebApi;

public interface IJobQueue
{
    void Enqueue(DrawingJob job);
    bool? TryDequeue(out DrawingJob? job);
    DrawingJob? GetJob(string id);
}