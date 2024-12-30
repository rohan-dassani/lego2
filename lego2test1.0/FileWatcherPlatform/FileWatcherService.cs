// using System;
// using System.Collections.Concurrent;
// using System.IO;
// using System.Net.Http;
// using System.Text.Json;
// using System.Threading.Tasks;

// public class FileWatcherService
// {
//     private readonly ConcurrentDictionary<string, FileWatcher> watchers = new();

//     public bool Subscribe(string partnerId, string directoryPath, string endpointUrl)
//     {
//         if (watchers.ContainsKey(partnerId))
//         {
//             Console.WriteLine($"Partner {partnerId} is already subscribed.");
//             return false;
//         }

//         var watcher = new FileWatcher(directoryPath, async (fileInfo) =>
//         { 
//             // await NotifyPartnerAsync(partnerId, endpointUrl, fileInfo),
//             // (exception) => Console.WriteLine($"Error for partner {partnerId}: {exception.Message}")
//             try
//             {
//                 // Call the provided async handler when a file change is detected
//                 await NotifyPartnerAsync(partnerId, endpointUrl, fileInfo);
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Event Processing failed for {partnerId} with exception : {ex.Message}");
//             }
//         });

//         if (watchers.TryAdd(partnerId, watcher))
//         {
//             watcher.Start();
//             Console.WriteLine($"Partner {partnerId} subscribed successfully.");
//             return true;
//         }

//         return false;
//     }

//     public bool Unsubscribe(string partnerId)
//     {
//         if (watchers.TryRemove(partnerId, out var watcher))
//         {
//             watcher.Stop();
//             Console.WriteLine($"Partner {partnerId} unsubscribed successfully.");
//             return true;
//         }

//         Console.WriteLine($"Partner {partnerId} is not subscribed.");
//         return false;
//     }

//     private async Task NotifyPartnerAsync(string partnerId, string endpointUrl, FileInfo fileInfo)
//     {
//         var payload = new
//         {
//             PartnerId = partnerId,
//             EventType = "Changed",
//             FilePath = fileInfo.FullName,
//             Timestamp = DateTime.UtcNow
//         };

//         var jsonContent = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
//         var httpClient = new HttpClient();

//         int retryCount = 3;
//         for (int attempt = 1; attempt <= retryCount; attempt++)
//         {
//             try
//             {
//                 HttpResponseMessage response = await httpClient.PostAsync(endpointUrl, jsonContent);
//                 if (response.IsSuccessStatusCode)
//                 {
//                     Console.WriteLine($"Notification to partner {partnerId} succeeded for file {fileInfo.FullName}.");
//                     return;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Notification attempt {attempt} failed: {ex.Message}");
//             }

//             await Task.Delay(1000);
//         }

//         Console.WriteLine($"Failed to notify partner {partnerId} for file {fileInfo.FullName} after {retryCount} attempts.");
//     }
// }


// using System;
// using System.Collections.Concurrent;
// using System.IO;
// using System.Threading;
// using System.Threading.Tasks;

// public class FileWatcherService
// {
//     private readonly ConcurrentDictionary<string, FileWatcher> watchers = new();
//     private readonly SemaphoreSlim semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent requests

//     public event Func<string, FileInfo, Task> FileChanged;

//     public bool Subscribe(string partnerId, string directoryPath)
//     {
//         if (watchers.ContainsKey(partnerId))
//         {
//             Console.WriteLine($"Partner {partnerId} is already subscribed.");
//             return false;
//         }

//         var watcher = new FileWatcher(directoryPath, async (fileInfo) =>
//         {
//             await semaphore.WaitAsync(); // Acquire the semaphore
//             try
//             {
//                 if (FileChanged != null)
//                 {
//                     await FileChanged.Invoke(partnerId, fileInfo);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Event Processing failed for {partnerId} with exception: {ex.Message}");
//             }
//             finally
//             {
//                 semaphore.Release(); // Release the semaphore
//             }
//         });

//         if (watchers.TryAdd(partnerId, watcher))
//         {
//             watcher.Start();
//             Console.WriteLine($"Partner {partnerId} subscribed successfully.");
//             return true;
//         }

//         return false;
//     }

//     public bool Unsubscribe(string partnerId)
//     {
//         if (watchers.TryRemove(partnerId, out var watcher))
//         {
//             watcher.Stop();
//             Console.WriteLine($"Partner {partnerId} unsubscribed successfully.");
//             return true;
//         }

//         Console.WriteLine($"Partner {partnerId} is not subscribed.");
//         return false;
//     }
// }

// //adding logs

// using System;
// using System.Collections.Concurrent;
// using System.IO;
// using System.Threading;
// using System.Threading.Tasks;

// public class FileWatcherService
// {
//     private readonly ConcurrentDictionary<string, FileWatcher> watchers = new();
//     private readonly SemaphoreSlim semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent requests

//     public event Func<string, FileInfo, Task> FileChanged;

//     public bool Subscribe(string partnerId, string directoryPath)
//     {
//         if (watchers.ContainsKey(partnerId))
//         {
//             Console.WriteLine($"Partner {partnerId} is already subscribed.");
//             return false;
//         }

//         var watcher = new FileWatcher(directoryPath, async (fileInfo) =>
//         {
//             await semaphore.WaitAsync(); // Acquire the semaphore
//             try
//             {
//                 Console.WriteLine($"File event received for partner {partnerId}: {fileInfo.FullName}");
//                 if (FileChanged != null)
//                 {
//                     await FileChanged.Invoke(partnerId, fileInfo);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"Event Processing failed for {partnerId} with exception: {ex.Message}");
//             }
//             finally
//             {
//                 semaphore.Release(); // Release the semaphore
//             }
//         },
//         (exception) =>
//         {
//             // Handle and log the error
//             Console.WriteLine($"Error in FileWatcher for partner {partnerId}: {exception.Message}");
//         });

//         if (watchers.TryAdd(partnerId, watcher))
//         {
//             watcher.Start();
//             Console.WriteLine($"Partner {partnerId} subscribed successfully.");
//             return true;
//         }

//         return false;
//     }

//     public bool Unsubscribe(string partnerId)
//     {
//         if (watchers.TryRemove(partnerId, out var watcher))
//         {
//             watcher.Stop();
//             Console.WriteLine($"Partner {partnerId} unsubscribed successfully.");
//             return true;
//         }

//         Console.WriteLine($"Partner {partnerId} is not subscribed.");
//         return false;
//     }
// }


// separating events for each partner
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
