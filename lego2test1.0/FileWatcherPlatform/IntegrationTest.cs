// using System;
// using System.IO;
// using System.Threading.Tasks;

// public class IntegrationTest
// {
//     public static async Task Main(string[] args)
//     {
//         string directoryPath1 = @"C:\Your\Directory\Path1";
//         string directoryPath2 = @"C:\Your\Directory\Path2";

//         FileWatcherService fileWatcherService = new FileWatcherService();

//         Partner partner1 = new Partner("Partner1", directoryPath1, fileWatcherService);
//         Partner partner2 = new Partner("Partner2", directoryPath2, fileWatcherService);

//         partner1.Start();
//         partner2.Start();

//         Console.WriteLine("Partners started. Press Enter to stop...");
//         Console.ReadLine();

//         partner1.Stop();
//         partner2.Stop();

//         Console.WriteLine("Partners stopped. Press Enter to exit...");
//         Console.ReadLine();
//     }
// }