using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using Quartz;
using Creek.Config;
using Creek.Jobs;
using Creek.JsonObject;
using Creek.Utility;
using NLog.Config;
using NLog.Targets;
using Newtonsoft.Json;

namespace Creek.Controller
{
    public class CreekController : MarshalByRefObject, IRemotableCreekController
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(CreekController));
        private static List<Tuple<DateTime, string, string>> logRecords = new List<Tuple<DateTime, string, string>>();
        private IScheduler quartzScheduler;

        static CreekController()
        {
            MethodCallTarget target = new MethodCallTarget();
            // Log only the message from the same assembly of the class belongs to!!
            LoggingRule rule = new LoggingRule(
                typeof(CreekController).Assembly.GetName().Name + ".*",
                NLog.LogLevel.Error,
                target);

            // Initialize and add the MethodCallTarget to the NLog engine
            target.ClassName = typeof(CreekController).AssemblyQualifiedName;
            target.MethodName = "LogError";
            target.Parameters.Add(new MethodCallParameter("level", typeof(string), "${level}"));
            target.Parameters.Add(new MethodCallParameter("message", typeof(string), "${message}"));
            if (File.Exists("NLog.config"))
            {
                // Read the settings from NLog.config and add the new target & rule
                XmlLoggingConfiguration oConfig = new XmlLoggingConfiguration("NLog.config");
                oConfig.AddTarget("MemoryTarget", target);
                oConfig.LoggingRules.Add(rule);
                NLog.LogManager.Configuration = oConfig;
            }
            else
            {
                throw new ApplicationException(AppResource.NLogConfigNotFound);
            }
        }

        public CreekController(IScheduler scheduler)
        {
            Check.IsNull(scheduler, "scheduler");
            quartzScheduler = scheduler;
        }

        public static void LogError(string level, string message)
        {
            Tuple<DateTime, string, string> tLog =
                new Tuple<DateTime, string, string>(DateTime.UtcNow, level, message);

            lock (logRecords)
            {
                // Insert the latest log to the begining of the list
                logRecords.Insert(0, tLog);
                // Remove the overflow log from the end of the list
                if (logRecords.Count > 1000)
                {
                    logRecords.RemoveAt(logRecords.Count - 1);
                }
            }
        }

        private void TriggerJobOfContentVersion<T>(string sJobName, string sGroupName, string sContentUniqueId, string sContentHashCode)
        {
            lock (quartzScheduler)
            {
                // Create a job of type T if none has existed
                if (null == quartzScheduler.GetJobDetail(sJobName, sGroupName))
                {
                    // Create a durable job of type T and add to the scheduler 
                    JobDetail oJob = new JobDetail(sJobName, sGroupName, typeof(T), false, true, false);
                    quartzScheduler.AddJob(oJob, true);
                }
                // Create and initialize the trigger for the specific job request
                JobDataMap oJobDataMap = new JobDataMap();
                oJobDataMap[GeneralJobDataMapConstants.ContentUniqueId] = sContentUniqueId;
                oJobDataMap[GeneralJobDataMapConstants.ContentHashCode] = sContentHashCode;
                quartzScheduler.TriggerJobWithVolatileTrigger(sJobName, sGroupName, oJobDataMap);
            }
        }

        ///<summary>
        ///Obtains a lifetime service object to control the lifetime policy for this instance.
        ///</summary>
        public override object InitializeLifetimeService()
        {
            // overriden to initialize null life time service,
            // this basically means that remoting object will live as long
            // as the application lives
            return null;
        }

        #region IRemotableCreekController Members

        public Tuple<string, bool, string[]>[] GetMonitoringSeedList()
        {
            List<Tuple<string, bool, string[]>> listSeeds = new List<Tuple<string, bool, string[]>>();

            // Enumerate all the seed web and retrieve the seed status accordingly
            AppConfig.ContentDeployJob.OfficalSeedWebList.ForEach(oSeedWeb =>
            {
                JobDetail oJob = quartzScheduler.GetJobDetail(oSeedWeb.IP, SeedMonitorConstants.SeedMonitorGroupName);

                if (oJob != null)
                {
                    string sTorrentDetails = (string)oJob.JobDataMap[SeedMonitorJobDataMapConstants.TorrentDetails];
                    if (sTorrentDetails != null)
                    {
                        List<TorrentDetail> listTorrentDetail = JsonConvert.DeserializeObject<List<TorrentDetail>>(sTorrentDetails);
                        List<string> listTorrentName = new List<string>();
                        listTorrentDetail.Sort((x, y) => { return x.Name.CompareTo(y.Name); });
                        listTorrentDetail.ForEach(x => { listTorrentName.Add(x.Name); });
                        listSeeds.Add(new Tuple<string, bool, string[]>(oSeedWeb.IP + string.Format(" ({0})", listTorrentName.Count), true, listTorrentName.ToArray()));
                    }
                    else
                    {
                        listSeeds.Add(new Tuple<string, bool, string[]>(oSeedWeb.IP, false, null));
                    }
                }
                else
                {
                    listSeeds.Add(new Tuple<string, bool, string[]>(oSeedWeb.IP, false, null));
                }
            });
            return listSeeds.ToArray();
        }

        public Tuple<string, bool>[] GetMonitoringContentList()
        {
            List<Tuple<string, bool>> listContents = new List<Tuple<string, bool>>();
            IList<string> listContentId = quartzScheduler.GetJobNames(ContentMonitorConstants.ContentMonitorGroupName);

            // Enumerate all the contents and retrieve the status accordingly
            foreach (string sContentId in listContentId)
            {
                JobDetail oJob = quartzScheduler.GetJobDetail(sContentId, ContentMonitorConstants.ContentMonitorGroupName);

                if (oJob != null)
                {
                    string sContentDetails = (string)oJob.JobDataMap[ContentMonitorJobDataMapConstants.ContentDetail];
                    if (sContentDetails != null)
                    {
                        listContents.Add(new Tuple<string, bool>(sContentId, true));
                    }
                    else
                    {
                        listContents.Add(new Tuple<string, bool>(sContentId, false));
                    }
                }
                else
                {
                    listContents.Add(new Tuple<string, bool>(sContentId, false));
                }
            };
            // Sort Content List by the name
            listContents.Sort((x, y) => { return x.Item1.CompareTo(y.Item1); });
            return listContents.ToArray();
        }

        public Tuple<string, string>[] GetContentSummary(string sContentUniqueId)
        {
            JobDetail oJob = quartzScheduler.GetJobDetail(sContentUniqueId, ContentMonitorConstants.ContentMonitorGroupName);
            List<Tuple<string, string>> listSummary = new List<Tuple<string, string>>();

            if (oJob != null)
            {
                try
                {
                    string sContentDetailJson = (string)oJob.JobDataMap[ContentMonitorJobDataMapConstants.ContentDetail];
                    if (sContentDetailJson != null)
                    {
                        ContentDetail oContent = JsonConvert.DeserializeObject<ContentDetail>(sContentDetailJson);
                        listSummary.Add(new Tuple<string, string>("Name", oContent.Name));
                        listSummary.Add(new Tuple<string, string>("DateCreated", oContent.DateCreated.ToString()));
                        listSummary.Add(new Tuple<string, string>("Count", oContent.Versions.Count.ToString()));
                    }
                }
                catch (Exception oEx)
                {
                    throw new ApplicationException(string.Format(AppResource.ContentDetailReadFailed, oEx.Message), oEx);
                }
            }
            return listSummary.ToArray();
        }

        public Tuple<DateTime, string>[] GetWorkInProgressList(string sContentUniqueId)
        {
            List<Tuple<DateTime, string>> listWorkInProgress = new List<Tuple<DateTime, string>>();
            IList<JobExecutionContext> listJobs = quartzScheduler.GetCurrentlyExecutingJobs();

            foreach (JobExecutionContext oJC in listJobs.Where((x, y) =>
            {
                return x.JobDetail.Name == sContentUniqueId &&
                       x.JobDetail.Group == ContentGenConstants.ContentGenGroupName;
            }))
            {
                listWorkInProgress.Add(new Tuple<DateTime, string>
                (
                    oJC.FireTimeUtc.Value,
                    oJC.JobDetail.JobDataMap[ContentGenJobDataMapConstants.OverallCompletion].ToString()
                ));
            }
            return listWorkInProgress.ToArray();
        }

        public Tuple<DateTime, string, string, bool, float, float>[] GetContentVersionList(string sContentUniqueId)
        {
            JobDetail oJob = quartzScheduler.GetJobDetail(sContentUniqueId, ContentMonitorConstants.ContentMonitorGroupName);
            List<Tuple<DateTime, string, string, bool, float, float>> listVersion = new List<Tuple<DateTime, string, string, bool, float, float>>();

            if (oJob != null)
            {
                try
                {
                    string sContentDetailJson = (string)oJob.JobDataMap[ContentMonitorJobDataMapConstants.ContentDetail];
                    if (sContentDetailJson != null)
                    {
                        ContentDetail oContent = JsonConvert.DeserializeObject<ContentDetail>(sContentDetailJson);

                        foreach (VersionDetail oVer in oContent.Versions)
                        {
                            int nUnkownCount = 0;
                            int nSeedingCount = 0;
                            int nSeedCount = oVer.TorrentSeeds.Count;

                            foreach (TorrentSeedDetail oTSD in oVer.TorrentSeeds)
                            {
                                if (oTSD.StatusCode == TorrentStatus.Unknown) nUnkownCount++;
                                if (oTSD.StatusCode == TorrentStatus.Seeding) nSeedingCount++;
                            }
                            oVer.SeedDeploymentRatio = ((float)(nSeedCount - nUnkownCount) / (float)nSeedCount);
                            oVer.SeedingRatio = ((float)nSeedingCount / (float)nSeedCount);
                            listVersion.Add(
                                new Tuple<DateTime, string, string, bool, float, float>(
                                oVer.DateCreated,
                                oVer.Name,
                                oVer.Hash,
                                oVer.DeployToTracker,
                                (float)Math.Round(oVer.SeedDeploymentRatio, 3),
                                (float)Math.Round(oVer.SeedingRatio, 3)));
                        }
                    }
                }
                catch (Exception oEx)
                {
                    throw new ApplicationException(string.Format(AppResource.ContentDetailReadFailed, oEx.Message), oEx);
                }
            }
            // Sort VersionDetails by DateCreated
            listVersion.Sort((x, y) => { return x.Item1.CompareTo(y.Item1); });
            return listVersion.ToArray();
        }

        public Tuple<string, long, float, string, string>[] GetTorrentSeedDetail(string sContentUniqueId, string sContentHashCode)
        {
            JobDetail oJob = quartzScheduler.GetJobDetail(sContentUniqueId, ContentMonitorConstants.ContentMonitorGroupName);
            List<Tuple<string, long, float, string, string>> listTorrentSeed = new List<Tuple<string, long, float, string, string>>();

            if (oJob != null)
            {
                try
                {
                    string sContentDetailJson = (string)oJob.JobDataMap[ContentMonitorJobDataMapConstants.ContentDetail];
                    if (sContentDetailJson != null)
                    {
                        ContentDetail oContent = JsonConvert.DeserializeObject<ContentDetail>(sContentDetailJson);

                        if (oContent.Versions.Count > 0)
                        {
                            VersionDetail oVer = oContent.Versions.Find(x => { return x.Hash == sContentHashCode; });

                            if (oVer != null)
                            {
                                foreach (TorrentSeedDetail oTSD in oVer.TorrentSeeds)
                                {
                                    listTorrentSeed.Add(
                                        new Tuple<string, long, float, string, string>(
                                        oTSD.IP,
                                        oTSD.TotalSize,
                                        (float)Math.Round(oTSD.PartDone, 3),
                                        oTSD.StatusCode.ToString(),
                                        oTSD.Error));
                                }
                            }
                        }
                    }
                }
                catch (Exception oEx)
                {
                    throw new ApplicationException(string.Format(AppResource.ContentDetailReadFailed, oEx.Message), oEx);
                }
            }
            return listTorrentSeed.ToArray();
        }

        public string GetContentGenRecipesJson(string sContentUniqueId, string sContentHashCode)
        {
            JobDetail oJob = quartzScheduler.GetJobDetail(sContentUniqueId, ContentMonitorConstants.ContentMonitorGroupName);

            if (oJob != null)
            {
                try
                {
                    return ContentGenJob.ReadContentRecipesJson(sContentUniqueId, sContentHashCode);
                }
                catch (Exception oEx)
                {
                    throw new ApplicationException(string.Format(AppResource.ContentRecipesReadFailed, oEx.Message), oEx);
                }
            }
            return "";
        }

        public string[] GetDownloaderDisplayNameHistory(string sContentUniqueId)
        {
            return ContentGenJob.GetHistoryList(
                ContentRecipesPropHistory.DownloaderDisplayName,
                sContentUniqueId,
                sContentUniqueId).ToArray();
        }

        public string[] GetDownloaderHomeUrlHistory(string sContentUniqueId)
        {
            return ContentGenJob.GetHistoryList(
                ContentRecipesPropHistory.DownloaderHomeUrl,
                sContentUniqueId,
                "http://").ToArray();
        }

        public string[] GetDownloaderOnlineFaqUrlHistory(string sContentUniqueId)
        {
            return ContentGenJob.GetHistoryList(
                ContentRecipesPropHistory.OnlineFaqUrl,
                sContentUniqueId,
                AppConfig.ContentGenJob.OnlineFaqUrlDefaultValue).ToArray();
        }

        public string[] GetDownloaderGAProfileIdHistory(string sContentUniqueId)
        {
            return ContentGenJob.GetHistoryList(
                ContentRecipesPropHistory.GAProfileId,
                sContentUniqueId,
                AppConfig.ContentGenJob.GAProfileIdDefaultValue).ToArray();
        }

        public string[] GetDownloaderPromoEventIdHistory(string sContentUniqueId)
        {
            return ContentGenJob.GetHistoryList(
                ContentRecipesPropHistory.PromoEventId,
                sContentUniqueId,
                AppConfig.ContentGenJob.PromoEventIdDefaultValue).ToArray();
        }

        public string[] GetDownloaderPromoEventServerUrlHistory(string sContentUniqueId)
        {
            return ContentGenJob.GetHistoryList(
                ContentRecipesPropHistory.PromoEventServerUrl,
                sContentUniqueId,
                AppConfig.ContentGenJob.PromoEventServerUrlDefaultValue).ToArray();
        }

        public string GetDownloaderDefaultValues()
        {
            return "<root>" +
                "<OnlineFaqUrl>" + AppConfig.ContentGenJob.OnlineFaqUrlDefaultValue + "</OnlineFaqUrl>" +
                "<GAProfileId>" + AppConfig.ContentGenJob.GAProfileIdDefaultValue + "</GAProfileId>" +
                "<PromoEventId>" + AppConfig.ContentGenJob.PromoEventIdDefaultValue + "</PromoEventId>" +
                "<PromoEventServerUrl>" + AppConfig.ContentGenJob.PromoEventServerUrlDefaultValue + "</PromoEventServerUrl>" +
                "</root>";
        }

        public string[] GetIconFileList()
        {
            return ContentGenJob.GetIconFileList().ToArray();
        }

        public string[] GetDisclaimerFileList()
        {
            return ContentGenJob.GetDisclaimerFileList().ToArray();
        }

        public void GenerateContentReq(string sContentUniqueId, string sContentGenRecipesJson)
        {
            lock (quartzScheduler)
            {
                // Create a ContentGenJob per Content if none has existed
                if (null == quartzScheduler.GetJobDetail(
                    sContentUniqueId,
                    ContentGenConstants.ContentGenGroupName))
                {
                    // Create a durable ContentGenJob and add to the scheduler 
                    JobDetail oJob = new JobDetail(
                        sContentUniqueId,
                        ContentGenConstants.ContentGenGroupName,
                        typeof(ContentGenJob), false, true, false);
                    oJob.JobDataMap[ContentGenJobDataMapConstants.OverallCompletion] = "0%";
                    quartzScheduler.AddJob(oJob, true);
                }
                // Create and initialize the trigger for the specific job request
                string sTriggerName = sContentUniqueId + " (" +
                    quartzScheduler.GetTriggersOfJob(
                        sContentUniqueId,
                        ContentGenConstants.ContentGenGroupName).Count + ")";
                Trigger oTrigger = new SimpleTrigger(
                    sTriggerName,
                    ContentGenConstants.ContentGenGroupName,
                    // Compute a time that is on the next round minute
                    TriggerUtils.GetNextGivenSecondDate(DateTime.UtcNow, 5));
                // ####################################################################
                oTrigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.FireNow;
                // ####################################################################
                oTrigger.JobName = sContentUniqueId;
                oTrigger.JobGroup = ContentGenConstants.ContentGenGroupName;
                ContentGenRecipes oJson = ContentGenRecipes.FromJson(sContentGenRecipesJson);
                oTrigger.JobDataMap[GeneralJobDataMapConstants.ContentUniqueId] = sContentUniqueId;
                oTrigger.JobDataMap[ContentGenJobDataMapConstants.ContentRecipesJson] = ContentGenRecipes.GetJson(oJson);
                quartzScheduler.ScheduleJob(oTrigger);
            }
        }

        public void InterruptContentGenReq(string sContentUniqueId)
        {
            // Interrupt all the Content related running jobs
            quartzScheduler.Interrupt(sContentUniqueId, ContentGenConstants.ContentGenGroupName);
        }

        public void DeployContentVersionReq(string sContentUniqueId, string sContentHashCode)
        {
            TriggerJobOfContentVersion<ContentDeployJob>(
                ContentDeployConstants.ContentDeployJobName,
                ContentDeployConstants.ContentDeployGroupName,
                sContentUniqueId,
                sContentHashCode);
        }

        public void OfflineContentVersionReq(string sContentUniqueId, string sContentHashCode)
        {
            TriggerJobOfContentVersion<ContentOfflineJob>(
                ContentOfflineConstants.ContentOfflineJobName,
                ContentOfflineConstants.ContentOfflineGroupName,
                sContentUniqueId,
                sContentHashCode);
        }

        public void PauseContentVersionReq(string sContentUniqueId, string sContentHashCode)
        {
            TriggerJobOfContentVersion<ContentPauseJob>(
                ContentPauseConstants.ContentPauseJobName,
                ContentPauseConstants.ContentPauseGroupName,
                sContentUniqueId,
                sContentHashCode);
        }

        public void ResumeContentVersionReq(string sContentUniqueId, string sContentHashCode)
        {
            TriggerJobOfContentVersion<ContentResumeJob>(
                ContentResumeConstants.ContentResumeJobName,
                ContentResumeConstants.ContentResumeGroupName,
                sContentUniqueId,
                sContentHashCode);
        }

        public string GetContentMonitoringJson(string sContentUniqueId)
        {
            JobDetail oJob = quartzScheduler.GetJobDetail(sContentUniqueId, ContentMonitorConstants.ContentMonitorGroupName);

            if (oJob != null)
            {
                try
                {
                    string sContentDetailJson = (string)oJob.JobDataMap[ContentMonitorJobDataMapConstants.ContentDetail];
                    return sContentDetailJson == null ? "" : sContentDetailJson;
                }
                catch (Exception oEx)
                {
                    throw new ApplicationException(string.Format(AppResource.ContentDetailReadFailed, oEx.Message), oEx);
                }
            }
            return "";
        }

        public Tuple<DateTime, string, string>[] GetExecLog(int startIndex, int count, out int recordLast)
        {
            Check.IsPositive(startIndex, "startIndex");
            Check.IsPositive(count, "count");

            lock (logRecords)
            {
                if (startIndex > (logRecords.Count - 1))
                {
                    // Return nothing if the specified range is out of index
                    recordLast = 0;
                    return new Tuple<DateTime, string, string>[0];
                }
                else
                {
                    // Return the logs of the specified range
                    recordLast = logRecords.Count - (startIndex + count);
                    if (recordLast < 0)
                    {
                        count += recordLast;
                        recordLast = 0;
                    }
                    return logRecords.GetRange(startIndex, count).ToArray();
                }
            }
        }

        #endregion
    }
}
