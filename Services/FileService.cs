using httpDownloader.Extensions;
using httpDownloader.Models;
using System.Text;
using System.Linq;

namespace httpDownloader.Services
{
    public class FileService
    {
        class PreparedData
        {
            private readonly List<ItemForDownload> data;
            private volatile int nextFileIdx;

            public PreparedData(InputParametersModel settings)
            {
                data = RetrieveUrlsAndFilenamesFromFile(settings.SourceFile);
                nextFileIdx = 0;
            }

            public ItemForDownload? GetNextFile()
            {
                lock (data)
                {
                    if (data.Count <= nextFileIdx)
                        return null;
                    return data[nextFileIdx++];
                }
            }
        }

        private DownloaderService _downloadService;
        private InputParametersModel _settings;

        public FileService(InputParametersModel settings)
        {
            _settings = settings;
            _downloadService = new DownloaderService(_settings.DownloadSpeedInBytesPerSecond);
        }

        public void DownloadFilesConcurrently()
        {
            var preparedData = new PreparedData(_settings);

            PrepareTargetDirectory(_settings.LocalStoragePath);

            var workerThreads = new List<Thread>();
            for (var i = 0; i < _settings.ConcurrentThreadsAmount; ++i)
            {
                workerThreads.Add(new Thread(() => WorkerThreadLoop(_downloadService, preparedData, _settings)));
            }

            workerThreads.ForEach(x => x.Start());
            workerThreads.ForEach(x => x.Join());
        }

        private static void WorkerThreadLoop(
            DownloaderService downloaderService,
            PreparedData preparedData,
            InputParametersModel settings
            )
        {
            ItemForDownload? file;
            while ((file = preparedData.GetNextFile()) != null)
            {
                downloaderService.DownloadFile(file, settings.LocalStoragePath);
            }
            Console.WriteLine($"Worker Thread {Thread.CurrentThread.ManagedThreadId} has nothing to do so it's going home. 🎉");
        }

        private void PrepareTargetDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static string ReadFile(string filePath)
        {
            string fileContent;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                fileContent = streamReader.ReadToEnd();
            }
            return fileContent;
        }

        private static List<ItemForDownload> RetrieveUrlsAndFilenamesFromFile(string filePath)
        {
            var urlsAndFilenamesCollection = new List<ItemForDownload>();
            var fileContent = ReadFile(filePath);
            var formattedContent = fileContent.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < formattedContent.Length; i++)
            {
                if (formattedContent[i].IsValidUrl())
                {
                    if (!formattedContent[i + 1].IsValidUrl())
                    {
                        urlsAndFilenamesCollection.Add(
                            new ItemForDownload
                            {
                                UrlForDownload = formattedContent[i],
                                LocalFileName = formattedContent[i + 1]
                            }
                        );
                    }
                    else
                    {
                        //handling case if there is url but no filename provided and extractring the file name from the url
                        if (formattedContent[i].UrlContainsFile(out string result))
                        {
                            urlsAndFilenamesCollection.Add(
                                new ItemForDownload
                                {
                                    UrlForDownload = formattedContent[i],
                                    LocalFileName = result
                                }
                            );
                        } 
                    }
                }
            }
            return urlsAndFilenamesCollection;
        }
    }
}
