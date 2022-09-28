using httpDownloader.Models;
using System.CommandLine;

namespace httpDownloader.Services
{
    public class InputProcessorService
    {
        public async Task ProcessInput(string[] args)
        {
            var rootCommand = new RootCommand();

            var sourceFilePath = new Option<string>("--sourceFile-path");
            sourceFilePath.AddAlias("-f");

            var localStoragePath = new Option<string>("--localStorage-path");
            localStoragePath.AddAlias("-o");

            var concurrentThreadsAmount = new Option<int>("--concurrent-threads-amount");
            concurrentThreadsAmount.AddAlias("-n");

            var downloadSpeedInBytesPerSecond = new Option<int>("--downloadSpeed-in-bytes-per-second");
            downloadSpeedInBytesPerSecond.AddAlias("-l");

            rootCommand.AddOption(sourceFilePath);
            rootCommand.AddOption(localStoragePath);
            rootCommand.AddOption(concurrentThreadsAmount);
            rootCommand.AddOption(downloadSpeedInBytesPerSecond);

            rootCommand.SetHandler((
                sourceFilePath,
                localStoragePath,
                concurrentThreadsAmount,
                downloadSpeedInBytesPerSecond) =>
            {
                var fileService = new FileService();
                var parametersModel = new InputParametersModel();

                parametersModel.SourceFile = sourceFilePath;
                parametersModel.LocalStoragePath = localStoragePath;
                parametersModel.ConcurrentThreadsAmount = concurrentThreadsAmount;
                parametersModel.DownloadSpeedInBytesPerSecond = downloadSpeedInBytesPerSecond;

                fileService.DownloadFilesConcurrently(parametersModel);
            },
                sourceFilePath,
                localStoragePath,
                concurrentThreadsAmount,
                downloadSpeedInBytesPerSecond);

            await rootCommand.InvokeAsync(args);
        } 
    }
}
