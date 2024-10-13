using System;

namespace GHent.Shared.Request
{
    public record Request : IRequest
    {
        public Uri DownloadPath { get; set; }
        public string SavePath { get; set; }
    }
}