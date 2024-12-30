using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class FileWatcherService
{
    private readonly ConcurrentDictionary<string, FileWatcher> watchers = new();
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent requests

    public bool Subscribe(string partnerId, string directoryPath, Func<FileInfo, Task> onFileChanged, Action<Exception> onError)
    {
        if (watchers.ContainsKey(partnerId))
        {
            Console.WriteLine($"Partner {partnerId} is already subscribed.");
            return false;
        }

        var watcher = new FileWatcher(directoryPath, async (fileInfo) =>
        {
            await semaphore.WaitAsync(); // Acquire the semaphore
            try
            {
                Console.WriteLine($"File event received for partner {partnerId}: {fileInfo.FullName}");
                await onFileChanged(fileInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Event Processing failed for {partnerId} with exception: {ex.Message}");
            }
            finally
            {
                semaphore.Release(); // Release the semaphore
            }
        },
        (exception) =>
        {
            // Handle and log the error
            Console.WriteLine($"Error in FileWatcher for partner {partnerId}: {exception.Message}");
            onError?.Invoke(exception);
        });

        if (watchers.TryAdd(partnerId, watcher))
        {
            watcher.Start();
            Console.WriteLine($"Partner {partnerId} subscribed successfully.");
            return true;
        }

        return false;
    }

    public bool Unsubscribe(string partnerId)
    {
        if (watchers.TryRemove(partnerId, out var watcher))
        {
            watcher.Stop();
            Console.WriteLine($"Partner {partnerId} unsubscribed successfully.");
            return true;
        }

        Console.WriteLine($"Partner {partnerId} is not subscribed.");
        return false;
    }
}
