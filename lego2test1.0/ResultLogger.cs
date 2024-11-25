using System.IO;

public static class ResultLogger
{
    private static readonly string resultsFilePath = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Logs\phase1_results.csv";

    static ResultLogger()
    {
        InitializeResultLogger();
    }

    private static void InitializeResultLogger()
    {

        Console.WriteLine($"ResultLogger init started");
            string resultDirectory = Path.GetDirectoryName(resultsFilePath);
            Console.WriteLine(resultDirectory);

            // Ensure the logs directory exists
            if (!Directory.Exists(resultDirectory))
            {
                Directory.CreateDirectory(resultDirectory);
            }

        if (!File.Exists(resultsFilePath))
        {
            using (var writer = File.CreateText(resultsFilePath))
            {
                writer.WriteLine("TestCase,FileCount,TimeSpan,FileSize,BufferSize,FilesProcessed,EventDrop,ErrorsObserved,Notes");
            }
        }
    }

    public static void LogResult(string testCase, int fileCount, string timeSpan, string fileSize, int buffersize,
                                 int filesProcessed, double eventDropPercentage, string errorsObserved, string notes = "")
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(resultsFilePath, true))
            {
                writer.WriteLine($"{testCase},{fileCount},{timeSpan},{fileSize},{buffersize},{filesProcessed},{eventDropPercentage},{errorsObserved},{notes}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error logging result: {ex.Message}");
        }
    }
}
