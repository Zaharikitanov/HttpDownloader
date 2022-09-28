namespace httpDownloader.Services
{
    public class DownloaderService
    {
        private static volatile int _totalBytes = 0;
        private static DateTime _lastEpox = DateTime.Now;
        public static int MaxThrottledBytes { get; set; }

        public static void ReadFully(Stream input, string outputPath)
        {
            int defaultBuffer = 16 * 1024;
            int bufferSize = defaultBuffer > MaxThrottledBytes ? MaxThrottledBytes : defaultBuffer;
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
                    if (_totalBytes + buffer.Length > MaxThrottledBytes)
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
