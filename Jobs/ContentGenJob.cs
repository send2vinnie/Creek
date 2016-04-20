using System;
using System.Collections.Generic;
using System.IO;
using Common.Logging;
using System.Threading;
using Creek.Config;
using Creek.JsonObject;
using Creek.Tasks;
using Creek.Utility;
using MonoTorrent.BEncoding;
using MonoTorrent.Common;
using Quartz;

namespace Creek.Jobs
{
    public class ContentGenJob : IJob, IInterruptableJob
    {
        public const string TorrentInfoDictionaryKey = "info";
        public const string TorrentContentNameKey = "name";
        public const string TorrentInfoAnnounceKey = "announce";
        public const string TorrentInfoUrlListKey = "url-list";
        public const string ContentWorkingPath = "Contents";
        public const string IconResPath = "Icons";
        public const string DisclaimerResPath = "Disclaimers";
        public const string ContentVerFileExtension = ".versioning";
        public const string ContentRecipesFileExtension = ".recipes";
        public const string HistoryFileExtension = ".history";
        public const string TorrentExtension = ".torrent";
        public const string TorrentVipExtension = ".vip.torrent";
        public const string TorrentFqdnExtension = ".fqdn.torrent";
        public const string DownloaderExtension = ".exe";
        public const string DisclaimerExtension = ".rtf";

