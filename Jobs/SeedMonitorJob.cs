using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Common.Logging;
using Creek.Config;
using Creek.Tasks;
using Creek.Utility;
using Quartz;
using Creek.JsonObject;
using Newtonsoft.Json;

namespace Creek.Jobs
{
    public class SeedMonitorJob : IStatefulJob
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(SeedMonitorJob));

        private static string ProcessSeedMonitor(AppConfig.ContentDeployJob.OfficalSeedWeb oSeedWeb)
        {
            try
            {
                // Retrieve all the torrent details in the offical seed
                IManagementTask oTask = (IManagementTask)new RetrieveTorrentDetailsTask();

                QbtAdapter oAdapter = new QbtAdapter(
                    false,
                    oSeedWeb.IP,
                    oSeedWeb.Port,
                    oSeedWeb.AdminName,
                    oSeedWeb.AdminPassword);
                oAdapter.ExecuteTask(oTask);
                // Serialize the torrent details to JSON and return
                return JsonConvert.SerializeObject(oTask.Result);
            }
            catch (Exception oEx)
            {
                // Generate the exception in more detail
                throw new ApplicationException(string.Format(AppResource.OfficalSeedCmdFailed, oSeedWeb.IP, typeof(RetrieveTorrentDetailsTask).Name), oEx);
            }
        }

        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            AppConfig.ContentDeployJob.OfficalSeedWeb oSeedWeb =
                (AppConfig.ContentDeployJob.OfficalSeedWeb)context.MergedJobDataMap[SeedMonitorJobDataMapConstants.OfficalSeedWeb];

            try
            {
                //=============================================================================
                log.InfoFormat(AppResource.StartJobExecution, typeof(SeedMonitorJob).Name);
                //=============================================================================

                // Validate the input data map parameters
                Check.IsNull((object)oSeedWeb, SeedMonitorJobDataMapConstants.OfficalSeedWeb);
                // Do the monitoring
                string sJson = ProcessSeedMonitor(oSeedWeb);
                // Add returned torrent details (JSON) into the job data map to avoid the concurrency issue
                context.JobDetail.JobDataMap[SeedMonitorJobDataMapConstants.TorrentDetails] = sJson;

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(SeedMonitorJob).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                // Remove the entry from the job data map to refresh the torrent details
                context.JobDetail.JobDataMap.Remove(SeedMonitorJobDataMapConstants.TorrentDetails);

                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(SeedMonitorJob).Name, oEx.Message);
                //===================================================================================================
            }
        }

        #endregion
    }
}
