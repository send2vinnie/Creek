using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creek.Utility;

namespace Creek.JsonObject
{

    /**
    * Represents a single file contained in a torrent.
    * 
    * @author erickok
    *
    */
    public class TorrentFile
    {

        private string key;
        private string name;
        private string relativePath;
        private string fullPath;
        private long totalSize;
        private long downloaded;
        private Priority priority;

        public TorrentFile(String key, String name, String relativePath, String fullPath, long size, long done, Priority priority)
        {
            this.key = key;
            this.name = name;
            this.relativePath = relativePath;
            this.fullPath = fullPath;
            this.totalSize = size;
            this.downloaded = done;
            this.priority = priority;
        }

        public string Key
        {
            get
            {
                return this.key;
            }
        }
        public string getName
        {
            get
            {
                return this.name;
            }
        }
        public string RelativePath
        {
            get
            {
                return this.relativePath;
            }
        }
        public string FullPath
        {
            get
            {
                return this.fullPath;
            }
        }
        public long TotalSize
        {
            get
            {
                return this.totalSize;
            }
        }
        public long Downloaded
        {
            get
            {
                return this.downloaded;
            }
        }

        public Priority Priority
        {
            get
            {
                return priority;
            }
        }

        public float PartDone
        {
            get
            {
                return (float)downloaded / (float)totalSize;
            }
        }

        /**
         * Returns a text showing the percentage that is already downloaded of this file
         * @return A string indicating the progress, e.g. '85%'
         */
        public string ProgressText
        {
            get
            {
                return String.Format("{0:0.##}%", PartDone * 100);
            }
        }

        /**
         * Returns a text showing the downloaded and total sizes of this file
         * @return A string with the sizes, e.g. '125.3 of 251.2 MB'
         */
        public string DownloadedAndTotalSizeText
        {
            get
            {
                return FileSizeConverter.GetSize(Downloaded) + " / " + FileSizeConverter.GetSize(TotalSize);
            }
        }

        /**
         * Returns if the download for this file is complete
         * @return True if the downloaded size equals the total size, i.e. if it is completed
         */
        public bool IsComplete
        {
            get
            {
                return Downloaded == TotalSize;
            }
        }

        /**
         * Returns the full path of this file as it should be located on the remote server
         * @return The full path, as String
         */
        public string FullPathUri
        {
            get
            {
                return "file://" + FullPath;
            }
        }

        /**
         * Try to infer the mime type of this file
         * @return The mime type of this file, or null if it could not be inferred
         */
        public string MimeType
        {
            // TODO: Test if this still works
            get
            {
                if (FullPath != null && FullPath.Contains("."))
                {
                    string ext = FullPath.Substring(FullPath.LastIndexOf('.') + 1);
                    if (mimeTypes.ContainsKey(ext))
                    {
                        // One of the known extensions: return logical mime type
                        return mimeTypes[ext];
                    }
                }
                // Unknown/none/unregistered extension: return null
                return null;
            }
        }

        private static Dictionary<String, String> mimeTypes = fillMimeTypes();
        private static Dictionary<String, String> fillMimeTypes()
        {
            // Full mime type support list is in http://code.google.com/p/android-vlc-remote/source/browse/trunk/AndroidManifest.xml
            // We use a selection of the most popular/obvious ones
            Dictionary<String, String> types = new Dictionary<String, String>();
            // Application
            types.Add("m4a", "application/x-extension-m4a");
            types.Add("flac", "application/x-flac");
            types.Add("mkv", "application/x-matroska");
            types.Add("ogg", "application/x-ogg");
            // Audio
            types.Add("m3u", "audio/mpegurl");
            types.Add("mp3", "audio/mpeg");
            types.Add("mpa", "audio/mpeg");
            types.Add("mpc", "audio/x-musepack");
            types.Add("wav", "audio/x-wav");
            types.Add("wma", "audio/x-ms-wma");
            // Video
            types.Add("3gp", "video/3gpp");
            types.Add("avi", "video/x-avi");
            types.Add("flv", "video/x-flv");
            types.Add("mov", "video/quicktime");
            types.Add("mp4", "video/mp4");
            types.Add("mpg", "video/mpeg");
            types.Add("mpeg", "video/mpeg");
            types.Add("vob", "video/mpeg");
            types.Add("wmv", "video/x-ms-wmv");
            return types;
        }
    }
}
