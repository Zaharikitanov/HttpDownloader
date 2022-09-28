namespace httpDownloader.Models
{
    public class InputParametersModel
    {
        public string SourceFile { get; set; } = string.Empty;

        public string LocalStoragePath { get; set; } = string.Empty;

        public int ConcurrentThreadsAmount { get; set; }

        public int DownloadSpeedInBytesPerSecond { get; set; }
    }
}
