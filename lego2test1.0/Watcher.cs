using System;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

public class Watcher
{
    private readonly string sourceDirectory;
    private readonly string targetDirectory;
    // private readonly ConcurrentQueue<FileInfo> fileQueue = new ConcurrentQueue<FileInfo>();
    private readonly ConcurrentQueue<string> fileQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentDictionary<string, FileInfo> fileDictionary = new ConcurrentDictionary<string, FileInfo>();
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
        // bufferSize = 8192;
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
            
            // watcher.Created += OnFileCreated;
            watcher.Changed += OnFileChanged;
            watcher.Error += OnError;
            watcher.EnableRaisingEvents = true;
            watcher.InternalBufferSize = bufferSize;

            Console.WriteLine($"Current Internal Buffer Size: {watcher.InternalBufferSize} bytes");

            Thread processingThread = new Thread(ProcessFiles);
            processingThread.Start();

            Console.WriteLine("File watcher started. Press 'q' to quit.");

            while (Console.Read() != 'q') ;

            exitEvent.Set();
            processingThread.Join();
        }
    }

    private void OnFileCreated(object source, FileSystemEventArgs e)
    {
        // FileInfo fileInfo = new FileInfo(e.FullPath);
        // fileQueue.Enqueue(fileInfo);
        // Logger.Instance.LogEvent("FileCreated", fileInfo.Name, DateTime.Now);
        // Console.WriteLine($"File Created: {fileInfo.Name}");
        HandleFileEvent(e.FullPath, "FileCreated");
    }

    private void OnFileChanged(object source, FileSystemEventArgs e)
    {
        // FileInfo fileInfo = new FileInfo(e.FullPath);
        // fileQueue.Enqueue(fileInfo);
        // Logger.Instance.LogEvent("FileChanged", fileInfo.Name, DateTime.Now);
        // Console.WriteLine($"File Updated: {fileInfo.Name}");
        HandleFileEvent(e.FullPath, "FileChanged");
        
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine("######################### BUFFER OVERFLOW DETECTED! ##########################");
        Console.WriteLine($"Error: {e.GetException().Message}");
        string errorLogPath = Path.Combine(targetDirectory, "error_loggggggg.txt");
        File.AppendAllText(errorLogPath, $"Error: {e.GetException().Message} at {DateTime.Now}\n");
        // exitEvent.Set();
    }

    private void HandleFileEvent(string filePath, string eventType)
    {
        FileInfo fileInfo = new FileInfo(filePath);

        bool isNewFile = fileDictionary.AddOrUpdate(
        filePath, 
        fileInfo, 
        (key, existingFile) => fileInfo) == fileInfo;

        if (isNewFile)
        {
            fileQueue.Enqueue(filePath); // Only enqueue if the file was newly added
            Console.WriteLine($"New unprocessed event added: {fileInfo.Name} and time: {DateTime.Now}");
        }

        Logger.Instance.LogEvent(eventType, fileInfo.Name, DateTime.Now);
        Console.WriteLine($"{eventType}: {fileInfo.Name}");
    }

    private void ProcessFiles()
    {
        while (!exitEvent.WaitOne(0))
        {
            if (fileQueue.TryDequeue(out string filePath))
            {
                if (fileDictionary.TryRemove(filePath, out FileInfo file))
                {
                    int attempts = 0;
                    bool processedSuccessfully = false;

                    while (attempts < MaxRetryAttempts && !processedSuccessfully)
                    {
                        try
                        {
                            string targetFilePath = Path.Combine(targetDirectory, $"processed_{file.Name}");
                            File.WriteAllText(targetFilePath, DateTime.Now.ToString() + content);

                            Logger.Instance.LogEvent("FileProcessed", file.Name, DateTime.Now);
                            Console.WriteLine($"File processed: {file.Name}");

                            // File.Delete(file.FullName);
                            // Logger.Instance.LogEvent("FileDeleted", file.Name, DateTime.Now);
                            // Console.WriteLine($"Source file deleted: {file.Name}");
                            // Thread.Sleep(5000);

                            processedSuccessfully = true;
                            filesProcessed++;
                            Console.WriteLine($"Processed no.: {filesProcessed}");
                        }
                        catch (IOException ex) when (ex.Message.Contains("used by another process"))
                        {
                            attempts++;
                            Console.WriteLine($"File {file.Name} is in use. Retry attempt {attempts}.");
                            Thread.Sleep(RetryDelay);
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.LogEvent("Error", file.Name, DateTime.Now);
                            Console.WriteLine($"Error processing file {file.Name}: {ex.Message}");
                            break;
                        }
                    }

                    if (!processedSuccessfully)
                    {
                        eventDrops++;
                        Console.WriteLine($"Failed to process file {file.Name} after {MaxRetryAttempts} attempts.");
                    }
                }
            }
            Thread.Sleep(50); 
        }
}

}
