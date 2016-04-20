using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using MonoTorrent;
using MonoTorrent.BEncoding;
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent.Client.PieceWriters;

namespace Creek.Tasks
{
    public class MetafileGenTask : TorrentCreator, IManagementTask
    {
        public MetafileGenTask()
        {
            Method = TaskMethod.CreateTorrent;
        }

        public string URL
        {
            get;
            private set;
        }

        public long ContentLength
        {
            get;
            private set;
        }

        public string TorrentName
        {
            get;
            private set;
        }

        private static long GetContentLength(string url)
        {
            try
            {
                HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.Method = "HEAD";
                // Try to get the content length from the HTTP response header
                using (HttpWebResponse webResp = (HttpWebResponse)(webReq.GetResponse()))
                {
                    return webResp.ContentLength;
                }
            }
            catch
            {
                return 0;
            }
        }

        public IAsyncResult BeginCreate(string url, AsyncCallback callback, object asyncState)
        {
            return BeginCreate(delegate { return Create(url); }, callback, asyncState);
        }

        private BEncodedDictionary Create(string url)
        {
            Check.Url(url);

            Uri uri = new Uri(url);
            URL = url; // The URL point to the file to be hashed
            ContentLength = GetContentLength(url); // Get the file length
            TorrentName = Path.GetFileName(uri.LocalPath); // Get the file name
            List<FileMapping> mappings = new List<FileMapping> { new FileMapping(URL, TorrentName) };

            if (ContentLength == 0)
                throw new ApplicationException(
                    string.Format(AppResource.UrlNotExist, url));

            List<TorrentFile> maps = new List<TorrentFile>();
            maps.Add(new TorrentFile(mappings[0].Destination, ContentLength, mappings[0].Source));
            return Create(TorrentName, maps);
        }

        #region TorrentCreator Members

        protected override PieceWriter CreateReader()
        {
            return new HttpRangeReader();
        }

        #endregion

        #region IManagementTask Members

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public TaskMethod Method
        {
            get;
            private set;
        }

        public object Result
        {
            get;
            set;
        }

        #endregion
    }
}
