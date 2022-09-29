using httpDownloader.Services;

namespace httpDownloader
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            //httpDownloader.exe -f urls.txt -o downloads -n 2 -l 100000
            var inputProcessor = new InputProcessorService();
            await inputProcessor.ProcessInput(args);
        }
    }
}