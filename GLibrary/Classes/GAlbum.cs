using System.Collections.Generic;
using System.Linq;
using GHentDownloaderLibrary.Abstract;
using GHentDownloaderLibrary.Helpers;
using HtmlAgilityPack;

namespace GHentDownloaderLibrary.Classes
{
    public class GAlbum : Album<GImage>
    {
        private string _name;

        public new string Name
        {
            get
            {
                if (_name == "" && Web != null)
                {
                    ParseName();
                }

                return _name;
            }
            set => _name = value;
        }

        public override CombinedItem Parse()
        {
            Verify();

            Items = GetImagesQueue();

            return this;
        }

        private Queue<GImage> GetImagesQueue()
        {
            Queue<GImage> images = new Queue<GImage>();

            for (int page = 0; page < PagesNumberFromDocument(); ++page)
            {
                EnqueueImagesFromPage(page, images);
            }

            return images;
        }

        private void EnqueueImagesFromPage(int page, Queue<GImage> imagesQueue)
        {
            foreach (HtmlNode el in Web.Load(NetPath + "?p=" + page).GetElementbyId("gdt").ChildNodes
                .Where(el => el.Attributes["class"].Value != "gdtm"))
            {
                int imageNumber = 0;
                imagesQueue.Enqueue(new GImage
                {
                    NetPath = el.SelectSingleNode(el.XPath + "//div//a").Attributes["href"].Value,
                    SaveDirectoryPath = $"{SaveDirectoryPath}\\{Name}",
                    Name = $"{Name}_{page:D2}_{imageNumber:D3}"
                });
            }
        }

        private int PagesNumberFromDocument()
        {
            return Web.Load(NetPath).DocumentNode.ChildNodes[2].ChildNodes[3].ChildNodes[12].ChildNodes[1].ChildNodes[0]
                       .ChildNodes.Count - 2;
        }

        private void ParseName()
        {
            Name = StringHelper.RemoveIllegalCharacters(ParseNameFromDocument());
        }

        private string ParseNameFromDocument()
        {
            return Web.Load(NetPath).GetElementbyId("gn").InnerHtml;
        }
    }
}