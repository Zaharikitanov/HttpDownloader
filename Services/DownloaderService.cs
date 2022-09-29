using httpDownloader.Models;

namespace httpDownloader.Services
{
    public class DownloaderService
    {
        private volatile int _totalBytes;
        private DateTime _lastEpox;
        public int _maxThrottledBytes;

        public DownloaderService(int maxThrottledBytes)
        {
            _totalBytes = 0;
            _lastEpox = DateTime.Now;
            _maxThrottledBytes = maxThrottledBytes;
        }

        public void DownloadFile(ItemForDownload item, string localStoragePath)
        {
            try
            {
                using var client = new HttpClient();
                var sourceAsByteArray = client.GetByteArrayAsync(item.UrlForDownload).GetAwaiter().GetResult();
                using var fileStream = new MemoryStream(sourceAsByteArray);

                Console.WriteLine($"Downloading {item.LocalFileName} on Thread {Thread.CurrentThread.ManagedThreadId}");

                ReadFully(fileStream, Path.Combine(localStoragePath, item.LocalFileName));

                Console.WriteLine($"Downloaded {item.LocalFileName}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{nameof(DownloadFile)}]: {ex.Message} on file {item.LocalFileName}");
                return;
            }
        }

        private void ReadFully(Stream input, string outputPath)
        {
            int defaultBuffer = 16 * 1024;
            int bufferSize = defaultBuffer > _maxThrottledBytes ? _maxThrottledBytes : defaultBuffer;
            byte[] buffer = new byte[bufferSize];

            using (FileStream localFileStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            {
                int read;
                for ( ; ; )
                {
                    // Throttle
                    DateTime now = DateTime.Now;
                    TimeSpan difference = now - _lastEpox;
                    if (difference.TotalMilliseconds > 1000)
                    {
                        _lastEpox = now;
                        _totalBytes = 0;
                    }
                    // Download
                    if (_totalBytes + buffer.Length > _maxThrottledBytes)
                    {
                        Thread.Sleep(10);
                    }
                    else
                    {
                        read = input.Read(buffer, 0, buffer.Length);
                        if (read > 0)
                        {
                            localFileStream.Write(buffer, 0, read);
                            _totalBytes += read;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
