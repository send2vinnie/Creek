using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Common.Logging;
using Creek.Config;
using Creek.Tasks;
using Creek.Utility;
using Quartz;

namespace Creek.Jobs
{
    public class ContentPauseJob : IJob
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentPauseJob));

        private static List<Tuple<string, Exception>> ProcessContentPause(string sContentUniqueId, string sContentHashCode)
        {
            string sIP = "";
            List<Tuple<string, Exception>> listFailedSeed = new List<Tuple<string, Exception>>();
            try
            {
                // Check the specified torrent in the offical seeds
                IManagementTask oCheckTask = (IManagementTask)new GetTorrentFileListTask(sContentHashCode);
                // Pause the specified torrent in the offical seeds
                IManagementTask oTask = (IManagementTask)new PauseTorrentTask(sContentHashCode);
                // Enumerate each seed web for sending the command
                AppConfig.ContentDeployJob.OfficalSeedWebList.ForEach(oSeedWeb =>
                {
                    try
                    {
                        sIP = oSeedWeb.IP;
                        QbtAdapter oAdapter = new QbtAdapter(
                            false,
                            sIP,
                            oSeedWeb.Port,
                            oSeedWeb.AdminName,
                            oSeedWeb.AdminPassword);
                        oAdapter.ExecuteTask(oCheckTask);
                        if (((ArrayList)oCheckTask.Result).Count > 0)
                        {
                            oAdapter.ExecuteTask(oTask);
                        }
                    }
                    catch (Exception oEx)
                    {
                        listFailedSeed.Add(new Tuple<string, Exception>(sIP, oEx));
                    }
                });
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
            return listFailedSeed;
        }

        #region IJob Members

        public void Execute(JobExecutionContext context)
        {
            string sContentUniqueId = (string)context.MergedJobDataMap[GeneralJobDataMapConstants.ContentUniqueId];
            string sContentHashCode = (string)context.MergedJobDataMap[GeneralJobDataMapConstants.ContentHashCode];

            try
            {
                //=============================================================================
                log.InfoFormat(AppResource.StartJobExecution, typeof(ContentPauseJob).Name);
                //=============================================================================

                // Validate the settings & the input data map parameters
                Check.IsNullOrEmpty(sContentUniqueId, GeneralJobDataMapConstants.ContentUniqueId);
                Check.IsNullOrEmpty(sContentHashCode, GeneralJobDataMapConstants.ContentHashCode);
                // Send the pause torrent command
                foreach (Tuple<string, Exception> failedSeed in ProcessContentPause(sContentUniqueId, sContentHashCode))
                {
                    //===================================================================================================
                    log.ErrorFormat(
                        AppResource.JobExecutionFailed,
                        failedSeed.Item2,
                        typeof(ContentPauseJob).Name,
                        string.Format(
                            AppResource.OfficalSeedCmdFailed,
                            failedSeed.Item1,
                            typeof(PauseTorrentTask).Name));
                    //===================================================================================================
                }

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(ContentPauseJob).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentPauseJob).Name, oEx.Message);
                //===================================================================================================
            }
        }

        #endregion
    }
}
