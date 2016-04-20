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
    public class ContentOfflineJob : IJob
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentOfflineJob));

        private static List<Tuple<string, Exception>> ProcessContentOffline(string sContentUniqueId, string sContentHashCode)
        {
            // Send the torrent remove command to the offical seeds first
            // then deleted the reployed torrent files afterward to make
            // the availability of the content files more consistent.

            string sIP = "";
            List<Tuple<string, Exception>> listFailedSeed = new List<Tuple<string, Exception>>();
            try
            {
                // Remove the deployed torrent files from the offical seeds
                IManagementTask oTask = (IManagementTask)new RemoveTorrentTask(sContentHashCode);
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
                        oAdapter.ExecuteTask(oTask);
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

            try
            {
                // Delete the deployed torrent files if any
                if (File.Exists(
                    AppConfig.ContentDeployJob.TorrentDeployTarget + "\\" +
                    sContentHashCode + ContentGenJob.TorrentExtension))
                {
                    File.Delete(
                        AppConfig.ContentDeployJob.TorrentDeployTarget + "\\" +
                        sContentHashCode + ContentGenJob.TorrentExtension);
                }
                if (File.Exists(
                    AppConfig.ContentDeployJob.VipTorrentDeployTarget + "\\" +
                    sContentHashCode + ContentGenJob.TorrentVipExtension))
                {
                    File.Delete(
                        AppConfig.ContentDeployJob.VipTorrentDeployTarget + "\\" +
                        sContentHashCode + ContentGenJob.TorrentVipExtension);
                }
                if (File.Exists(
                    AppConfig.ContentDeployJob.FqdnTorrentDeployTarget + "\\" +
                    sContentHashCode + ContentGenJob.TorrentFqdnExtension))
                {
                    File.Delete(
                        AppConfig.ContentDeployJob.FqdnTorrentDeployTarget + "\\" +
                        sContentHashCode + ContentGenJob.TorrentFqdnExtension);
                }
                // Delete the deployed downloader (for portal users download) if any
                if (File.Exists(
                    AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + ContentGenJob.DownloaderExtension))
                {
                    File.Delete(
                        AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                        sContentUniqueId + "\\" +
                        sContentHashCode + ContentGenJob.DownloaderExtension);
                }
                // Delete the deployed torrent file (for portal users download) if any
                if (File.Exists(
                    AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + ContentGenJob.TorrentExtension))
                {
                    File.Delete(
                        AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                        sContentUniqueId + "\\" +
                        sContentHashCode + ContentGenJob.TorrentExtension);
                }
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
                log.InfoFormat(AppResource.StartJobExecution, typeof(ContentOfflineJob).Name);
                //=============================================================================

                // Validate the settings & the input data map parameters
                Check.IsNullOrEmpty(sContentUniqueId, GeneralJobDataMapConstants.ContentUniqueId);
                Check.IsNullOrEmpty(sContentHashCode, GeneralJobDataMapConstants.ContentHashCode);
                // Do the deployment
                foreach (Tuple<string, Exception> failedSeed in ProcessContentOffline(sContentUniqueId, sContentHashCode))
                {
                    //===================================================================================================
                    log.ErrorFormat(
                        AppResource.JobExecutionFailed,
                        failedSeed.Item2,
                        typeof(ContentOfflineJob).Name,
                        string.Format(
                            AppResource.OfficalSeedCmdFailed,
                            failedSeed.Item1,
                            typeof(RemoveTorrentTask).Name));
                    //===================================================================================================
                }

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(ContentOfflineJob).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentOfflineJob).Name, oEx.Message);
                //===================================================================================================
            }
        }

        #endregion
    }
}
