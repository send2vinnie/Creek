using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Creek.JsonObject;

namespace Creek.Tasks
{
    public class QbtAdapter
    {
        public bool Ssl
        {
            get;
            private set;
        }

        public string WebUiUrl
        {
            get;
            private set;
        }

        public int Port
        {
            get;
            private set;
        }

        public string UserName
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            private set;
        }

        public QbtAdapter(bool ssl, string url, int port, string username, string password)
        {
            Ssl = ssl;
            WebUiUrl = url;
            Port = port;
            UserName = username;
            Password = password;
        }

        public void ExecuteTask(IManagementTask task)
        {
            switch (task.Method)
            {
                case TaskMethod.RetrieveTorrentDetails:
                    {
                        RetrieveTorrentDetailsTask theTask = (RetrieveTorrentDetailsTask)task;
                        // Request the details of all torrents from server
                        JArray details = JArray.Parse(MakeRequest(BuildWebUIUrl("/json/events")));
                        theTask.Result = ParseJsonTorrentDetails(details);
                    }
                    break;

                case TaskMethod.GetTorrentFileList:
                    {
                        GetTorrentFileListTask theTask = (GetTorrentFileListTask)task;
                        // Request files listing for a specific torrent
                        string hash = theTask.TorrentHash.ToLower(); // <== Must be lower cases!!
                        JArray filelist = JArray.Parse(MakeRequest(BuildWebUIUrl("/json/propertiesFiles/") + hash));
                        theTask.Result = ParseJsonFiles(filelist);
                    }
                    break;

                case TaskMethod.AddTorrentByFile:
                    {
                        AddTorrentByFileTask theTask = (AddTorrentByFileTask)task;
                        // Upload a local .torrent file
                        string file = theTask.TorrentFilePath;
                        MakeUploadRequest(BuildWebUIUrl("/command/upload"), file);
                    }
                    break;

                case TaskMethod.AddTorrentByUrl:
                    {
                        AddTorrentByUrlTask theTask = (AddTorrentByUrlTask)task;
                        // Request to add a torrent by URL
                        string url = theTask.TorrentFileUrl;
                        MakeRequest(BuildWebUIUrl("/command/download"), new KeyValuePair<string, string>("urls", url));
                    }
                    break;

                case TaskMethod.AddTorrentByMagnetUrl:
                    {
                        AddTorrentByMagnetUrlTask theTask = (AddTorrentByMagnetUrlTask)task;
                        // Request to add a magnet link by URL
                        string magnet = theTask.TorrentMagnetUrl;
                        MakeRequest(BuildWebUIUrl("/command/download"), new KeyValuePair<string, string>("urls", magnet));
                    }
                    break;

                case TaskMethod.RemoveTorrent:
                    {
                        RemoveTorrentTask theTask = (RemoveTorrentTask)task;
                        // Remove a torrent
                        string hash = theTask.TorrentHash.ToLower(); // <== Must be lower cases!!
                        MakeRequest(BuildWebUIUrl("/command/deletePerm"), new KeyValuePair<string, string>("hashes", hash));
                    }
                    break;

                case TaskMethod.PauseTorrent:
                    {
                        PauseTorrentTask theTask = (PauseTorrentTask)task;
                        // Pause a torrent
                        string hash = theTask.TorrentHash.ToLower(); // <== Must be lower cases!!
                        MakeRequest(BuildWebUIUrl("/command/pause"), new KeyValuePair<string, string>("hash", hash));
                    }
                    break;

                case TaskMethod.ResumeTorrent:
                    {
                        ResumeTorrentTask theTask = (ResumeTorrentTask)task;
                        // Resume a torrent
                        string hash = theTask.TorrentHash.ToLower(); // <== Must be lower cases!!
                        MakeRequest(BuildWebUIUrl("/command/resume"), new KeyValuePair<string, string>("hash", hash));
                    }
                    break;

                case TaskMethod.PauseAllTorrents:
                    {
                        // Resume all torrents
                        MakeRequest(BuildWebUIUrl("/command/pauseall"));
                    }
                    break;

                case TaskMethod.ResumeAllTorrents:
                    {
                        // Resume all torrents
                        MakeRequest(BuildWebUIUrl("/command/resumeall"));
                    }
                    break;

                case TaskMethod.SetTorrentFilePriorities:
                    {
                        throw new NotImplementedException();
                        //// Update the priorities to a set of files
                        //SetTorrentFilePrioritiesTask setPrio = (SetTorrentFilePrioritiesTask)task;
                        //String newPrio = "0";
                        //if (setPrio.getNewPriority() == Priority.Low) {
                        //    newPrio = "1";
                        //} else if (setPrio.getNewPriority() == Priority.Normal) {
                        //    newPrio = "2";
                        //} else if (setPrio.getNewPriority() == Priority.High) {
                        //    newPrio = "7";
                        //}
                        //// We have to make a separate request per file, it seems
                        //for (TorrentFile file in setPrio.FileList) {
                        //    MakeRequest("/command/setFilePrio",
                        //        new KeyValuePair<string, string>("hash", setPrio.TorrentHash),
                        //        new KeyValuePair<string, string>("id", file.Key),
                        //        new KeyValuePair<string, string>("priority", newPrio));
                        //}
                    }
                    break;

                case TaskMethod.SetTorrentTransferRates:
                    {
                        throw new NotImplementedException();
                        //// TODO: This doesn't seem to work yet
                        //// Request to set the maximum transfer rates
                        //SetTorrentTransferRatesTask ratesTask = (SetTorrentTransferRatesTask)task;
                        //int dl = (ratesTask.getDownloadRate() == null ? -1 : ratesTask.getDownloadRate().intValue());
                        //int ul = (ratesTask.getUploadRate() == null ? -1 : ratesTask.getUploadRate().intValue());

                        //// First get the preferences
                        //JObject prefs = JObject.Parse(MakeRequest("/json/preferences"));
                        //prefs.Add("dl_limit", dl);
                        //prefs.Add("up_limit", ul);
                        //MakeRequest("/command/setPreferences", new KeyValuePair<string, string>("json", URLEncoder.encode(prefs.toString(), HTTP.UTF_8)));
                    }
                    break;

                default:
                    throw new NotImplementedException(task.Method.ToString() + " is not supported by " + this.GetType().Name);
                    break;
            }
        }

