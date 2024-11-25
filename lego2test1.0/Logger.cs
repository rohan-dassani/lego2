// using System;
// using System.IO;

// public static class Logger
// {
//     // private static readonly string logFilePath = @"C:\PoC\Logs\event_log.csv";
//     private static readonly string logFilePath = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Logs\event_log.csv";

//     // Static constructor to ensure initialization
//     static Logger()
//     {
//         Console.WriteLine($"Logger const started");
//         InitializeLogger();
//         Console.WriteLine($"Logger const done");
//     }

//     private static void InitializeLogger()
//     {
//         try
//         {
//             Console.WriteLine($"Logger init started");
//             string logDirectory = Path.GetDirectoryName(logFilePath);
//             Console.WriteLine(logDirectory);

//             // Ensure the logs directory exists
//             if (!Directory.Exists(logDirectory))
//             {
//                 Directory.CreateDirectory(logDirectory);
//             }
//             Console.WriteLine($"Direc created");

//             // Create the log file if it doesn’t exist
//             if (!File.Exists(logFilePath))
//             {
//                 using (var writer = File.CreateText(logFilePath))
//                 {
//                     writer.WriteLine("Timestamp,EventType,FileName"); // Add headers
//                 }
//             }

//             Console.WriteLine($"Logger initialized. Logs will be written to: {logFilePath}");
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error initializing logger: {ex.Message}");
//         }
//     }

//     public static void LogEvent(string eventType, string fileName, DateTime timestamp)
//     {
//         try
//         {
//             using (StreamWriter writer = new StreamWriter(logFilePath, true))
//             {
//                 writer.WriteLine($"{timestamp},{eventType},{fileName}");
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"Error logging event: {ex.Message}");
//         }
//     }
// }


using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class Logger
{
    private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
    private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly Task _logTask;
    private readonly string _logFilePath = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Logs\event_log.csv";

    private Logger()
    {
        // Start the background logging task
        _logTask = Task.Run(ProcessLogQueueAsync);
    }

    public static Logger Instance => _instance.Value;

    public void LogEvent(string severity, string fileName, DateTime timestamp)
    {
        string logEntry = $"{timestamp},{severity},{fileName}";
        _logQueue.Enqueue(logEntry);
    }

    private async Task ProcessLogQueueAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            while (_logQueue.TryDequeue(out string logEntry))
            {
                try
                {
                    // Write the log entry to the file
                    using (StreamWriter writer = new StreamWriter(_logFilePath, append: true))
                    {
                        await writer.WriteLineAsync(logEntry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing log entry: {ex.Message}");
                }
            }

            // Delay to avoid high CPU usage
            await Task.Delay(100);
        }
    }

    public void StopLogging()
    {
        _cancellationTokenSource.Cancel();
        _logTask.Wait(); // Ensure the background task completes
    }
}
