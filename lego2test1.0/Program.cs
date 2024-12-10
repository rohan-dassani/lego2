using System;
using System.IO;
using System.Threading.Tasks;

class Program
{
    private static readonly string sourceDirectory = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Source";
    private static readonly string targetDirectory = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Target";
    // private static readonly string sourceDirectory = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Sourceeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
    // private static readonly string targetDirectory = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Targettttttttttttttttttttttttttttttttttttttttttt";

    static async Task Main()
    {
        Console.WriteLine("Initializing Logger...");
        Logger.Instance.LogEvent("INFO", "Program started", DateTime.Now);

        Console.WriteLine("Initializing Result Logger...");
        ResultLogger.LogResult("starting", 0,"0", "0",0, 0, 0, "0", "0");

        // Test params
        Console.WriteLine("Starting Phase 1: File Volume and Event Duration Tests...");
        await RunTest("Test1", fileCount: 50000, duration: TimeSpan.FromMinutes(1), fileSizeInKB: 10, 4096*16, 8);

        //thread counts for file creation
        // await RunTest("Test1-phase3", fileCount: 1000, duration: TimeSpan.FromMinutes(1), fileSizeInKB: 25 , 8192, 2);
        // await RunTest("Test2-phase3", fileCount: 2500, duration: TimeSpan.FromMinutes(1), fileSizeInKB: 25 , 8192, 2);
        // await RunTest("Test3-phase3", fileCount: 2500, duration: TimeSpan.FromMinutes(1), fileSizeInKB: 25 , 8192, 4);
        // await RunTest("Test4-phase3", fileCount: 2500, duration: TimeSpan.FromMinutes(1), fileSizeInKB: 25 , 8192, 8);

        Console.WriteLine("Phase 1 tests completed. Check the results log for details.");

        Logger.Instance.StopLogging();
    }

    private static async Task RunTest(string testCase, int fileCount, TimeSpan duration, int fileSizeInKB , int buffersize = 8192, int threadcount = 1)
    {
        Console.WriteLine($"Running {testCase}: {fileCount} files, {duration.TotalMinutes} minutes, {fileSizeInKB} KB each.");

        // int threadCount = Environment.ProcessorCount;
        int threadCount = threadcount;
        Console.WriteLine($"Using {threadCount} threads for file generation...");
        Console.WriteLine($"buffersize: {buffersize}");

        // Prepare directories
        if (Directory.Exists(sourceDirectory))
        {
            Directory.Delete(sourceDirectory, true); // true for recursive deletion
        }

        if (Directory.Exists(targetDirectory))
        {
            Directory.Delete(targetDirectory, true); // true for recursive deletion
        }
        await Task.Delay(5000);

        Directory.CreateDirectory(sourceDirectory);
        Directory.CreateDirectory(targetDirectory);

        // ////////////////////////////// toremove
        // FileGenerator generator = new FileGenerator(sourceDirectory);
        // await generator.GenerateFiles(fileCount, duration, threadCount , fileSizeInKB);
        // ////////////////////////////// toremove

        // Start the watcher
        Watcher watcher = new Watcher(sourceDirectory, targetDirectory, buffersize);
        Task watcherTask = Task.Run(() => watcher.Start());

        await Task.Delay(5000); // Delay to ensure watcher is running

        // Generate files
        FileGenerator generator = new FileGenerator(sourceDirectory);
        await generator.GenerateFiles(fileCount, duration, threadCount , fileSizeInKB);

        Console.WriteLine($"File generation for {testCase} completed.");

        // Allow watcher to process all files
        Console.WriteLine($"Waiting for FileWatcher to process events for {testCase}...");
        await Task.Delay(5000); // Wait for some buffer time if needed

        // End watcher and log results
        Console.WriteLine($"Logging results for {testCase}...");
        Logger.Instance.LogEvent("INFO", $"{testCase} completed", DateTime.Now);
        ResultLogger.LogResult(testCase, fileCount, duration.ToString(), $"{fileSizeInKB}KB", buffersize, watcher.filesProcessed, watcher.eventDrops, "");

        Console.WriteLine($"{testCase} completed. Moving to the next test...");
    }
    private static async Task PromptUserToContinue()
    {
        Console.WriteLine("Press Enter to continue or type 'exit' to stop...");
        string input = Console.ReadLine();
        if (input?.ToLower() == "exit")
        {
            Console.WriteLine("Exiting...");
            Environment.Exit(0);
        }
    }
}
