using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        string directoryPath1 = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Source1";
        string directoryPath2 = @"C:\Users\rohandassani\AzureMigrateRepos\rohan-lego-test2\Source2";

        Console.WriteLine("##### Before Starting FileWatcherService #####");
        FileWatcherService fileWatcherService = new FileWatcherService();

        Console.WriteLine("##### Before Creating Partner #####");
        Partner partner1 = new Partner("Partner1", directoryPath1, fileWatcherService);
        Partner partner2 = new Partner("Partner2", directoryPath2, fileWatcherService);

        Console.WriteLine("##### Before Starting Partner #####");
        partner1.Start();
        partner2.Start();

        Console.WriteLine("Partners started. Press Enter to stop...");
        Console.ReadLine();

        Console.WriteLine("##### Before Stopping Partner #####");
        partner1.Stop();
        partner2.Stop();

        Console.WriteLine("Partners stopped. Press Enter to exit...");
        Console.ReadLine();

        //todo: scale testing with file generator
        //todo: add more partners
        //todo: failure testing
        //todo: failures in subcribe/unsibscribe
    }
}