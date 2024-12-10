using System;
using System.IO;
using System.Threading.Tasks;

public class FileGenerator
{
    private readonly string directoryPath;

    public FileGenerator(string dirPath)
    {
        directoryPath = dirPath;
        Directory.CreateDirectory(directoryPath);
    }

    public async Task GenerateFiles(int fileCount, TimeSpan duration, int threadCount, int fileSizeInKB)
    {
        int delayBetweenFiles = (int)(duration.TotalMilliseconds / fileCount);
        SemaphoreSlim semaphore = new SemaphoreSlim(threadCount);

        // Create the fixed-size content once and reuse it
        string content = new string('A', fileSizeInKB * 1024); // Generate content to match file size

        var tasks = Enumerable.Range(0, fileCount).Select(async i =>
        {
            await semaphore.WaitAsync(); // Wait indefinitely to acquire the semaphore

            try
            {
                // small file name
                string filePath = Path.Combine(directoryPath, $"testfile_{i}.txt");
                // large file name
                // string filePath = Path.Combine(directoryPath, $"testfileeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeefedaereeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee_{i}.txt");

                //simulate update
                // string filePath = Path.Combine(directoryPath, $"testfile_0.txt");

                // Try-catch block for file writing
                try
                {
                    //for create
                    await File.WriteAllTextAsync(filePath, content); // Write fixed-size content

                    //for update
                    // await File.WriteAllTextAsync(filePath, DateTime.Now.ToString()); // Write current timestamp as string
                    Console.WriteLine($"Generated file: {filePath}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"IO Error writing to file {filePath}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error for file {filePath}: {ex.Message}");
                }

                // Add delay after each file generation
                // await Task.Delay(delayBetweenFiles); // Control creation rate
            }
            finally
            {
                semaphore.Release(); // Release semaphore
            }
        });

        await Task.WhenAll(tasks);

        // Cleanup: ensure semaphore is disposed properly
        semaphore.Dispose();
    }

}
