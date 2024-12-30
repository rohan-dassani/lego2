public class Partner
{
    private readonly string partnerId;
    private readonly string directoryPath;
    private readonly FileWatcherService fileWatcherService;
    private readonly JobQueue jobQueue;

    public Partner(string partnerId, string directoryPath, FileWatcherService fileWatcherService)
    {
        this.partnerId = partnerId;
        this.directoryPath = directoryPath;
        this.fileWatcherService = fileWatcherService;
        this.jobQueue = new JobQueue();
    }

    public void Start()
    {
        fileWatcherService.Subscribe(partnerId, directoryPath, OnFileChanged, OnError);
        Console.WriteLine($"Partner {partnerId} started watching directory: {directoryPath}");
    }

    public void Stop()
    {
        fileWatcherService.Unsubscribe(partnerId);
        jobQueue.CompleteAdding();
        Console.WriteLine($"Partner {partnerId} stopped watching directory: {directoryPath}");
    }

    private async Task ProcessFileEvent(FileInfo fileInfo)
    {
        // Mock processing by adding some delay
        await Task.Delay(500);
        Console.WriteLine($"Partner {partnerId} processed file: {fileInfo.FullName}");
    }

    private Task OnFileChanged(FileInfo fileInfo)
    {
        Console.WriteLine($"Partner {partnerId} received file event: {fileInfo.FullName}");
        jobQueue.EnqueueJob(() => ProcessFileEvent(fileInfo));
        return Task.CompletedTask;
    }

    private void OnError(Exception exception)
    {
        Console.WriteLine($"Error for partner {partnerId}: {exception.Message}");
    }
}