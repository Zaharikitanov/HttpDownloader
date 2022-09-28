using httpDownloader.Extensions;
using httpDownloader.Models;
using System.Text;

namespace httpDownloader.Services
{
    public class FileService
    {
        public void DownloadFilesConcurrently(InputParametersModel settings)
        {
            var preparedData = PrepareDataForProcessing(settings);

            PrepareTargetDirectory(settings.LocalStoragePath);

            foreach (var concurrentCollection in preparedData)
            {
                DownloaderService.MaxThrottledBytes = settings.DownloadSpeedInBytesPerSecond;

                foreach (var item in concurrentCollection)
                {
                    new Thread(() => {
                        DownloadFile(item, settings.LocalStoragePath);
                    }).Start();
                }
            }
        }

        private void DownloadFile(ItemForDownload item, string localStoragePath)
        {
            try
            {
                using var client = new HttpClient();
                var sourceAsByteArray = client.GetByteArrayAsync(item.UrlForDownload).GetAwaiter().GetResult();
                using var fileStream = new MemoryStream(sourceAsByteArray);

                Console.WriteLine($"Downloading {item.LocalFileName} on Thread {Thread.CurrentThread.ManagedThreadId}");

                DownloaderService.ReadFully(fileStream, Path.Combine(localStoragePath, item.LocalFileName));

                Console.WriteLine($"Downloaded {item.LocalFileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(DownloadFile)}]: {ex.Message} on file {item.LocalFileName}");
                return;
            }
        }

        private List<List<ItemForDownload>> PrepareDataForProcessing(InputParametersModel settings)
        {
            var itemsList = RetrieveUrlsAndFilenamesFromFile(settings.SourceFile);
            return itemsList.ChunkBy(settings.ConcurrentThreadsAmount);
        }

        private void PrepareTargetDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string ReadFile(string filePath)
        {
            string fileContent;

            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
            {
                fileContent = streamReader.ReadToEnd();
            }
            return fileContent;
        }

        private List<ItemForDownload> RetrieveUrlsAndFilenamesFromFile(string filePath)
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
