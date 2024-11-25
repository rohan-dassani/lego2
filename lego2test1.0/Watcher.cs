using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

public class Watcher
{
    private readonly string sourceDirectory;
    private readonly string targetDirectory;
    private readonly ConcurrentQueue<FileInfo> fileQueue = new ConcurrentQueue<FileInfo>();
    private readonly ManualResetEvent exitEvent = new ManualResetEvent(false);
    private const int MaxRetryAttempts = 3; // Number of retries for locked files
    private const int RetryDelay = 200; // Delay between retries in milliseconds
    public int filesProcessed = 0;
    public int eventDrops = 0;
    private int bufferSize = 8192;
    private string content = new string('A', 10 * 1024); //newfilesize


    public Watcher(string sourceDir, string targetDir , int buffersize = 8192)
    {
        sourceDirectory = sourceDir;
        targetDirectory = targetDir;
        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(targetDirectory);
        bufferSize = buffersize;
        filesProcessed = 0;
        eventDrops = 0;
        
    }

    public void Start()
    {
        using (FileSystemWatcher watcher = new FileSystemWatcher())
        {
            watcher.Path = sourceDirectory;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
            watcher.Filter = "*.*";
            
            watcher.Created += OnFileCreated;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Current Internal Buffer Size: {watcher.InternalBufferSize} bytes");

            Thread processingThread = new Thread(ProcessFiles);
            processingThread.Start();

            Console.WriteLine("File watcher started. Press 'q' to quit.");

            while (Console.Read() != 'q') ;

            // LogResults("Test1", filesProcessed, eventDrops);  // Example for Test1 logging

            exitEvent.Set();
            processingThread.Join();
        }
    }

    private void OnFileCreated(object source, FileSystemEventArgs e)
    {
        FileInfo fileInfo = new FileInfo(e.FullPath);
        fileQueue.Enqueue(fileInfo);
        Logger.Instance.LogEvent("FileCreated", fileInfo.Name, DateTime.Now);
        Console.WriteLine($"File Created: {fileInfo.Name}");
    }

    private void ProcessFiles()
    {
        while (!exitEvent.WaitOne(0))
        {
            if (fileQueue.TryDequeue(out FileInfo file))
            {
                int attempts = 0;
                bool processedSuccessfully = false;

                while (attempts < MaxRetryAttempts && !processedSuccessfully)
                {
                    try
                    {
                        string targetFilePath = Path.Combine(targetDirectory, $"processed_{file.Name}");
                        // string targetFilePath = Path.Combine(targetDirectory, $"processeddddddddddddddddddddddddddddd_{file.Name}");
                        
                        // Thread.Sleep(1000); // Simulate processing time
                        File.WriteAllText(targetFilePath, content);

                        // Log successful processing
                        Logger.Instance.LogEvent("FileProcessed", file.Name, DateTime.Now);
                        Console.WriteLine($"File processed: {file.Name}");

                        // Delete the original source file upon successful processing
                        File.Delete(file.FullName);
                        Logger.Instance.LogEvent("FileDeleted", file.Name, DateTime.Now);
                        Console.WriteLine($"Source file deleted: {file.Name}");

                        processedSuccessfully = true; // Mark as processed
                        filesProcessed++;
                        Console.WriteLine($"Processed no.: {filesProcessed}");
                    }
                    catch (IOException ex) when (ex.Message.Contains("used by another process"))
                    {
                        attempts++;
                        Console.WriteLine($"File {file.Name} is in use. Retry attempt {attempts}.");
                        Thread.Sleep(RetryDelay); // Wait before retrying
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogEvent("Error", file.Name, DateTime.Now);
                        Console.WriteLine($"Error processing file {file.Name}: {ex.Message}");
                        break; // Stop further attempts on non-IO errors
                    }
                }

                if (!processedSuccessfully)
                {
                    eventDrops+=1;
                    Console.WriteLine($"Failed to process file {file.Name} after {MaxRetryAttempts} attempts.");
                }
            }
            Thread.Sleep(50); // Adjust to improve responsiveness
        }
    }
    // private void LogResults(string testCase, int filesProcessed, int eventDrops)
    // {
    //     double eventDropPercentage = (double)eventDrops / (filesProcessed + eventDrops) * 100;
    //     ResultLogger.LogResult(testCase, 100, "2 mins", "5KB", filesProcessed, eventDropPercentage, "IO Error (File in use)");
    // }
}




// public class Watcher
// {
//     private readonly string sourceDirectory;
//     private readonly string targetDirectory;
//     private int filesProcessed = 0;
//     private int eventDrops = 0;

//     public Watcher(string sourceDirectory, string targetDirectory)
//     {
//         this.sourceDirectory = sourceDirectory;
//         this.targetDirectory = targetDirectory;
//     }

//     public void Start()
//     {
//         using FileSystemWatcher watcher = new FileSystemWatcher(sourceDirectory);
//         watcher.Created += OnCreated;
//         watcher.EnableRaisingEvents = true;

//         Console.WriteLine("Watcher started. Press 'q' to stop and log results.");

//         while (Console.ReadKey().KeyChar != 'q') { }

//         LogResults("Test1", filesProcessed, eventDrops);  // Example for Test1 logging
//     }

//     private void OnCreated(object sender, FileSystemEventArgs e)
//     {
//         try
//         {
//             string targetPath = Path.Combine(targetDirectory, e.Name);
//             File.Move(e.FullPath, targetPath);
//             filesProcessed++;
//         }
//         catch (IOException ex)
//         {
//             eventDrops++;
//             Logger.LogEvent("Error", e.Name, DateTime.Now);
//         }
//     }

//     private void LogResults(string testCase, int filesProcessed, int eventDrops)
//     {
//         double eventDropPercentage = (double)eventDrops / (filesProcessed + eventDrops) * 100;
//         ResultLogger.LogResult(testCase, 100, "10 mins", "1KB", filesProcessed, eventDropPercentage, "IO Error (File in use)");
//     }
// }
