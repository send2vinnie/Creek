using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Jobs
{
    public static class ContentGenConstants
    {
        public const string ContentGenGroupName = "ContentGen";
    }

    public static class ContentDeployConstants
    {
        public const string ContentDeployGroupName = "ContentDeploy";
        public const string ContentDeployJobName = "ContentDeployJob";
    }

    public static class ContentOfflineConstants
    {
        public const string ContentOfflineGroupName = "ContentOffline";
        public const string ContentOfflineJobName = "ContentOfflineJob";
    }

    public static class ContentPauseConstants
    {
        public const string ContentPauseGroupName = "ContentPause";
        public const string ContentPauseJobName = "ContentPauseJob";
    }

    public static class ContentResumeConstants
    {
        public const string ContentResumeGroupName = "ContentResume";
        public const string ContentResumeJobName = "ContentResumeJob";
    }

    public static class ContentMonitorConstants
    {
        public const int ContentMonitorPeriodSec = 10;
        public const string ContentMonitorGroupName = "ContentMonitor";
    }

    public static class SeedMonitorConstants
    {
        public const int SeedMonitorPeriodSec = 10;
        public const string SeedMonitorGroupName = "SeedMonitor";
    }

    public static class SeedMonitorJobDataMapConstants
    {
        public const string OfficalSeedWeb = "OfficalSeedWeb";
        public const string TorrentDetails = "TorrentDetails";
    }

    public static class GeneralJobDataMapConstants
    {
        public const string ContentUniqueId = "ContentUniqueId";
        public const string ContentHashCode = "ContentHashCode";
    }

    public static class ContentMonitorJobDataMapConstants
    {
        public const string ContentDetail = "ContentDetail";
    }

    public static class ContentGenJobDataMapConstants
    {
        public const string ContentRecipesJson = "ContentRecipesJson";
        public const string OverallCompletion = "OverallCompletion";
    }

    public enum ContentRecipesPropHistory
    {
        DownloaderDisplayName,
        DownloaderHomeUrl,
        OnlineFaqUrl,
        GAProfileId,
        PromoEventId,
        PromoEventServerUrl
    }
}
