namespace GHent.Database.Models
{
    public class Album
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public WatchMode Watch { get; set; }
    }
}