        // Logging
        private bool _InterruptFlag;
        internal static readonly object sigLock = new object();
        internal static readonly object sigResUpdateLock = new object();
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentDeployJob));

        static ContentGenJob()
        {
            lock (ContentGenJob.sigLock)
            {
                // Assign the culture information of the resource manager
                AppResource.Culture = Thread.CurrentThread.CurrentCulture;
                // Check if the working directory exist or create it
                if (!Directory.Exists(ContentWorkingPath))
                    Directory.CreateDirectory(ContentWorkingPath);
            }
        }

        internal static List<string> GetHistoryList(ContentRecipesPropHistory tagHistoryName, string sContentUniqueId, string sDefaultHistoryValue)
        {
            lock (sigLock)
            {
                string sHistoryFile =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    tagHistoryName.ToString() + HistoryFileExtension;

                if (!File.Exists(sHistoryFile))
                {
                    if (sDefaultHistoryValue == null)
                    {
                        return new List<string>(new string[0]);
                    }
                    else
                    {
                        return new List<string>(new string[] { sDefaultHistoryValue });
                    }
                }
                else
                {
                    string[] allHistoryValues = File.ReadAllLines(sHistoryFile);
                    if (allHistoryValues.Length > 0)
                    {
                        return new List<string>(allHistoryValues);
                    }
                    else
                    {
                        if (sDefaultHistoryValue == null)
                        {
                            return new List<string>(new string[0]);
                        }
                        else
                        {
                            return new List<string>(new string[] { sDefaultHistoryValue });
                        }
                    }
                }
            }
        }

        private static void AppendHistoryFile(ContentRecipesPropHistory tagHistoryName, string sContentUniqueId, string sHistoryValue)
        {
            lock (sigLock)
            {
                if (sHistoryValue.Trim().Length > 0)
                {
                    string sHistoryFile =
                        ContentWorkingPath + "\\" +
                        sContentUniqueId + "\\" +
                        tagHistoryName.ToString() + HistoryFileExtension;

                    if (!File.Exists(sHistoryFile))
                    {
                        CheckAndCreateContentVersionFile(sContentUniqueId);
                        File.WriteAllLines(
                            sHistoryFile,
                            new string[] { sHistoryValue.Trim() });
                    }
                    else
                    {
                        string[] allHistoryValues = File.ReadAllLines(sHistoryFile);
                        List<string> listAllHistoryValues = new List<string>(allHistoryValues);
                        if (!listAllHistoryValues.Exists((x) => { return sHistoryValue == x; }))
                        {
                            File.AppendAllLines(
                                sHistoryFile,
                                new string[] { sHistoryValue.Trim() });
                        }
                    }
                }
            }
        }

        private static void SaveContentRecipesJson(string sContentUniqueId, string sContentHashCode, string sContentRecipesJson)
        {
            lock (sigLock)
            {
                CheckAndCreateContentVersionFile(sContentUniqueId);
                File.WriteAllText(
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + ContentRecipesFileExtension,
                    sContentRecipesJson);
            }
        }

        internal static string ReadContentRecipesJson(string sContentUniqueId, string sContentHashCode)
        {
            lock (sigLock)
            {
                string sContentRecipesJsonFile =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + ContentRecipesFileExtension;

                if (File.Exists(sContentRecipesJsonFile))
                {
                    return File.ReadAllText(sContentRecipesJsonFile);
                }
                else
                {
                    return ContentGenRecipes.GetJson(
                        new ContentGenRecipes()
                        {
                            AutoDeploy = true,
                            UPXCompression = true
                        }
                    );
                }
            }
        }

        private static void CheckAndCreateContentVersionFile(string sContentUniqueId)
        {
            lock (sigLock)
            {
                string sContentFolder =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\";
                string sContentVerFile =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentUniqueId + ContentVerFileExtension;

                if (!Directory.Exists(sContentFolder))
                {
                    // Create the content specific folder
                    Directory.CreateDirectory(sContentFolder);
                }
                if (!File.Exists(sContentVerFile))
                {
                    // Create the versioning file
                    File.WriteAllText(sContentVerFile, "");
                }
            }
        }

        internal static string GetContentCreateDate(string sContentUniqueId)
        {
            lock (sigLock)
            {
                string sContentFolder =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\";

                if (Directory.Exists(sContentFolder))
                {
                    return Directory.GetCreationTime(sContentFolder).ToString();
                }
                return DateTime.Now.ToString();
            }
        }

        internal static List<string> GetContentVersionList(string sContentUniqueId)
        {
            lock (sigLock)
            {
                string sContentVerFile =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentUniqueId + ContentVerFileExtension;

                if (File.Exists(sContentVerFile))
                {
                    string[] allVersionHashs = File.ReadAllLines(sContentVerFile);

                    if (allVersionHashs.Length > 0)
                    {
                        return new List<string>(allVersionHashs);
                    }
                }
                return new List<string>();
            }
        }

        internal static List<string> GetIconFileList()
        {
            lock (sigLock)
            {
                if (Directory.Exists(IconResPath))
                {
                    string[] allIconFiles = Directory.GetFiles(IconResPath);

                    if (allIconFiles.Length > 0)
                    {
                        List<string> listIconFiles = new List<string>();

                        foreach (string sIconFile in allIconFiles)
                        {
                            listIconFiles.Add(Path.GetFileName(sIconFile));
                        }
                        return listIconFiles;
                    }
                }
                return new List<string>();
            }
        }

        internal static List<string> GetDisclaimerFileList()
        {
            lock (sigLock)
            {
                if (Directory.Exists(DisclaimerResPath))
                {
                    string[] allDisclaimerFiles = Directory.GetFiles(DisclaimerResPath);

                    if (allDisclaimerFiles.Length > 0)
                    {
                        List<string> listDisclaimerFiles = new List<string>();

                        foreach (string sDisclaimerFile in allDisclaimerFiles)
                        {
                            listDisclaimerFiles.Add(Path.GetFileName(sDisclaimerFile));
                        }
                        return listDisclaimerFiles;
                    }
                }
                return new List<string>();
            }
        }

        private static void AppendContentVersionFile(string sContentUniqueId, string sContentHashCode)
        {
            lock (sigLock)
            {
                string sContentVerFile =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentUniqueId + ContentVerFileExtension;

                if (!File.Exists(sContentVerFile))
                {
                    throw new ApplicationException(
                        string.Format(AppResource.FileNotExist, sContentVerFile));
                }
                else
                {
                    string[] allVersions = File.ReadAllLines(sContentVerFile);
                    List<string> listAllVersions = new List<string>(allVersions);
                    if (!listAllVersions.Exists((x) => { return sContentHashCode == x; }))
                    {
                        File.AppendAllLines(
                            sContentVerFile,
                            new string[] { sContentHashCode });
                    }
                }
            }
        }

        private static void RemoveFromContentVersionFile(string sContentUniqueId, string sContentHashCode)
        {
            lock (sigLock)
            {
                string sContentVerFile =
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentUniqueId + ContentVerFileExtension;

                if (!File.Exists(sContentVerFile))
                {
                    throw new ApplicationException(
                        string.Format(AppResource.FileNotExist, sContentVerFile));
                }
                else
                {
                    string[] allVersions = File.ReadAllLines(sContentVerFile);
                    List<string> listAllVersions = new List<string>(allVersions);
                    if (!listAllVersions.Exists((x) => { return sContentHashCode == x; }))
                    {
                        listAllVersions.Remove(sContentHashCode);
                    }
                    File.WriteAllLines(sContentVerFile, listAllVersions.ToArray());
                }
            }
        }

        private static void Rollback(string sContentUniqueId, string sContentHashCode)
        {
            lock (sigLock)
            {
                if (sContentHashCode != "")
                {
                    string sContentVerFileGeneral =
                        ContentWorkingPath + "\\" +
                        sContentUniqueId + "\\" +
                        sContentHashCode;

                    // Defer for the release of the downloader from the other job
                    Thread.Sleep(500);
                    // Delete the content recipes files if any
                    if (File.Exists(sContentVerFileGeneral + ContentRecipesFileExtension))
                        File.Delete(sContentVerFileGeneral + ContentRecipesFileExtension);
                    // Delete the generated torrent files if any
                    if (File.Exists(sContentVerFileGeneral + TorrentExtension))
                        File.Delete(sContentVerFileGeneral + TorrentExtension);
                    if (File.Exists(sContentVerFileGeneral + TorrentVipExtension))
                        File.Delete(sContentVerFileGeneral + TorrentVipExtension);
                    if (File.Exists(sContentVerFileGeneral + TorrentFqdnExtension))
                        File.Delete(sContentVerFileGeneral + TorrentFqdnExtension);
                    // Delete the generated downloader if any
                    if (File.Exists(sContentVerFileGeneral + DownloaderExtension))
                        File.Delete(sContentVerFileGeneral + DownloaderExtension);
                    // Remove the certain version if any from the versioning file
                    RemoveFromContentVersionFile(sContentUniqueId, sContentHashCode);
                    // Defer for the release of the downloader from the current job
                    Thread.Sleep(500);
                }
            }
        }

        private void ProcessTorrentGen(string sContentUniqueId, ContentGenRecipes oRecipes, EventHandler<TorrentCreatorEventArgs> callbackHashed, out string sContentHashCode)
        {
            long lPieceLengthKB = AppConfig.ContentGenJob.PieceLengthKB;
            string sCreatedBy = AppConfig.ContentGenJob.CreatedBy;
            string sTrackerAnnounceUrl = AppConfig.ContentGenJob.TrackerAnnounceUrl;
            string sInternalTrackerAnnounceUrl = AppConfig.ContentGenJob.InternalTrackerAnnounceUrl;

            // Initialization
            _InterruptFlag = false;
            sContentHashCode = "";

            // Initialize the torrent creation task
            MetafileGenTask oMG = new MetafileGenTask();
            oMG.Hashed += callbackHashed;

            // Set the torrent info
            oMG.PieceLength = lPieceLengthKB * 1024; // Torrent info: PieceLength
            oMG.StoreMD5 = false; // Don't store MD5SUM in the torrent file
            oMG.Private = true; // Always be private torrent
            oMG.CreatedBy = sCreatedBy; // Torrent info: CreatedBy
            if (oRecipes.HttpSeedsUrl != null && oRecipes.HttpSeedsUrl.Length > 0)
            {
                List<string> listUrls = oRecipes.HttpSeedsUrlList;
                listUrls.ForEach(sUrl => { oMG.GetrightHttpSeeds.Add(sUrl); }); // URL seed
            }
            List<string> oAnn = new List<string>();
            oAnn.Add(sTrackerAnnounceUrl);
            oMG.Announces.Add(oAnn); // Torrent Info: Tracker Server
            // Assign the custom fields to the "info" section of the torrent
            oMG.AddCustomSecure("ga account name", new BEncodedString(oRecipes.GAProfileId));
            oMG.AddCustomSecure("ga host name", new BEncodedString("http://www.gamania.com"));
            oMG.AddCustomSecure("custom display name", new BEncodedString(oRecipes.DownloaderDisplayName));

            // Begin the async torrent creation process
            IAsyncResult asyncResult = oMG.BeginCreate(
                oRecipes.ContentSourceUrl, // Content Source URL
                null, // No callback needed
                oRecipes.ContentSourceUrl // Async state
            );
            // Wait for the completion or the aborting
            if (!asyncResult.IsCompleted)
            {
                while (!asyncResult.AsyncWaitHandle.WaitOne(1000))
                {
                    if (_InterruptFlag) oMG.AbortCreation(); // Try to abort the task
                }
            }
            // End of the async torre creation
            BEncodedDictionary oTorrentBenDict = oMG.EndCreate(asyncResult);

            // Get the content hash code from the torrent file
            sContentHashCode = Torrent.Load(oTorrentBenDict).InfoHash.ToHex();
            lock (sigLock)
            {
                // ========= Save 3 types of torrent files for 3 different usages ==============
                // 1. Save the torrent file which contains the public tracker announce URL
                File.WriteAllBytes(
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + TorrentExtension,
                    oTorrentBenDict.Encode());

                // 2. Save the VIP torrent file contains the public tracker announce URL
                if (oRecipes.VipHttpSeedsUrl != null && oRecipes.VipHttpSeedsUrl.Length > 0)
                {
                    // Add more Url Seed Urls
                    List<string> listUrls = oRecipes.VipHttpSeedsUrlList;
                    listUrls.ForEach(sUrl => { oMG.GetrightHttpSeeds.Add(sUrl); }); // Add more URL seed
                    // Convert to Ben-Encoded List
                    BEncodedList listUrlSeeds = new BEncodedList();
                    listUrlSeeds.AddRange(oMG.GetrightHttpSeeds.ConvertAll<BEncodedValue>(s => { return (BEncodedString)s; }));
                    oTorrentBenDict[TorrentInfoUrlListKey] = listUrlSeeds;
                }
                // Save the VIP Torrent file
                File.WriteAllBytes(
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + TorrentVipExtension,
                    oTorrentBenDict.Encode());
                // 3. Save the FQDN torrent file which contains the internal tracker announce URL
                // First, add the content source URL to the torrent to accelerate the download speed
                if (-1 == oRecipes.HttpSeedsUrlList.FindIndex(x => { return x.Equals(oRecipes.ContentSourceUrl); }) &&
                    -1 == oRecipes.VipHttpSeedsUrlList.FindIndex(x => { return x.Equals(oRecipes.ContentSourceUrl); }))
                {
                    // Add the content source URL to the TorrentInfoUrlListKey
                    // together with the VIP URL seeds just added previously
                    oMG.GetrightHttpSeeds.Add(oRecipes.ContentSourceUrl);
                    // Convert to Ben-Encoded List
                    BEncodedList listUrlSeeds = new BEncodedList();
                    listUrlSeeds.AddRange(oMG.GetrightHttpSeeds.ConvertAll<BEncodedValue>(s => { return (BEncodedString)s; }));
                    oTorrentBenDict[TorrentInfoUrlListKey] = listUrlSeeds;
                }
                // Second, replace the tracker announce URL with the internal tracker announce URL
                oTorrentBenDict[TorrentInfoAnnounceKey] = new BEncodedString(sInternalTrackerAnnounceUrl);
                // Save the FQDN Torrent file
                File.WriteAllBytes(
                    ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + TorrentFqdnExtension,
                    oTorrentBenDict.Encode());
            }
        }

        private void ProcessDownloaderGen(ContentGenRecipes oRecipes, string sContentUniqueId, string sContentHashCode)
        {
            string sNewGUID = Guid.NewGuid().ToString().ToUpper();
            string sWorkingFolder = ContentWorkingPath + "\\" + sContentUniqueId;

            // Initialization
            DownloaderGenTask oDG = new DownloaderGenTask(
                sWorkingFolder,
                sNewGUID,
                sWorkingFolder + "\\" + sContentHashCode + TorrentExtension,
                "", // No Banner Replacement Support
                oRecipes.IconFile == "" ? "" : IconResPath + "\\" + oRecipes.IconFile,
                oRecipes.PromotionEventID,
                sContentHashCode + DownloaderExtension,
                oRecipes.DownloaderDisplayName,
                oRecipes.DownloaderHomeUrl,
                oRecipes.DisclaimerFile == "" ? "" : DisclaimerResPath + "\\" + oRecipes.DisclaimerFile,
                oRecipes.OnlineFaqUrl,
                oRecipes.GAProfileId,
                oRecipes.PromotionEventServerUrl,
                oRecipes.UPXCompression);
            // Generate the downloader accordingly
            oDG.Generate();
        }

        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            string sContentHashCode = "";
            ContentGenRecipes oContentRecipes;
            JobDataMap oJobDataMap = context.JobDetail.JobDataMap;
            string sContentUniqueId = (string)context.MergedJobDataMap[GeneralJobDataMapConstants.ContentUniqueId];
            string sContentRecipesJson = (string)context.MergedJobDataMap[ContentGenJobDataMapConstants.ContentRecipesJson];

            #region Content (Metafiles / Torrents) Generating (support for interruption)
            try
            {
                //=============================================================================
                log.InfoFormat(AppResource.StartJobExecution, typeof(ContentGenJob).Name);
                //=============================================================================

                // Validate the settings & the input data map parameters
                Check.IsNullOrEmpty(sContentUniqueId, GeneralJobDataMapConstants.ContentUniqueId);
                Check.IsNullOrEmpty(sContentRecipesJson, ContentGenJobDataMapConstants.ContentRecipesJson);
                // Check the existence of the content versioning file
                CheckAndCreateContentVersionFile(sContentUniqueId);
                // Deserialize Content Recipes from the input Json
                oContentRecipes = ContentGenRecipes.FromJson(sContentRecipesJson);
                if (oContentRecipes.ContentHashCode == null ||
                    oContentRecipes.ContentHashCode == "")
                {
                    // Do the torrent generating process
                    oJobDataMap[ContentGenJobDataMapConstants.OverallCompletion] = "0%";
                    ProcessTorrentGen(
                        sContentUniqueId,
                        oContentRecipes,
                        (o, ev) =>
                        {
                            // Update the overall completion rate of the job
                            oJobDataMap[ContentGenJobDataMapConstants.OverallCompletion] =
                            ev.OverallCompletion.ToString("#0.##\\%");
                        },
                        out sContentHashCode);
                }
                else
                {
                    // Skip the torrent generating process
                    oJobDataMap[ContentGenJobDataMapConstants.OverallCompletion] = "100%";
                    sContentHashCode = oContentRecipes.ContentHashCode;
                }
                // Update the content hash code to the job data map
                oJobDataMap[GeneralJobDataMapConstants.ContentHashCode] = sContentHashCode;

                //=============================================================================
                log.InfoFormat(AppResource.TaskExecutionDone, AppResource.MetafileGenTaskName);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                Rollback(sContentUniqueId, sContentHashCode);

                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentGenJob).Name, oEx.Message);
                //===================================================================================================
                return;
            }
            #endregion

            #region Downloader Generating
            // The most brutal way to make sure of the job be executed synchronouly
            try
            {
                // Validate the settings & the input data map parameters
                Check.IsNullOrEmpty(sContentHashCode, GeneralJobDataMapConstants.ContentHashCode);
                lock (sigResUpdateLock) // To protect the global memory used in the ResourceLib
                {
                    // Generate the downloader accordingly
                    ProcessDownloaderGen(oContentRecipes, sContentUniqueId, sContentHashCode);
                }
                // Save the Content Recipes Json when downloader generating successfully
                Uri uri = new Uri(oContentRecipes.ContentSourceUrl);
                oContentRecipes.ContentFileName = Path.GetFileName(uri.LocalPath); // Get the file name
                oContentRecipes.ContentHashCode = sContentHashCode;
                oContentRecipes.CreateDateTime = DateTime.Now;
                SaveContentRecipesJson(
                    sContentUniqueId, sContentHashCode,
                    ContentGenRecipes.GetJson(oContentRecipes));
                // Update the history file when downloader generating successfully
                AppendHistoryFile(ContentRecipesPropHistory.DownloaderDisplayName, sContentUniqueId, oContentRecipes.DownloaderDisplayName);
                AppendHistoryFile(ContentRecipesPropHistory.DownloaderHomeUrl, sContentUniqueId, oContentRecipes.DownloaderHomeUrl);
                AppendHistoryFile(ContentRecipesPropHistory.GAProfileId, sContentUniqueId, oContentRecipes.GAProfileId);
                AppendHistoryFile(ContentRecipesPropHistory.OnlineFaqUrl, sContentUniqueId, oContentRecipes.OnlineFaqUrl);
                AppendHistoryFile(ContentRecipesPropHistory.PromoEventId, sContentUniqueId, oContentRecipes.PromotionEventID);
                AppendHistoryFile(ContentRecipesPropHistory.PromoEventServerUrl, sContentUniqueId, oContentRecipes.PromotionEventServerUrl);

                //=============================================================================
                log.InfoFormat(AppResource.TaskExecutionDone, AppResource.DownloaderGenTaskName);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                Rollback(sContentUniqueId, sContentHashCode);

                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentGenJob).Name, oEx.Message);
                //===================================================================================================
                return;
            }
            #endregion

            #region Content Deployment if specified
            // Do the content deployment if specified
            if (oContentRecipes.AutoDeploy)
            {
                try
                {
                    // Validate the settings & the input data map parameters
                    Check.IsNullOrEmpty(sContentHashCode, GeneralJobDataMapConstants.ContentHashCode);
                    // The most brutal way to make sure of the job be executed synchronouly
                    lock (sigResUpdateLock)
                    {
                        // Do the content deployment task
                        ContentDeployJob.ProcessContentDeploy(sContentUniqueId, sContentHashCode);
                    }

                    //=============================================================================
                    log.InfoFormat(AppResource.TaskExecutionDone, AppResource.MetafileDeployTaskName);
                    //=============================================================================
                }
                catch (Exception oEx)
                {
                    // Do nothing here so users can re-deploy again without re-generating all the files
                    // And keep on the next task for the aforementioned reason.

                    //===================================================================================================
                    log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentGenJob).Name, oEx.Message);
                    //===================================================================================================
                }
            }
            #endregion

            #region Update Content Version File
            try
            {
                // Update the content versioning file in the very end of the job
                // to maintain the consistence as possible
                AppendContentVersionFile(sContentUniqueId, sContentHashCode);

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(ContentGenJob).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                // Clear up all the generated files & the deployed files since it should be a sever error!
                if (oContentRecipes.AutoDeploy)
                {
                    ContentDeployJob.Rollback(sContentHashCode);
                }
                Rollback(sContentUniqueId, sContentHashCode);

                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentGenJob).Name, oEx.Message);
                //===================================================================================================
                return;
            }
            #endregion
        }

        #endregion

        #region IInterruptableJob Members

        public void Interrupt()
        {
            _InterruptFlag = true;
        }

        #endregion
    }

    // This is used to do the re-schedule of the content generating job
    // as a work-around of the unschedule-issue encountered when removing
    // the job from the Quartz scheduler
    public class ContentGenDummyJob : IJob, IInterruptableJob
    {
        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            return;
        }

        #endregion

        #region IInterruptableJob Members

        public void Interrupt()
        {
            return;
        }

        #endregion
    }
}
