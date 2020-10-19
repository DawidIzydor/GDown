using System;

namespace GHent.Models
{
    public interface IRequest
    {
        Uri DownloadPath { get; }
        string SavePath { get; }
    }
}
