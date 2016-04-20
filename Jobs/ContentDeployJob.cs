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
    public class ContentDeployJob : IJob
    {
        // Logging
        private static readonly ILog log = LogManager.GetLogger(typeof(ContentDeployJob));

        private static void CopyFile(string sFilePublishDirectory, string sContentUniqueId, string sContentHashCode, string sFileExt, bool bContentSpecificSubdir = false)
        {
            lock (ContentGenJob.sigLock)
            {

                string sDestinationFile = sContentHashCode + sFileExt;
                string sSourceFile =
                    ContentGenJob.ContentWorkingPath + "\\" +
                    sContentUniqueId + "\\" +
                    sContentHashCode + sFileExt;

                if (!Directory.Exists(sFilePublishDirectory))
                {
                    throw new ApplicationException(string.Format(AppResource.DirectoryNotExist, sFilePublishDirectory));
                }
                if (!File.Exists(sSourceFile))
                {
                    throw new ApplicationException(string.Format(AppResource.FileNotExist, sSourceFile));
                }
                if (bContentSpecificSubdir && !Directory.Exists(sFilePublishDirectory + "\\" + sContentUniqueId))
                {
                    Directory.CreateDirectory(sFilePublishDirectory + "\\" + sContentUniqueId);
                }
                // Copy & overwrite
                File.Copy(
                    sSourceFile,
                    sFilePublishDirectory + "\\" +
                    (bContentSpecificSubdir ? (sContentUniqueId + "\\") : "") +
                    sDestinationFile,
                    true /*overwrite*/);
            }
        }

        public static void Rollback(string sContentHashCode)
        {
            lock (ContentGenJob.sigLock)
            {
                if (sContentHashCode != "")
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
                    if (File.Exists(
                        AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                        sContentHashCode + ContentGenJob.DownloaderExtension))
                    {
                        File.Delete(
                            AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                            sContentHashCode + ContentGenJob.DownloaderExtension);
                    }
                    if (File.Exists(
                        AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                        sContentHashCode + ContentGenJob.TorrentExtension))
                    {
                        File.Delete(
                            AppConfig.ContentDeployJob.DownloaderDeployTarget + "\\" +
                            sContentHashCode + ContentGenJob.TorrentExtension);
                    }

                    // Remove the deployed torrent files from the offical seeds by sending the remove command anyway
                    IManagementTask oTask = (IManagementTask)new RemoveTorrentTask(sContentHashCode);
                    // Enumerate each seed web for sending the command
                    AppConfig.ContentDeployJob.OfficalSeedWebList.ForEach(oSeedWeb =>
                    {
                        QbtAdapter oAdapter = new QbtAdapter(
                            false,
                            oSeedWeb.IP,
                            oSeedWeb.Port,
                            oSeedWeb.AdminName,
                            oSeedWeb.AdminPassword);
                        // Supress the exception if occurs to keep on trying the next seed
                        try
                        {
                            oAdapter.ExecuteTask(oTask);
                        }
                        catch { }
                    });
                }
            }
        }

        public static List<Tuple<string, Exception>> ProcessContentDeploy(string sContentUniqueId, string sContentHashCode)
        {
            try
            {
                // Deploy Torrent File to the target folder
                CopyFile(
                    AppConfig.ContentDeployJob.TorrentDeployTarget,
                    sContentUniqueId,
                    sContentHashCode,
                    ContentGenJob.TorrentExtension);
                // Deploy Vip Torrent File to the target folder
                CopyFile(
                    AppConfig.ContentDeployJob.VipTorrentDeployTarget,
                    sContentUniqueId,
                    sContentHashCode,
                    ContentGenJob.TorrentVipExtension);
                // Deploy FQDN Torrent File to the target folder
                CopyFile(
                    AppConfig.ContentDeployJob.FqdnTorrentDeployTarget,
                    sContentUniqueId,
                    sContentHashCode,
                    ContentGenJob.TorrentFqdnExtension);
                // Deploy Content Downloader (for portal users download) to the target folder
                CopyFile(
                    AppConfig.ContentDeployJob.DownloaderDeployTarget,
                    sContentUniqueId,
                    sContentHashCode,
                    ContentGenJob.DownloaderExtension,
                    true);
                // Deploy Torrent File (for portal users download) to the target folder
                CopyFile(
                    AppConfig.ContentDeployJob.DownloaderDeployTarget,
                    sContentUniqueId,
                    sContentHashCode,
                    ContentGenJob.TorrentExtension);
            }
            catch (Exception oEx)
            {
                Rollback(sContentHashCode);
                throw oEx;
            }

            string sIP = "";
            List<Tuple<string, Exception>> listFailedSeed = new List<Tuple<string, Exception>>();
            try
            {
                string sFqdnTorrentPublishUrl = AppConfig.ContentDeployJob.FqdnTorrentPublishUrl + "//" + sContentHashCode + ContentGenJob.TorrentFqdnExtension;
                IManagementTask oTask = (IManagementTask)new AddTorrentByUrlTask(sFqdnTorrentPublishUrl);
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
                        listFailedSeed.Add(new Tuple<string, Exception>(sIP,oEx));
                    }
                });
            }
            catch (Exception oEx)
            {
                Rollback(sContentHashCode);
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
                log.InfoFormat(AppResource.StartJobExecution, typeof(ContentDeployJob).Name);
                //=============================================================================

                // Validate the settings & the input data map parameters
                Check.IsNullOrEmpty(sContentUniqueId, GeneralJobDataMapConstants.ContentUniqueId);
                Check.IsNullOrEmpty(sContentHashCode, GeneralJobDataMapConstants.ContentHashCode);
                // Do the deployment
                foreach(Tuple<string, Exception> failedSeed in ProcessContentDeploy(sContentUniqueId, sContentHashCode))
                {
                    //===================================================================================================
                    log.ErrorFormat(
                        AppResource.JobExecutionFailed,
                        failedSeed.Item2,
                        typeof(ContentDeployJob).Name,
                        string.Format(
                            AppResource.OfficalSeedCmdFailed,
                            failedSeed.Item1,
                            typeof(AddTorrentByUrlTask).Name));
                    //===================================================================================================
                }

                //=============================================================================
                log.InfoFormat(AppResource.EndJobExecution, typeof(ContentDeployJob).Name);
                //=============================================================================
            }
            catch (Exception oEx)
            {
                //===================================================================================================
                log.ErrorFormat(AppResource.JobExecutionFailed, oEx, typeof(ContentDeployJob).Name, oEx.Message);
                //===================================================================================================
            }
        }

        #endregion
    }
}
