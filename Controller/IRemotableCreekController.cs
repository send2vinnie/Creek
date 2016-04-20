using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Controller
{
    public interface IRemotableCreekController
    {
        Tuple<string /* IP */, bool /* Availability */, string[] /* Torrent list */>[] GetMonitoringSeedList();

        Tuple<string /* Content Id */, bool /* Availability */>[] GetMonitoringContentList();

        Tuple<string /* Name */, string /* Value */>[] GetContentSummary(string sContentUniqueId);

        Tuple<DateTime /* execute time */, string /* completion ratio */>[] GetWorkInProgressList(string sContentUniqueId);

        Tuple<
            DateTime /* date created */,
            string /* name */,
            string /* hash */,
            bool /* deploy to tracker */,
            float /* seed deployment ratio */,
            float /* seeding ratio */>[] GetContentVersionList(string sContentUniqueId);

        Tuple<
            string /* Seed Web IP */,
            long /* total size */,
            float /* part done */,
            string /* status */,
            string /* error */>[] GetTorrentSeedDetail(string sContentUniqueId, string sContentHashCode);

        // Content Recipes History
        string GetContentGenRecipesJson(string sContentUniqueId, string sContentHashCode);

        string[] GetDownloaderDisplayNameHistory(string sContentUniqueId);

        string[] GetDownloaderHomeUrlHistory(string sContentUniqueId);

        string[] GetDownloaderOnlineFaqUrlHistory(string sContentUniqueId);

        string[] GetDownloaderGAProfileIdHistory(string sContentUniqueId);

        string[] GetDownloaderPromoEventIdHistory(string sContentUniqueId);

        string[] GetDownloaderPromoEventServerUrlHistory(string sContentUniqueId);

        string GetDownloaderDefaultValues();

        string[] GetIconFileList();

        string[] GetDisclaimerFileList();

        // Content Operations
        void GenerateContentReq(string sContentUniqueId, string sContentGenRecipesJson);

        void InterruptContentGenReq(string sContentUniqueId);

        void DeployContentVersionReq(string sContentUniqueId, string sContentHashCode);

        void OfflineContentVersionReq(string sContentUniqueId, string sContentHashCode);

        void PauseContentVersionReq(string sContentUniqueId, string sContentHashCode);

        void ResumeContentVersionReq(string sContentUniqueId, string sContentHashCode);

        // Content Monitoring
        string GetContentMonitoringJson(string sContentUniqueId);

        // Log
        Tuple<DateTime /* log time */, string /* level */, string /* message */>[] GetExecLog(int startIndex, int count, out int recordLast);
    }
}
