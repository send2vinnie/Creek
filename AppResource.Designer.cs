﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Creek {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AppResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Creek.AppResource", typeof(AppResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 內容監控狀態資料 (Content Details) 讀取失敗, 原因: {0}.
        /// </summary>
        internal static string ContentDetailReadFailed {
            get {
                return ResourceManager.GetString("ContentDetailReadFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 內容尚未部署至官方種子.
        /// </summary>
        internal static string ContentNotDeployToSeed {
            get {
                return ResourceManager.GetString("ContentNotDeployToSeed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 內容製作輸入資料 (Recipes) 讀取失敗, 原因: {0}.
        /// </summary>
        internal static string ContentRecipesReadFailed {
            get {
                return ResourceManager.GetString("ContentRecipesReadFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 待監控之內容製作輸入資料 (Recipes) 移除失敗, 除設定檔之外請避免手動更新任何其他之檔案, 原因: {0}.
        /// </summary>
        internal static string ContentRecipesRemoveFailed {
            get {
                return ResourceManager.GetString("ContentRecipesRemoveFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 待監控之內容製作輸入資料 (Recipes) 更新失敗, 除設定檔之外請避免手動更新任何其他之檔案, 原因: {0}.
        /// </summary>
        internal static string ContentRecipesUpdateFailed {
            get {
                return ResourceManager.GetString("ContentRecipesUpdateFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 待監控之內容版本清單更新失敗, 除設定檔之外請避免手動更新任何其他之檔案, 原因: {0}.
        /// </summary>
        internal static string ContentVersionListUpdateFailed {
            get {
                return ResourceManager.GetString("ContentVersionListUpdateFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的檔案目錄{0}不存在, 請檢查參數設定是否正確.
        /// </summary>
        internal static string DirectoryNotExist {
            get {
                return ResourceManager.GetString("DirectoryNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 下載器產生.
        /// </summary>
        internal static string DownloaderGenTaskName {
            get {
                return ResourceManager.GetString("DownloaderGenTaskName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}作業執行完畢.
        /// </summary>
        internal static string EndJobExecution {
            get {
                return ResourceManager.GetString("EndJobExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的檔案{0}不存在, 請檢查參數設定是否正確.
        /// </summary>
        internal static string FileNotExist {
            get {
                return ResourceManager.GetString("FileNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的參數或設定值不存在或長度為零, 請檢查參數或設定是否正確.
        /// </summary>
        internal static string InputParameterRequired {
            get {
                return ResourceManager.GetString("InputParameterRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的參數必須是大於或等於零的數值, 請檢查參數或設定是否正確.
        /// </summary>
        internal static string InputParameterSouldBePositive {
            get {
                return ResourceManager.GetString("InputParameterSouldBePositive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 應用程式設定資料 ({0}) 不存在, 請檢查參數設定是否正確.
        /// </summary>
        internal static string InvalidAppSetting {
            get {
                return ResourceManager.GetString("InvalidAppSetting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 設定檔中的 &quot;ContentGenJob.CreatedBy&quot; 設定有誤, 必須為長度大於零之字串值.
        /// </summary>
        internal static string InvalidContentGenJobConfigCreatedBy {
            get {
                return ResourceManager.GetString("InvalidContentGenJobConfigCreatedBy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 設定檔中的 &quot;ContentGenJob.InternalTrackerAnnounceUrl&quot; 設定有誤, 必須為長度大於零之字串值.
        /// </summary>
        internal static string InvalidContentGenJobConfigInternalTrackerAnnounceUrl {
            get {
                return ResourceManager.GetString("InvalidContentGenJobConfigInternalTrackerAnnounceUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 設定檔中的 &quot;ContentGenJob.PieceLengthKB&quot; 設定有誤, 必須為大於零之正整數.
        /// </summary>
        internal static string InvalidContentGenJobConfigPieceLengthKB {
            get {
                return ResourceManager.GetString("InvalidContentGenJobConfigPieceLengthKB", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 設定檔中的 &quot;ContentGenJob.TrackerAnnounceUrl&quot; 設定有誤, 必須為長度大於零之字串值.
        /// </summary>
        internal static string InvalidContentGenJobConfigTrackerAnnounceUrl {
            get {
                return ResourceManager.GetString("InvalidContentGenJobConfigTrackerAnnounceUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 傳入 &quot;ContentGenJob&quot; 的參數 &quot;ContentSourceUrl&quot; 有誤, 必須為長度大於零之字串值.
        /// </summary>
        internal static string InvalidContentGenJobDataMapContentSourceUrl {
            get {
                return ResourceManager.GetString("InvalidContentGenJobDataMapContentSourceUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的免責聲明 &quot;RFT檔&quot; 不存在.
        /// </summary>
        internal static string InvalidDisclaimerFullPath {
            get {
                return ResourceManager.GetString("InvalidDisclaimerFullPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請輸入下載內容物的 &quot;顯示名稱&quot;.
        /// </summary>
        internal static string InvalidDownloaderDisplayName {
            get {
                return ResourceManager.GetString("InvalidDownloaderDisplayName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請為下載器產生一個專用的 GUID.
        /// </summary>
        internal static string InvalidDownloaderGuid {
            get {
                return ResourceManager.GetString("InvalidDownloaderGuid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請輸入下載內容物的 &quot;官網位址&quot;.
        /// </summary>
        internal static string InvalidDownloaderHomeUrl {
            get {
                return ResourceManager.GetString("InvalidDownloaderHomeUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請輸入下Google Analytics報表的 &quot;資源編號&quot;.
        /// </summary>
        internal static string InvalidGoogleAnalyticsProfileID {
            get {
                return ResourceManager.GetString("InvalidGoogleAnalyticsProfileID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的下載內容物 &quot;Torrent檔&quot; 不存在.
        /// </summary>
        internal static string InvalidMetafileFullPath {
            get {
                return ResourceManager.GetString("InvalidMetafileFullPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 應用程式設定資料 ({0}) 必須為數值型態, 請檢查參數設定是否正確.
        /// </summary>
        internal static string InvalidNumbericalValue {
            get {
                return ResourceManager.GetString("InvalidNumbericalValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 索取虛寶活動的 &quot;活動序號&quot; 格式不正確, 必須為長度36字元的序號!.
        /// </summary>
        internal static string InvalidPromotionEventID {
            get {
                return ResourceManager.GetString("InvalidPromotionEventID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請輸入索取虛寶活動的 &quot;活動伺服器&quot; 的URL.
        /// </summary>
        internal static string InvalidPromotionEventServerUrl {
            get {
                return ResourceManager.GetString("InvalidPromotionEventServerUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 應用程式設定資料 ({0}) 不正確, 官方種子IP位址清單設定必須為以分號 (;) 區隔且不重複之IP位址.
        /// </summary>
        internal static string InvalidSeedWebIPListSetting {
            get {
                return ResourceManager.GetString("InvalidSeedWebIPListSetting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}作業執行失敗, 原因: {1}.
        /// </summary>
        internal static string JobExecutionFailed {
            get {
                return ResourceManager.GetString("JobExecutionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 中介資料部署.
        /// </summary>
        internal static string MetafileDeployTaskName {
            get {
                return ResourceManager.GetString("MetafileDeployTaskName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 中介資料檔產生.
        /// </summary>
        internal static string MetafileGenTaskName {
            get {
                return ResourceManager.GetString("MetafileGenTaskName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 找不到 &quot;NLog.config&quot; 設定檔, 本系統採用 NLog 做為系統日誌記錄工具, 請確定 NLog 之設定是否正確.
        /// </summary>
        internal static string NLogConfigNotFound {
            get {
                return ResourceManager.GetString("NLogConfigNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 官方種子 (IP={0}) 命令執行作業 ({1}) 失敗.
        /// </summary>
        internal static string OfficalSeedCmdFailed {
            get {
                return ResourceManager.GetString("OfficalSeedCmdFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到Banner Bitmap (207), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResBitmap207NotFound {
            get {
                return ResourceManager.GetString("ResBitmap207NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到Dialog Template (201), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResDialog201NotFound {
            get {
                return ResourceManager.GetString("ResDialog201NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到Disclaimer Template (40112), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResDisclaimerSlot40112NotFound {
            get {
                return ResourceManager.GetString("ResDisclaimerSlot40112NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到Embedded Satellite Resource (1000), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResEmbededSatelliteDllNotFound {
            get {
                return ResourceManager.GetString("ResEmbededSatelliteDllNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到程式 ICON (128), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResIcon128NotFound {
            get {
                return ResourceManager.GetString("ResIcon128NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到String Template ({0}), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResStringTemplateNotFound {
            get {
                return ResourceManager.GetString("ResStringTemplateNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 原因: 找不到Torrent Template (1022), 下載器生成作業無法繼續.
        /// </summary>
        internal static string ResTorrentSlot2011NotFound {
            get {
                return ResourceManager.GetString("ResTorrentSlot2011NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 與官方種子間之連線尚未建立或無法建立連線.
        /// </summary>
        internal static string SeedWebConnectionFailed {
            get {
                return ResourceManager.GetString("SeedWebConnectionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 開始執行{0}作業.
        /// </summary>
        internal static string StartJobExecution {
            get {
                return ResourceManager.GetString("StartJobExecution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 系統初始化失敗, 請根據系統日誌記錄檔所提供的資訊進行問題排除.
        /// </summary>
        internal static string SystemInitializationFailed {
            get {
                return ResourceManager.GetString("SystemInitializationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 作業步驟 ({0}) 執行完畢.
        /// </summary>
        internal static string TaskExecutionDone {
            get {
                return ResourceManager.GetString("TaskExecutionDone", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 下載器壓縮失敗, 請重新再試或選擇不壓縮下載器.
        /// </summary>
        internal static string UPXCompressionFailed {
            get {
                return ResourceManager.GetString("UPXCompressionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指定的URL ({0}) 不存在, 請檢查參數設定是否正確.
        /// </summary>
        internal static string UrlNotExist {
            get {
                return ResourceManager.GetString("UrlNotExist", resourceCulture);
            }
        }
    }
}
