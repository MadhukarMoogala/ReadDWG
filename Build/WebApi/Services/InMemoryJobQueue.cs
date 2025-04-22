using System.Collections.Concurrent;


namespace WebApi;

public class InMemoryJobQueue : IJobQueue
{
    private readonly ConcurrentQueue<DrawingJob> _queue = new();
    private readonly ConcurrentDictionary<string, DrawingJob> _store = new();

    public void Enqueue(DrawingJob job)
    {
        _store[job.Id] = job;
        _queue.Enqueue(job);
    }

    public bool? TryDequeue(out DrawingJob? job) => _queue.TryDequeue(out job);

    public DrawingJob? GetJob(string id) => _store.TryGetValue(id, out var job) ? job : null;
}