        private string ReadResponse(HttpWebResponse response)
        {
            Stream stream = response.GetResponseStream();
            using (StreamReader reader = new StreamReader(stream))
            {
                string sResponse = reader.ReadToEnd();
                reader.Close();
                return sResponse;
            }
        }

        private string MakeRequest(string path, params KeyValuePair<string, string>[] kvps)
        {
            try
            {
                // Setup request using POST
                HttpHandler httppost = new HttpHandler();
                Dictionary<string, string> nvps = new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> kvp in kvps)
                {
                    nvps.Add(HttpUtility.UrlEncode(kvp.Key, Encoding.UTF8),
                             HttpUtility.UrlEncode(kvp.Value, Encoding.UTF8));
                }
                lock (typeof(QbtAdapter))
                {
                    using (HttpWebResponse response = httppost.Post(UserName, Password, path, nvps))
                    {
                        string s = ReadResponse(response);
                        response.Close();
                        return s;
                    }
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format("Failed to make the HTTP Post request to {0}", path),
                    e);
            }
        }

        private string MakeUploadRequest(string path, string file)
        {
            try
            {
                // Read file data
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();

                // Generate post objects
                Dictionary<string, object> nvps = new Dictionary<string, object>();
                nvps.Add("torrentfile",
                    new MultiPartPostRequestBuilder.MultiPartFileParameter(
                    data, Path.GetFileName(file)));

                // Setup request using POST
                HttpHandler httppost = new HttpHandler();
                using (HttpWebResponse response = httppost.MultiPartPost(UserName, Password, path, nvps))
                {
                    string s = ReadResponse(response);
                    response.Close();
                    return s;
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(
                    string.Format("Failed to make the Multi-Part HTTP Post request to {0}", path),
                    e);
            }
        }

        /**
         * Build the URL of the web UI request from the user settings
         * @return The URL to request
         */
        private string BuildWebUIUrl(string path)
        {
            return string.Format("{0}{1}:{2}{3}", (Ssl ? "https://" : "http://"), WebUiUrl, Port, path);
        }

        private ArrayList ParseJsonTorrentDetails(JArray response)
        {
            // Parse response
            ArrayList torrents = new ArrayList();
            for (int i = 0; i < response.Count; i++)
            {
                JToken tor = response[i];
                int leechers = ParseLeech(tor.SelectToken("num_leechs").ToString());
                int seeders = ParseSeeds(tor.SelectToken("num_seeds").ToString());
                int known = ParseKnown(tor.SelectToken("num_leechs").ToString(), tor.SelectToken("num_seeds").ToString());
                long size = ParseSize(tor.SelectToken("size").ToString());
                double ratio = ParseRatio(tor.SelectToken("ratio").ToString());
                double progress = Convert.ToDouble(tor.SelectToken("progress").ToString());
                int dlspeed = (int)ParseSpeed(tor.SelectToken("dlspeed").ToString());
                // Add the parsed torrent to the list
                torrents.Add(new TorrentDetail(
                        (long)i,
                        tor.SelectToken("hash").ToString(),
                        tor.SelectToken("name").ToString(),
                        ParseStatus(tor.SelectToken("state").ToString()),
                        null,
                        dlspeed,
                        ParseSpeed(tor.SelectToken("upspeed").ToString()),
                        leechers,
                        leechers + seeders,
                        known,
                        known,
                        (int)((size - (size * progress)) / dlspeed),
                        (long)(size * progress),
                        (long)(size * ratio),
                        size,
                        (float)progress,
                        0f,
                        null,
                        DateTime.MinValue,
                        null));
            }
            // Return the list
            return torrents;
        }

        private double ParseRatio(string tokenString)
        {
            // Ratio is given in "1.5" string format
            try
            {
                return Double.Parse(tokenString);
            }
            catch (Exception e)
            {
                return 0D;
            }
        }

        private long ParseSize(string tokenString)
        {
            // Sizes are given in "703.3 MiB" string format
            // Returns size in B-based long
            string[] parts = tokenString.Split(new char[] { ' ' });
            if (parts[1].Equals("GiB"))
            {
                return (long)(Double.Parse(parts[0]) * 1024L * 1024L * 1024L);
            }
            else if (parts[1].Equals("MiB"))
            {
                return (long)(Double.Parse(parts[0]) * 1024L * 1024L);
            }
            else if (parts[1].Equals("KiB"))
            {
                return (long)(Double.Parse(parts[0]) * 1024L);
            }
            return (long)(Double.Parse(parts[0]));
        }

        private int ParseKnown(string leechs, string seeds)
        {
            // Peers are given in the "num_leechs":"91 (449)","num_seeds":"6 (27)" strings
            // Or sometimes just "num_leechs":"91","num_seeds":"6" strings
            // Peers known are in the last () bit of the leechers and seeders
            int leechers = 0;
            if (leechs.IndexOf("(") < 0)
            {
                leechers = Int32.Parse(leechs);
            }
            else
            {
                leechers = Int32.Parse(leechs.Substring(leechs.IndexOf("(") + 1, leechs.IndexOf(")") - leechs.IndexOf("(") - 1));
            }
            int seeders = 0;
            if (seeds.IndexOf("(") < 0)
            {
                seeders = Int32.Parse(seeds);
            }
            else
            {
                seeders = Int32.Parse(seeds.Substring(seeds.IndexOf("(") + 1, seeds.IndexOf(")") - seeds.IndexOf("(") - 1));
            }
            return leechers + seeders;
        }

        private int ParseSeeds(string seeds)
        {
            // Seeds are in the first part of the "num_seeds":"6 (27)" string
            // In some situations it it just a "6" string
            if (seeds.IndexOf(" ") < 0)
            {
                return Int32.Parse(seeds);
            }
            return Int32.Parse(seeds.Substring(0, seeds.IndexOf(" ")));
        }

        private int ParseLeech(string leechs)
        {
            // Leechers are in the first part of the "num_leechs":"91 (449)" string
            // In some situations it it just a "0" string
            if (leechs.IndexOf(" ") < 0)
            {
                return Int32.Parse(leechs);
            }
            return Int32.Parse(leechs.Substring(0, leechs.IndexOf(" ")));
        }

        private int ParseSpeed(string speed)
        {
            // Speeds are in "21.9 KiB/s" string format
            // Returns speed in B/s-based integer
            string[] parts = speed.Split(new char[] { ' ' });
            if (parts[1].Equals("GiB/s"))
            {
                return (int)(Double.Parse(parts[0]) * 1024 * 1024 * 1024);
            }
            else if (parts[1].Equals("MiB/s"))
            {
                return (int)(Double.Parse(parts[0]) * 1024 * 1024);
            }
            else if (parts[1].Equals("KiB/s"))
            {
                return (int)(Double.Parse(parts[0]) * 1024);
            }
            return (int)(Double.Parse(parts[0]));
        }

        private TorrentStatus ParseStatus(string state)
        {
            // Status is given as a descriptive string
            if (state.Equals("downloading"))
            {
                return TorrentStatus.Downloading;
            }
            else if (state.Equals("uploading"))
            {
                return TorrentStatus.Seeding;
            }
            else if (state.Equals("pausedDL"))
            {
                return TorrentStatus.Paused;
            }
            else if (state.Equals("pausedUP"))
            {
                return TorrentStatus.Paused;
            }
            else if (state.Equals("stalledUP"))
            {
                return TorrentStatus.Seeding;
            }
            else if (state.Equals("stalledDL"))
            {
                return TorrentStatus.Downloading;
            }
            else if (state.Equals("checkingUP"))
            {
                return TorrentStatus.Checking;
            }
            else if (state.Equals("checkingDL"))
            {
                return TorrentStatus.Checking;
            }
            else if (state.Equals("queuedDL"))
            {
                return TorrentStatus.Queued;
            }
            else if (state.Equals("queuedUP"))
            {
                return TorrentStatus.Queued;
            }
            return TorrentStatus.Unknown;
        }

        private ArrayList ParseJsonFiles(JArray response)
        {
            // Parse response
            ArrayList torrentfiles = new ArrayList();
            for (int i = 0; i < response.Count; i++)
            {
                JToken file = response[i];
                long size = ParseSize((string)file.SelectToken("size"));
                torrentfiles.Add(new TorrentFile(
                        "" + i,
                        file.SelectToken("name").ToString(),
                        null,
                        null,
                        size,
                        (long)(size * Convert.ToDouble(file.SelectToken("progress").ToString())),
                        ParsePriority(Convert.ToInt32(file.SelectToken("priority").ToString()))));
            }
            // Return the list
            return torrentfiles;
        }

        private Priority ParsePriority(int priority)
        {
            // Priority is an integer
            // Actually 1 = Normal, 2 = High, 7 = Maximum, but adjust this to Transdroid values
            if (priority == 0)
            {
                return Priority.Off;
            }
            else if (priority == 1)
            {
                return Priority.Low;
            }
            else if (priority == 2)
            {
                return Priority.Normal;
            }
            return Priority.High;
        }
    }
}
