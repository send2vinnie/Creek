using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Common.Logging;
using Creek.Config;
using Creek.Tasks;
using Creek.Utility;
using Creek.Controller;
using Quartz;
using Newtonsoft.Json;

namespace Creek.Jobs
{
    public class ContentMontiorFactory : IJob
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentMontiorFactory));

        static ContentMontiorFactory()
        {
            lock (ContentGenJob.sigLock)
            {
                // Assign the culture information of the resource manager
                AppResource.Culture = Thread.CurrentThread.CurrentCulture;
                // Check if the working directory exist or create it
                if (!Directory.Exists(ContentGenJob.ContentWorkingPath))
                    Directory.CreateDirectory(ContentGenJob.ContentWorkingPath);
            }
        }

        private void ExportRemotingController(CreekController oController)
        {
            RemotingControllerExporter oExporter = new RemotingControllerExporter();
            oExporter.Bind(oController);
        }

        private void StopRemotingController(CreekController oController)
        {
            RemotingControllerExporter oExporter = new RemotingControllerExporter();
            oExporter.UnBind(oController);
        }

        private void ProcessSeedMonitorForking(IScheduler oSch)
        {
            // Get all the registered seed IP from the app configuration file
            List<AppConfig.ContentDeployJob.OfficalSeedWeb> listSeedIp = AppConfig.ContentDeployJob.OfficalSeedWebList;

            // Enumerate all the seed web and schedule the new trigger / job pair accordingly
            AppConfig.ContentDeployJob.OfficalSeedWebList.ForEach(oSeedWeb =>
            {
                // Create and initialize the content monitor job & the trigger
                JobDetail oJob = new JobDetail(
                    oSeedWeb.IP,
                    SeedMonitorConstants.SeedMonitorGroupName,
                    typeof(SeedMonitorJob));
                SimpleTrigger oTrigger = new SimpleTrigger(
                    oSeedWeb.IP,
                    SeedMonitorConstants.SeedMonitorGroupName,
                    -1, TimeSpan.FromSeconds(SeedMonitorConstants.SeedMonitorPeriodSec));

                // Add the seed web to the job data map
                // There is no concurrency issue here since the data map entry won't be
                // updated when the job is running
                oJob.JobDataMap[SeedMonitorJobDataMapConstants.OfficalSeedWeb] = oSeedWeb;
                oJob.Durable = false;
                oTrigger.Priority = 1; // Low priority
                oTrigger.JobName = oSeedWeb.IP;
                oTrigger.JobGroup = SeedMonitorConstants.SeedMonitorGroupName;
                oSch.ScheduleJob(oJob, oTrigger);
                // ************************************************************************************
                log.DebugFormat("Start to monitor the seed: {0}", oSeedWeb.IP);
                // ************************************************************************************
            });
        }

        private void ProcessContentMonitorForking(IScheduler oSch)
        {
            // Get all the content unique ID by retrieving the subdirectory list
            string[] listContent = Directory.GetDirectories(
                ContentGenJob.ContentWorkingPath, "*", SearchOption.TopDirectoryOnly);
            // Get the list of all the running content monitor triggers
            IList<string> listTemp =
                oSch.TriggerGroupNames.Contains(ContentMonitorConstants.ContentMonitorGroupName) ?
                oSch.GetTriggerNames(ContentMonitorConstants.ContentMonitorGroupName) : null;
            // The list retrieved above is fixed length so we have to create a new list
            List<string> listTriggers = listTemp != null ? new List<string>(listTemp) : null;

            // Enumerate all the content and schedule the new trigger / job pair accordingly
            foreach (string sPath in listContent)
            {
                string sContentId = sPath.Substring(sPath.LastIndexOf(Path.DirectorySeparatorChar) + 1);

                if (listTriggers == null || !listTriggers.Contains(sContentId))
                {
                    // Create and initialize the content monitor job & the trigger
                    JobDetail oJob = new JobDetail(
                        sContentId,
                        ContentMonitorConstants.ContentMonitorGroupName,
                        typeof(ContentMonitorJob));
                    SimpleTrigger oTrigger = new SimpleTrigger(
                        sContentId,
                        ContentMonitorConstants.ContentMonitorGroupName,
                        -1, TimeSpan.FromSeconds(ContentMonitorConstants.ContentMonitorPeriodSec));

                    oJob.Durable = false;
                    oTrigger.Priority = 1; // Low priority
                    oTrigger.JobName = sContentId;
                    oTrigger.JobGroup = ContentMonitorConstants.ContentMonitorGroupName;
                    oSch.ScheduleJob(oJob, oTrigger);
                    // ************************************************************************************
                    log.DebugFormat("Start to monitor the content: {0}", sContentId);
                    // ************************************************************************************
                }
                else
                {
                    // Do nothing to the already monitored contents
                    listTriggers.Remove(sContentId);
                }
            }
            // Check the remaining triggers which have no corresponding directory found
            // in the working directory to unschedule these triggers accordingly
            if (listTriggers != null)
            {
                foreach (string sIdleTriggerName in listTriggers)
                {
                    JobDetail oJob = oSch.GetJobDetail(sIdleTriggerName, ContentMonitorConstants.ContentMonitorGroupName);
                    Trigger oTrigger = oSch.GetTrigger(sIdleTriggerName, ContentMonitorConstants.ContentMonitorGroupName);

                    oJob.JobType = typeof(ContentMonitorDummyJob); // Change the job type to the dummy one
                    if (oTrigger is SimpleTrigger)
                    {
                        ((SimpleTrigger)oTrigger).RepeatCount = 0;
                        ((SimpleTrigger)oTrigger).RepeatInterval = TimeSpan.FromSeconds(1);
                    }
                    oTrigger.StartTimeUtc = DateTime.UtcNow.AddHours(-1);
                    oTrigger.EndTimeUtc = DateTime.UtcNow;
                    oSch.UnscheduleJob(sIdleTriggerName, ContentMonitorConstants.ContentMonitorGroupName);
                    // !!! Re-schedule the job to prevent the hard to solved internal Quartz exception !!!
                    oSch.ScheduleJob(oJob, oTrigger);
                    // ************************************************************************************
                    log.DebugFormat("Stop monitoring the content: {0}", sIdleTriggerName);
                    // ************************************************************************************
                }
            }
        }

        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            try
            {
                //=============================================================================
                log.InfoFormat(AppResource.StartJobExecution, typeof(ContentMontiorFactory).Name);
                //=============================================================================

                // Creek Controller for remote context
                CreekController oController = new CreekController(context.Scheduler);
                // Run the seed monitor jobs & triggers per the registered seeds
                ProcessSeedMonitorForking(context.Scheduler);
                // Run the cotent monitor jobs & triggers per the existing contents
                ProcessContentMonitorForking(context.Scheduler);
                // Export the controller to the remoting context
                ExportRemotingController(oController);

                // Initialize the file system watcher for monitoring the activities of the working directory
                FileSystemWatcher oFSW = new FileSystemWatcher(ContentGenJob.ContentWorkingPath);
                oFSW.EnableRaisingEvents = false;
                oFSW.IncludeSubdirectories = false;
                oFSW.NotifyFilter = NotifyFilters.DirectoryName;
                oFSW.Created += (o, e) => { ProcessContentMonitorForking(context.Scheduler); };
                oFSW.Deleted += (o, e) => { ProcessContentMonitorForking(context.Scheduler); };
                oFSW.EnableRaisingEvents = true;

                // Wait until the scheduler is shutdown
                int nGCTimeout = 0;
                AutoResetEvent[] oEventNeverSet = new AutoResetEvent[1] { new AutoResetEvent(false) };
                while (!context.Scheduler.InStandbyMode)
                {
                    WaitHandle.WaitAny(oEventNeverSet, 3000);
                    nGCTimeout += 3000;
                    if (nGCTimeout >= 5 * 60 * 1000) // GC for every 5 mins
                    {
                        nGCTimeout = 0; // Reset the timeout
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized);
                    }
                }

                // Disconnect the controller to the remoting context
                StopRemotingController(oController);

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(ContentMontiorFactory).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentMontiorFactory).Name, oEx.Message);
                //===================================================================================================
            }
        }

        #endregion
    }
}
