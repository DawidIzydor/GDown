using System;

namespace GHent.Models
{
    public record Request : IRequest
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}