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
                string filePath = Path.Combine(directoryPath, $"testfile_{i}.txt");
                // string filePath = Path.Combine(directoryPath, $"testfileeeeeeeeeeeeeeeeeeee_{i}.txt");

                // Try-catch block for file writing
                try
                {
                    await File.WriteAllTextAsync(filePath, content); // Write fixed-size content
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
                await Task.Delay(delayBetweenFiles); // Control creation rate
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
