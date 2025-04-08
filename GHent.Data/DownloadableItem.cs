namespace GHent.Data
{
    public record DownloadableItem
    {
        public required string Url { get; set; }
        public required DownloadStatus Status { get; set; }

        public required string SavePath { get; set; }
        public required bool SaveCbr { get; set; }

        public string? Title { get; set; }

        public HashSet<string>? Tags { get; set; }

        public DateTime Created { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
