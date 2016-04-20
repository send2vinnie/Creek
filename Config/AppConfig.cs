using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Creek.Config
{
    internal class AppConfig
    {
        private const string PieceLengthKB = "ContentGenJob.PieceLengthKB";
        private const string CreatedBy = "ContentGenJob.CreatedBy";
        private const string TrackerAnnounceUrl = "ContentGenJob.TrackerAnnounceUrl";
        private const string InternalTrackerAnnounceUrl = "ContentGenJob.InternalTrackerAnnounceUrl";
        private const string GAProfileIdDefaultValue = "ContentGenJob.GAProfileIdDefaultValue";
        private const string OnlineFaqUrlDefaultValue = "ContentGenJob.OnlineFaqUrlDefaultValue";
        private const string PromoEventIdDefaultValue = "ContentGenJob.PromoEventIdDefaultValue";
        private const string PromoEventServerUrlDefaultValue = "ContentGenJob.PromoEventServerUrlDefaultValue";

        private const string TorrentDeployTarget = "ContentDeployJob.TorrentDeployTarget";
        private const string VipTorrentDeployTarget = "ContentDeployJob.VipTorrentDeployTarget";
        private const string FqdnTorrentDeployTarget = "ContentDeployJob.FqdnTorrentDeployTarget";
        private const string DownloaderDeployTarget = "ContentDeployJob.DownloaderDeployTarget";
        private const string FqdnTorrentPublishUrl = "ContentDeployJob.FqdnTorrentPublishUrl";

        private const string OfficalSeedWebIPList = "ContentDeployJob.OfficalSeedWebIPList";
        private const string OfficalSeedWebPort = "ContentDeployJob.OfficalSeedWebPort";
        private const string OfficalSeedWebAdminName = "ContentDeployJob.OfficalSeedWebAdminName";
        private const string OfficalSeedWebAdminPassword = "ContentDeployJob.OfficalSeedWebAdminPassword";

        private const string TrackerMonitorUrl = "ContentMonitorJob.TrackerMonitorUrl";
        private const string TrackerAdminName = "ContentMonitorJob.TrackerAdminName";
        private const string TrackerAdminPassword = "ContentMonitorJob.TrackerAdminPassword";

        private static string GetSettingWithValidation<T>(string sKeyName, bool bMandatory = true)
        {
            T oTInstance = default(T);
            string sValue = ConfigurationManager.AppSettings[sKeyName];

            if (bMandatory && (sValue == null || sValue.Length == 0))
            {
                throw new ApplicationException(string.Format(AppResource.InvalidAppSetting, sKeyName));
            }
            if (oTInstance is int && sValue.Length > 0)
            {
                int iValue;
                if (!int.TryParse(sValue, out iValue))
                {
                    throw new ApplicationException(string.Format(AppResource.InvalidNumbericalValue, sKeyName));
                }
            }
            if (oTInstance is long && sValue.Length > 0)
            {
                long lValue;
                if (!long.TryParse(sValue, out lValue))
                {
                    throw new ApplicationException(string.Format(AppResource.InvalidNumbericalValue, sKeyName));
                }
            }
            if (oTInstance is DirectoryInfo && sValue.Length > 0)
            {
                if (!Directory.Exists(sValue))
                {
                    throw new ApplicationException(string.Format(AppResource.DirectoryNotExist, sValue));
                }
            }
            return sValue;
        }

        internal class ContentGenJob
        {
            public static long PieceLengthKB
            {
                get
                {
                    return long.Parse(GetSettingWithValidation<long>(AppConfig.PieceLengthKB));
                }
            }

            public static string CreatedBy
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.CreatedBy);
                }
            }

            public static string TrackerAnnounceUrl
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.TrackerAnnounceUrl);
                }
            }

            public static string InternalTrackerAnnounceUrl
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.InternalTrackerAnnounceUrl);
                }
            }

            public static string GAProfileIdDefaultValue
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.GAProfileIdDefaultValue);
                }
            }

            public static string OnlineFaqUrlDefaultValue
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.OnlineFaqUrlDefaultValue);
                }
            }

            public static string PromoEventIdDefaultValue
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.PromoEventIdDefaultValue);
                }
            }

            public static string PromoEventServerUrlDefaultValue
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.PromoEventServerUrlDefaultValue);
                }
            }
        }

        internal class ContentDeployJob
        {
            internal class OfficalSeedWeb
            {
                public OfficalSeedWeb(string sIP, int nPort, string sAdminName, string sAdminPassword)
                {
                    IP = sIP;
                    Port = nPort;
                    AdminName = sAdminName;
                    AdminPassword = sAdminPassword;
                }

                public string IP
                { get; private set; }

                public int Port
                { get; private set; }

                public string AdminName
                { get; private set; }

                public string AdminPassword
                { get; private set; }
            }

            public static string TorrentDeployTarget
            {
                get
                {
                    return GetSettingWithValidation<DirectoryInfo>(AppConfig.TorrentDeployTarget);
                }
            }

            public static string VipTorrentDeployTarget
            {
                get
                {
                    return GetSettingWithValidation<DirectoryInfo>(AppConfig.VipTorrentDeployTarget);
                }
            }

            public static string FqdnTorrentDeployTarget
            {
                get
                {
                    return GetSettingWithValidation<DirectoryInfo>(AppConfig.FqdnTorrentDeployTarget);
                }
            }

            public static string FqdnTorrentPublishUrl
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.FqdnTorrentPublishUrl);
                }
            }

            public static string DownloaderDeployTarget
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.DownloaderDeployTarget);
                }
            }

            public static List<OfficalSeedWeb> OfficalSeedWebList
            {
                get
                {
                    List<string> listSeedIP = new List<string>();
                    List<OfficalSeedWeb> listSeedWeb = new List<OfficalSeedWeb>();

                    foreach(string sSeedIP in OfficalSeedWebIPList)
                    {
                        if (listSeedIP.Contains(sSeedIP))
                        {
                            throw new ApplicationException(string.Format(AppResource.InvalidSeedWebIPListSetting, AppConfig.OfficalSeedWebIPList));
                        }
                        else
                        {
                            listSeedIP.Add(sSeedIP);
                        }
                    }
                    listSeedIP.ForEach(sIP => { listSeedWeb.Add(new OfficalSeedWeb(
                        sIP,
                        OfficalSeedWebPort,
                        OfficalSeedWebAdminName,
                        OfficalSeedWebAdminPassword)); });
                    return listSeedWeb;
                }
            }

            public static string[] OfficalSeedWebIPList
            {
                get
                {
                    return GetSettingWithValidation<string[]>(AppConfig.OfficalSeedWebIPList, false /*optional*/).Split(new char[] { ';', ',' });
                }
            }

            public static int OfficalSeedWebPort
            {
                get
                {
                    return int.Parse(GetSettingWithValidation<int>(AppConfig.OfficalSeedWebPort, false /*optional*/));
                }
            }

            public static string OfficalSeedWebAdminName
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.OfficalSeedWebAdminName, false /*optional*/);
                }
            }

            public static string OfficalSeedWebAdminPassword
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.OfficalSeedWebAdminPassword, false /*optional*/);
                }
            }
        }

        internal class ContentMonitorJob
        {
            public static string TrackerMonitorUrl
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.TrackerMonitorUrl);
                }
            }

            public static string TrackerAdminName
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.TrackerAdminName, false /*optional*/);
                }
            }

            public static string TrackerAdminPassword
            {
                get
                {
                    return GetSettingWithValidation<string>(AppConfig.TrackerAdminPassword, false /*optional*/);
                }
            }
        }
    }
}
