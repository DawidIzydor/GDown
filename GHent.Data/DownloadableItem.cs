namespace GHent.Data
{
    public record DownloadableItem
    {
        public required string Url { get; set; }
        public required DownloadStatus Status { get; set; }

        public required string SavePath { get; set; }
        public required bool SaveCbr { get; set; }

    }
}
