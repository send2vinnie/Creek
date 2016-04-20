using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Common.Logging;
using Creek.Config;
using Creek.Tasks;
using Creek.Utility;
using Creek.JsonObject;
using Quartz;
using Newtonsoft.Json;

namespace Creek.Jobs
{
    public class ContentMonitorJob : IStatefulJob
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentMonitorJob));

        private static DateTime GetContentCreateDate(string sContentUniqueId)
        {
            string sDate = ContentGenJob.GetContentCreateDate(sContentUniqueId);
            if (sDate != null) return DateTime.Parse(sDate);
            return DateTime.Now;
        }

        private static ContentGenRecipes GetContentRecipes(string sContentUniqueId, string sContentHashCode)
        {
            string sRecipesJson = ContentGenJob.ReadContentRecipesJson(sContentUniqueId, sContentHashCode);
            if (sRecipesJson != null) return ContentGenRecipes.FromJson(sRecipesJson);
            return null;
        }

        private static void UpdateTorrentSeedDetail(ContentDetail oContentDetail, TorrentSeedDetail oDetail)
        {
            // Try to find the corresponding TorrentSeedDetail in the pre-created list
            // and update it with the given data
            foreach (VersionDetail oVerDetail in oContentDetail.Versions)
            {
                if (oVerDetail.Hash == oDetail.Hash)
                {
                    List<TorrentSeedDetail> oSeeds = oVerDetail.TorrentSeeds;
                    // Update the pre-created TorrentSeedDetail object contained in the VersionDetail's if any
                    TorrentSeedDetail oToBeUpdate = oSeeds.Find(o => { return o.IP == oDetail.IP; });
                    if (oToBeUpdate != null)
                    {
                        oToBeUpdate.Name = oDetail.Name;
                        oToBeUpdate.TotalSize = oDetail.TotalSize;
                        oToBeUpdate.PartDone = oDetail.PartDone;
                        oToBeUpdate.StatusCode = oDetail.StatusCode;
                        oToBeUpdate.DatePublished = oDetail.DatePublished;
                        oToBeUpdate.Error = oDetail.Error;
                    }
                }
            }
        }

        private static bool QueryTrackerTorrentDeploymentStatus(string sContentHashCode)
        {
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(
                string.Format(AppConfig.ContentMonitorJob.TrackerMonitorUrl + "/?InfoHash={0}",
                sContentHashCode)
                );

            if (AppConfig.ContentMonitorJob.TrackerAdminName == null ||
                AppConfig.ContentMonitorJob.TrackerAdminName.Length == 0)
            {
                webReq.UseDefaultCredentials = false;
            }
            else
            {
                NetworkCredential oNC = new NetworkCredential();
                oNC.UserName = AppConfig.ContentMonitorJob.TrackerAdminName;
                oNC.Password = AppConfig.ContentMonitorJob.TrackerAdminPassword;
                webReq.Credentials = oNC;
            }
            // Get only the header instead of the whole response body
            webReq.Method = "HEAD";
            try
            {
                // Try to get the response of the KAWA tracker monitor page
                using (HttpWebResponse webResp = (HttpWebResponse)(webReq.GetResponse()))
                {
                    StreamReader reader = new StreamReader(webResp.GetResponseStream());
                    string sResponse = reader.ReadToEnd();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private static string ProcessContentMonitor(List<string> listVersion, JobExecutionContext context)
        {
            JobDataMap oJobDataMap = context.JobDetail.JobDataMap;
            string[] listSeedWebIP = AppConfig.ContentDeployJob.OfficalSeedWebIPList;
            ContentDetail oContentDetail = new ContentDetail();

            // Construct the hierarchy per the Version list & registered Seed Web IP list first
            oContentDetail.Name = context.JobDetail.Name;
            oContentDetail.DateCreated = GetContentCreateDate(context.JobDetail.Name);
            foreach (string sVer in listVersion)
            {
                VersionDetail oVerDetail = new VersionDetail();
                ContentGenRecipes oContentRecipes = GetContentRecipes(oContentDetail.Name, sVer);

                if (oContentRecipes != null)
                {
                    oVerDetail.Name = oContentRecipes.ContentFileName;
                    oVerDetail.DateCreated = oContentRecipes.CreateDateTime;
                }
                else
                {
                    oVerDetail.Name = "";
                    oVerDetail.DateCreated = DateTime.MaxValue;
                }
                oVerDetail.Hash = sVer;
                oVerDetail.DeployToTracker = QueryTrackerTorrentDeploymentStatus(sVer);
                oContentDetail.Versions.Add(oVerDetail);
                foreach (string sIP in listSeedWebIP)
                {
                    TorrentSeedDetail oDetail = new TorrentSeedDetail();

                    // Create the TorrentSeedDetail first with the unknown status
                    // And replace the unkown status with the real status parsed from JSON later
                    oDetail.IP = sIP;
                    oDetail.Hash = oVerDetail.Hash.ToUpper();
                    oDetail.Name = "";
                    oDetail.TotalSize = 0;
                    oDetail.PartDone = 0f;
                    oDetail.StatusCode = TorrentStatus.Unknown;
                    oDetail.DatePublished = DateTime.MaxValue;
                    oDetail.Error = AppResource.SeedWebConnectionFailed;
                    oVerDetail.TorrentSeeds.Add(oDetail);
                }
            }
            // Enumerate all the registered seed web IP for walking through all the seed monitors
            foreach (string sIP in listSeedWebIP)
            {
                JobDetail oSeedJob = context.Scheduler.GetJobDetail(
                    sIP, SeedMonitorConstants.SeedMonitorGroupName);
                // Check to see if the seed monitor job exists
                if (oSeedJob != null)
                {
                    string sTorrentDetails = (string)oSeedJob.JobDataMap[SeedMonitorJobDataMapConstants.TorrentDetails];
                    // Check to see if the seed monitor has torrent detail JSON ready
                    if (sTorrentDetails != null)
                    {
                        List<TorrentDetail> listTorrentDetail = JsonConvert.DeserializeObject<List<TorrentDetail>>(sTorrentDetails);
                        List<string> listTorrentInSeed = new List<string>();
                        // Enumerate all the TorrentDetail to update to the pre-created TorrentSeedDetail
                        foreach (TorrentDetail oTorrentDetail in listTorrentDetail)
                        {
                            TorrentSeedDetail oDetail = new TorrentSeedDetail();
                            oDetail.IP = sIP;
                            oDetail.Hash = oTorrentDetail.UniqueID.ToUpper();
                            oDetail.Name = oTorrentDetail.Name;
                            oDetail.TotalSize = oTorrentDetail.TotalSize;
                            oDetail.PartDone = oTorrentDetail.PartDone;
                            oDetail.StatusCode = oTorrentDetail.StatusCode;
                            oDetail.DatePublished = oTorrentDetail.DateAdded;
                            oDetail.Error = oTorrentDetail.Error;
                            // Use the TorrentSeedDetail object to update the corresponding list
                            // in the VersionDetails list so the JSON just needs to be parsed just once
                            UpdateTorrentSeedDetail(oContentDetail, oDetail);
                            // Add the torrent hash for later use of checking the exclusion of the torrents
                            listTorrentInSeed.Add(oDetail.Hash);
                        }
                        // Update TorrentSeedDetail not found in the seed's torrent list to "content not deployed" error
                        foreach (VersionDetail oVersion in oContentDetail.Versions.FindAll
                            (x => { return !listTorrentInSeed.Contains(x.Hash); }))
                        {
                            TorrentSeedDetail oTorrentSeed = oVersion.TorrentSeeds.Find(x => { return x.IP == sIP; });
                            if (oTorrentSeed != null) oTorrentSeed.Error = AppResource.ContentNotDeployToSeed;
                        }
                    }
                }
            }
            // Serialize the content detail to JSON and return
            return JsonConvert.SerializeObject(oContentDetail);
        }

        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            try
            {
                //=============================================================================
                log.InfoFormat(AppResource.StartJobExecution, typeof(ContentMonitorJob).Name);
                //=============================================================================

                // Deserialize the given JSON to the version list
                // Make sure the JobDataMap carries only value type here
                // to prevent the concurrency issue when using reference type
                List<string> listVersion = ContentGenJob.GetContentVersionList(context.JobDetail.Name);
                // Do the monitoring
                string sJson = ProcessContentMonitor(listVersion, context);
                // Add returned content detail (JSON) into the job data map to avoid the concurrency issue
                context.JobDetail.JobDataMap[ContentMonitorJobDataMapConstants.ContentDetail] = sJson;

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(ContentMonitorJob).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                // Remove the entry from the job data map to refresh the content details
                context.JobDetail.JobDataMap.Remove(ContentMonitorJobDataMapConstants.ContentDetail);

                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentMonitorJob).Name, oEx.Message);
                //===================================================================================================
            }
        }

        #endregion
    }

    // This is used to do the re-schedule of the content monitor job
    // as a work-around of the unschedule-issue encountered when removing
    // the job from the Quartz scheduler
    public class ContentMonitorDummyJob : IJob
    {
        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            return;
        }

        #endregion
    }
}
