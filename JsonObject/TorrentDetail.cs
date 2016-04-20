using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json;

namespace Creek.JsonObject
{
    public enum Priority
    {
        Off = 0,
        Low = 1,
        Normal = 2,
        High = 3
    }

    public enum TorrentStatus
    {
        Waiting = 1,
        Checking = 2,
        Downloading = 4,
        Seeding = 8,
        Paused = 16,
        Queued = 32,
        Error = 64,
        Unknown = 0
    }

    public class TorrentDetail
    {
        private long id;
        private string hash;
        private string name;
        TorrentStatus statusCode;
        private string locationDir;

        private int rateDownload;
        private int rateUpload;
        private int peersGettingFromUs;
        private int peersSendingToUs;
        private int peersConnected;
        private int peersKnown;
        private int eta;

        private long downloadedEver;
        private long uploadedEver;
        private long totalSize;
        private float partDone;
        private float available;
        private String label;

        private DateTime dateAdded;
        private DateTime dateDone;
        private string error;

        //public long getID() { return id; }
        //public String getHash() { return hash; }
        public string Name { get { return name; } set { name = value; } }
        public TorrentStatus StatusCode { get { return statusCode; } set { statusCode = value; } }
        public string LocationDir { get { return locationDir; } set { locationDir = value; } }

        public int RateDownload { get { return rateDownload; } set { rateDownload = value; } }
        public int RateUpload { get { return rateUpload; } set { rateUpload = value; } }
        public int PeersGettingFromUs { get { return peersGettingFromUs; } set { peersGettingFromUs = value; } }
        public int PeersSendingToUs { get { return peersSendingToUs; } set { peersSendingToUs = value; } }
        public int PeersConnected { get { return peersConnected; } set { peersConnected = value; } }
        public int PeersKnown { get { return peersKnown; } set { peersKnown = value; } }
        public int Eta { get { return eta; } set { eta = value; } }

        public long DownloadedEver { get { return downloadedEver; } set { downloadedEver = value; } }
        public long UploadedEver { get { return uploadedEver; } set { uploadedEver = value; } }
        public long TotalSize { get { return totalSize; } set { totalSize = value; } }
        public float PartDone { get { return partDone; } set { partDone = value; } }
        public float Availability { get { return available; } set { available = value; } }
        public string LabelName { get { return label; } set { label = value; } }

        public DateTime DateAdded { get { return dateAdded; } set { dateAdded = value; } }
        public DateTime DateDone { get { return dateDone; } set { dateDone = value; } }
        public string Error { get { return error; } set { error = value; } }

        public TorrentDetail() { }
        public TorrentDetail(
                long id, string hash, string name, TorrentStatus statusCode,
                string locationDir, int rateDownload, int rateUpload,
                int peersGettingFromUs, int peersSendingToUs,
                int peersConnected, int peersKnown, int eta,
                long downloadedEver, long uploadedEver, long totalSize,
                float partDone, float available, string label,
                DateTime dateAdded, string error)
        {
            this.id = id;
            this.hash = hash;
            this.name = name;
            this.statusCode = statusCode;
            this.locationDir = locationDir;

            this.rateDownload = rateDownload;
            this.rateUpload = rateUpload;
            this.peersGettingFromUs = peersGettingFromUs;
            this.peersSendingToUs = peersSendingToUs;
            this.peersConnected = peersConnected;
            this.peersKnown = peersKnown;
            this.eta = eta;

            this.downloadedEver = downloadedEver;
            this.uploadedEver = uploadedEver;
            this.totalSize = totalSize;
            this.partDone = partDone;
            this.available = available;
            this.label = label;

            this.dateAdded = dateAdded;
            DateTime dt;
            if (eta == -1 || eta == -2)
            {
                dt = new DateTime(9999, 12, 31);
            }
            else
            {
                dt = DateTime.Now.AddSeconds(eta);
            }
            this.dateDone = dt;
            this.error = error;
        }

        /**
         * Returns the torrent-specific ID, which is the torrent's hash or (if not available) the long number
         * @return The torrent's (session-transient) unique ID
         */
        public string UniqueID
        {
            get
            {
                if (this.hash == null)
                {
                    return "" + this.id;
                }
                else
                {
                    return this.hash;
                }
            }
            set
            {
                this.hash = value;
            }
        }

        /**
         * Gives the upload/download seed ratio. If not downloading, 
         * it will base the ratio on the total size; so if you created the torrent yourself
         * you will have downloaded 0 bytes, but the ratio will pretend you have 100%.
         * @return The ratio in range [0,r]
         */
        [JsonIgnore]
        public double Ratio
        {
            get
            {
                if (statusCode == TorrentStatus.Downloading)
                {
                    return ((double)uploadedEver) / ((double)downloadedEver);
                }
                else
                {
                    return ((double)uploadedEver) / ((double)totalSize);
                }
            }
        }

        /**
         * Gives the percentage of the download that is completed
         * @return The downloaded percentage in range [0,1]
         */
        [JsonIgnore]
        public float DownloadedPercentage
        {
            get
            {
                return partDone;
            }
        }

        /**
         * Indicates if the torrent can be paused at this moment
         * @return If it can be paused
         */
        [JsonIgnore]
        public bool CanPause
        {
            get
            {
                // Can pause when it is downloading or seeding
                return statusCode == TorrentStatus.Downloading || statusCode == TorrentStatus.Seeding;
            }
        }

        /**
         * Indicates whether the torrent can be resumed
         * @return If it can be resumed
         */
        [JsonIgnore]
        public bool CanResume
        {
            get
            {
                // Can resume when it is paused
                return statusCode == TorrentStatus.Paused;
            }
        }

        /**
         * Indicates if the torrent can be started at this moment
         * @return If it can be started
         */
        [JsonIgnore]
        public bool CanStart
        {
            get
            {
                // Can start when it is queued
                return statusCode == TorrentStatus.Queued;
            }
        }

        /**
         * Indicates whether the torrent can be stopped
         * @return If it can be stopped
         */
        [JsonIgnore]
        public bool CanStop
        {
            get
            {
                // Can stop when it is downloading or seeding or paused
                return statusCode == TorrentStatus.Downloading || statusCode == TorrentStatus.Seeding || statusCode == TorrentStatus.Paused;
            }
        }
    }
}
