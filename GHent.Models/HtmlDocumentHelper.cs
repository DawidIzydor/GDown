using GHent.Shared.Request;
using HtmlAgilityPack;
using System;
using System.IO;

namespace GHent.Shared
{
    public static class HtmlDocumentHelper
    {
        public static string ExtractSavePath(this HtmlDocument document, IRequest request, string albumTitleXPath)
        {
            var albumTitle = document.GetAlbumTitle(albumTitleXPath);
            var albumRequestSavePath = request.SavePath;

            string savePath = CombinePaths(albumTitle, albumRequestSavePath);
            EnsurePathExists(savePath);
            return savePath;
        }

        private static string CombinePaths(string albumTitle, string albumRequestSavePath)
        {
            var savePath = Path.Combine(albumRequestSavePath, albumTitle);
            if (savePath.Length > 128)
            {
                savePath = savePath[0..127];
            }

            return savePath;
        }

        private static void EnsurePathExists(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        public static string GetAlbumTitle(this HtmlDocument document, string albumTitleXPath)
        {
            var nameNode = document.DocumentNode.SelectSingleNode(albumTitleXPath);
            if (nameNode == null)
            {
                throw new InvalidOperationException("Album title not found");
            }

            return nameNode.InnerHtml.RemoveIllegalCharacters();
        }

        public static HtmlNodeCollection GetThumbnailNodes(this HtmlDocument document, IRequest request, string thumbnailNodesXPath)
        {
            var thumbContainerNodes = document.DocumentNode.SelectNodes(thumbnailNodesXPath);
            return thumbContainerNodes is null
                ? throw new ArgumentNullException($"Thumb containers nodes null: {request.DownloadPath}")
                : thumbContainerNodes;
        }
    }
}