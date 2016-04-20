using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.JsonObject
{
    public class ContentDetail
    {
        public string Name;
        public DateTime DateCreated; // The create date of the content directory
        public List<VersionDetail> Versions;

        public ContentDetail()
        {
            Versions = new List<VersionDetail>();
        }
    }

    public class VersionDetail
    {
        public string Hash;
        public string Name;
        public DateTime DateCreated; // The create date of the receipts file
        public bool DeployToTracker;
        public float SeedDeploymentRatio;
        public float SeedingRatio;
        public List<TorrentSeedDetail> TorrentSeeds;

        public VersionDetail()
        {
            TorrentSeeds = new List<TorrentSeedDetail>();
        }
    }

    public class TorrentSeedDetail
    {
        public string IP;
        public string Hash;
        public string Name;
        public long TotalSize;
        public DateTime DatePublished;
        public float PartDone;
        public TorrentStatus StatusCode;
        public string Error;
    }
}
