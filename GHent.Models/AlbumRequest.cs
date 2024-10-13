using System;

namespace GHent.Models
{
    public class AlbumRequest : IRequest
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}