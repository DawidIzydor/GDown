using System;

namespace GHent.Models
{
    public class ImageRequest : IRequest
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}