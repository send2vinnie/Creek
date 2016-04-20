using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Creek.Jobs;

namespace Creek.JsonObject
{
    public class ContentGenRecipes
    {
        public static string GetJson(ContentGenRecipes oRecipes)
        {
            return JsonConvert.SerializeObject(oRecipes, Formatting.Indented);
        }

        public static ContentGenRecipes FromJson(string sJson)
        {
            return JsonConvert.DeserializeObject<ContentGenRecipes>(sJson);
        }

        public static string UrlListToString(List<string> listUrl)
        {
            string sUrls = "";
            listUrl.ForEach(x => { sUrls += (HttpUtility.UrlEncode(x) + ";"); });
            return sUrls;
        }

        public static List<string> StringToUrlList(string sUrls)
        {
            return sUrls.Split(new char[] { ';', ',' }).ToList<string>();
        }

        public bool AutoDeploy
        {
            get;
            set;
        }

        public DateTime CreateDateTime
        {
            get;
            set;
        }

        public string ContentHashCode
        {
            get;
            set;
        }

        public string ContentFileName
        {
            get;
            set;
        }

        public string ContentSourceUrl
        {
            get;
            set;
        }

        public string HttpSeedsUrl
        {
            get;
            set;
        }

        [JsonIgnore]
        public List<string> HttpSeedsUrlList
        {
            get
            {
                return StringToUrlList(HttpSeedsUrl);
            }
        }

        public string VipHttpSeedsUrl
        {
            get;
            set;
        }

        [JsonIgnore]
        public List<string> VipHttpSeedsUrlList
        {
            get
            {
                return StringToUrlList(VipHttpSeedsUrl);
            }
        }

        public string DownloaderDisplayName
        {
            get;
            set;
        }

        public string DownloaderHomeUrl
        {
            get;
            set;
        }

        public string OnlineFaqUrl
        {
            get;
            set;
        }

        public string IconFile
        {
            get;
            set;
        }

        public string DisclaimerFile
        {
            get;
            set;
        }

        public string GAProfileId
        {
            get;
            set;
        }

        public string PromotionEventID
        {
            get;
            set;
        }

        public string PromotionEventServerUrl
        {
            get;
            set;
        }

        public bool UPXCompression
        {
            get;
            set;
        }
    }
}
