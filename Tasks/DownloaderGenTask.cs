using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using Vestris.ResourceLib;

namespace Creek.Tasks
{
    public class DownloaderGenTask : IManagementTask
    {
        private const string constUPX = "upx.exe";
        private const string constGLoaderResName = "GLoader.exe.gz";
        private const string constGLoaderPromoResName = "GLoader-Gamania P2P Promotion.exe.gz";
        private const string constDownloaderSatelliteResource = "GLoader.Resource.dll";

        public string WorkingDir
        {
            get;
            private set;
        }

        public string DownloaderGuid
        {
            get;
            private set;
        }

        public string MetafileFullPath
        {
            get;
            private set;
        }

        public string BannerBitmapFullPath
        {
            get;
            private set;
        }

        public string IconFullPath
        {
            get;
            private set;
        }

        public string PromotionEventID
        {
            get;
            private set;
        }

        public string DownloaderFileName
        {
            get;
            private set;
        }

        public string DownloaderDisplayName
        {
            get;
            private set;
        }

        public string DownloaderHomeUrl
        {
            get;
            private set;
        }

        public string DisclaimerFullPath
        {
            get;
            private set;
        }

        public string OnlineFaqUrl
        {
            get;
            private set;
        }

        public string GoogleAnalyticsProfileID
        {
            get;
            private set;
        }

        public string PromotionEventServerUrl
        {
            get;
            private set;
        }

        public bool UPXCompression
        {
            get;
            private set;
        }

        public DownloaderGenTask(
            string workingDir,
            string downloaderGuid,
            string metafileFullPath,
            string bannerBitmapFullPath,
            string iconFullPath,
            string promotionEventID,
            string downloaderFileName,
            string downloaderDisplayName,
            string downloaderHomeUrl,
            string disclaimerFullPath,
            string onlineFaqUrl,
            string googleAnalyticsProfileID,
            string promotionEventServerUrl,
            bool needCompression)
        {
            Method = TaskMethod.CreateDownloader;
            // Initialization
            WorkingDir = workingDir + "\\";
            DownloaderGuid = downloaderGuid;
            MetafileFullPath = metafileFullPath;
            BannerBitmapFullPath = bannerBitmapFullPath;
            IconFullPath = iconFullPath;
            PromotionEventID = promotionEventID;
            DownloaderFileName = downloaderFileName;
            DownloaderDisplayName = downloaderDisplayName;
            DownloaderHomeUrl = downloaderHomeUrl;
            DisclaimerFullPath = disclaimerFullPath;
            OnlineFaqUrl = onlineFaqUrl;
            GoogleAnalyticsProfileID = googleAnalyticsProfileID;
            PromotionEventServerUrl = promotionEventServerUrl;
            UPXCompression = needCompression;
        }

        // readStream is the stream you need to read
        // writeStream is the stream you want to write to
        private void readWriteStream(Stream readStream, Stream writeStream)
        {
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = readStream.Read(buffer, 0, Length);
            // write the required bytes
            while (bytesRead > 0)
            {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, Length);
            }
            readStream.Close();
            writeStream.Close();
        }

        private static int msCtrlIdToFind = 0;
        // Explicit predicate delegate.
        private static bool FindDlgControl(DialogTemplateControlBase ctrl)
        {
            DialogExTemplateControl ctrlEX = (DialogExTemplateControl)ctrl;
            if (ctrlEX.Id == msCtrlIdToFind)
            {
                return true;
            }
            {
                return false;
            }
        }

        private static int msResIdToFind = 0;
        // Explicit predicate delegate.
        private static bool FindRes(Resource rc)
        {
            if (rc.Name.ToString() == msResIdToFind.ToString())
            {
                return true;
            }
            {
                return false;
            }
        }

        private void doUPXCompression()
        {
            string sUPXFileName = WorkingDir + constUPX;
            string sDownloaderFileName = WorkingDir + DownloaderFileName;

            if (!File.Exists(sUPXFileName))
            {
                // Extract the corresponding Downloader Template from the resource & Save it
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream UPXStream = assembly.GetManifestResourceStream(
                        assembly.GetName().Name + ".Resources." + constUPX))
                {
                    //Create the UPX file.
                    using (FileStream fileUPX = File.Create(sUPXFileName))
                    {
                        readWriteStream(UPXStream, fileUPX);
                    }
                }
            }
            // Shell Execute the command to do the UPX Compression
            Process procUPX = new Process();
            procUPX.EnableRaisingEvents = false;
            procUPX.StartInfo.FileName = sUPXFileName; // UPX executable name
            procUPX.StartInfo.Arguments = "-9 \"" + sDownloaderFileName + "\""; // UPX arguments
            procUPX.StartInfo.UseShellExecute = false;
            procUPX.StartInfo.RedirectStandardOutput = true;
            procUPX.Start();
            string stdOutput = procUPX.StandardOutput.ReadToEnd();
            procUPX.WaitForExit();
            if (procUPX.ExitCode != 0)
                throw new ApplicationException(AppResource.UPXCompressionFailed);
        }

        private void updateMainResource()
        {
            string filenameDownloader = WorkingDir + DownloaderFileName;
            string filenameSatelliteRC = WorkingDir + constDownloaderSatelliteResource;

            ResourceInfo rcInfo = new ResourceInfo();
            rcInfo.Load(filenameDownloader);
            rcInfo.Unload(); // Release the module so its can be saved
            // Begin the batch update to the designated module to speed up the process
            ResourceInfo.BeginBatchUpdate(filenameDownloader);
            try
            {
                // ==========================
                // Modify the Icon
                // ==========================
                // Look up the ICON resource
                if (IconFullPath.Trim() != "")
                {
                    msResIdToFind = 128;
                    IconDirectoryResource icoRC = (IconDirectoryResource)rcInfo[Kernel32.ResourceTypes.RT_GROUP_ICON].Find(FindRes);
                    if (icoRC == null)
                        throw new ApplicationException(AppResource.ResIcon128NotFound);

                    IconFile icoFile = new IconFile(IconFullPath.Trim());
                    uint j = 1;
                    icoRC.Icons.RemoveRange(0, icoRC.Icons.Count);
                    icoRC.SaveTo(filenameDownloader);
                    foreach (IconFileIcon icoFileIcon in icoFile.Icons)
                    {
                        IconResource icoRes = new IconResource(icoFileIcon, new ResourceId(j), 1033);
                        icoRes.Name = new ResourceId(j++);
                        icoRC.Icons.Add(icoRes);
                    }
                    icoRC.SaveTo(filenameDownloader);
                }
                // ==================================
                // Modify the strings in the resource
                // ==================================
                // Downloader GUID
                StringResource stringRC = null;
                msResIdToFind = StringResource.GetBlockId(40401);
                stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                if (stringRC == null)
                    throw new ApplicationException(
                        string.Format(AppResource.ResStringTemplateNotFound, 40401));

                stringRC.Strings[40401] = string.Format(stringRC.Strings[40401], DownloaderGuid);
                stringRC.SaveTo(filenameDownloader);
                // ==========================================================
                // Embed the modified satellite resource into the main resource
                // ==========================================================
                // Look up the embedded satellite resource
                msResIdToFind = 1000;
                GenericResource satelliteRC = (GenericResource)rcInfo.Resources[new ResourceId(1001)].Find(FindRes);
                if (satelliteRC == null) throw new ApplicationException(
                    AppResource.ResEmbededSatelliteDllNotFound);

                // Compresse the satellite RC file.
                MemoryStream memStream = new MemoryStream(1024 * 10);
                FileStream fs = new FileStream(
                    filenameSatelliteRC,
                    FileMode.Open);
                byte[] temp = new byte[fs.Length];
                fs.Read(temp, 0, temp.Length);
                fs.Close();
                using (GZipStream Compress = new GZipStream(memStream, CompressionMode.Compress))
                {
                    // Compress the data into the memory stream
                    Compress.Write(temp, 0, temp.Length);
                    // Read the compressed data from the memory stream
                    temp = new byte[memStream.Length + 1];
                    memStream.Position = 0;
                    // Leave the 1st byte as cheating byte and start to fill the array from the 2nd byte
                    for (long i = 1; i < memStream.Length + 1; i++) temp[i] = Convert.ToByte(memStream.ReadByte());
                }
                memStream.Close();
                satelliteRC.Data = temp;
                satelliteRC.SaveTo(filenameDownloader);
                // Erease the english satellite resource since it has been used to overwrite the main satellite resource!!
                // Or it will remain useless if its choose to generate the chinese downloader!
                msResIdToFind = 1002;
                satelliteRC = (GenericResource)rcInfo.Resources[new ResourceId(1001)].Find(FindRes);
                if (satelliteRC == null) throw new ApplicationException(
                    AppResource.ResEmbededSatelliteDllNotFound);
                // The binary array must contain at least one element to prevent the failure in some cases
                satelliteRC.Data = new byte[1];
                satelliteRC.SaveTo(filenameDownloader);
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
            finally
            {
                // Commit the batch updates to the module
                ResourceInfo.EndBatchUpdate();
            }
        }

        private void updateResource()
        {
            string filenameDownloader = WorkingDir + DownloaderFileName;
            string filenameDownloaderSatelliteResource = WorkingDir + constDownloaderSatelliteResource;

            // Look up the satellite resource
            ResourceInfo rcInfo = new ResourceInfo();
            rcInfo.Load(filenameDownloader);
            rcInfo.Unload(); // Release the module so its can be saved

            // Chinese Satellite Resource
            msResIdToFind = 1000;
            if (AppResource.Culture.Name == "en-US")
            {
                // English Satellite Resource
                msResIdToFind = 1002;
            }

            GenericResource satelliteRC = (GenericResource)rcInfo.Resources[new ResourceId(1001)].Find(FindRes);
            byte[] temp = satelliteRC.WriteAndGetBytes();
            MemoryStream memSatellite = new MemoryStream(1024 * 10);
            memSatellite.Write(temp, 0, temp.Length);
            memSatellite.Position = 0;
            //Create the decompressed file.
            using (FileStream fileSatellite = File.Create(filenameDownloaderSatelliteResource))
            {
                using (GZipStream Decompress = new GZipStream(memSatellite, CompressionMode.Decompress))
                {
                    readWriteStream(Decompress, fileSatellite);
                }
            }
            // Load the satellite resource
            rcInfo = new ResourceInfo();
            rcInfo.Load(filenameDownloaderSatelliteResource);
            rcInfo.Unload(); // Release the module so its can be saved
            // Begin the batch update to the designated module to speed up the process
            ResourceInfo.BeginBatchUpdate(filenameDownloaderSatelliteResource);
            try
            {
                // ==========================
                // Modify the Banner
                // ==========================
                // Look up the bitmap resource
                if (BannerBitmapFullPath.Trim() != "")
                {
                    msResIdToFind = 207;
                    BitmapResource bmpRC = (BitmapResource)rcInfo[Kernel32.ResourceTypes.RT_BITMAP].Find(FindRes);
                    if (bmpRC == null)
                        throw new ApplicationException(
                            AppResource.ResBitmap207NotFound);

                    BitmapFile bmpFile = new BitmapFile(BannerBitmapFullPath.Trim());
                    bmpRC.Bitmap = bmpFile.Bitmap;
                    bmpRC.SaveTo(filenameDownloaderSatelliteResource);
                }
                // ==========================
                // Modify the Icon
                // ==========================
                // Look up the ICON resource
                if (IconFullPath.Trim() != "")
                {
                    msResIdToFind = 128;
                    IconDirectoryResource icoRC = (IconDirectoryResource)rcInfo[Kernel32.ResourceTypes.RT_GROUP_ICON].Find(FindRes);
                    if (icoRC == null)
                        throw new ApplicationException(AppResource.ResIcon128NotFound);

                    IconFile icoFile = new IconFile(IconFullPath.Trim());
                    uint j = 1;
                    icoRC.Icons.RemoveRange(0, icoRC.Icons.Count);
                    icoRC.SaveTo(filenameDownloaderSatelliteResource);
                    foreach (IconFileIcon icoFileIcon in icoFile.Icons)
                    {
                        IconResource icoRes = new IconResource(icoFileIcon, new ResourceId(j), 1033);
                        icoRes.Name = new ResourceId(j++);
                        icoRC.Icons.Add(icoRes);
                    }
                    icoRC.SaveTo(filenameDownloaderSatelliteResource);
                }
                // ==========================
                // Modify the main dialog box
                // ==========================
                // Look up the dialog resource
                msResIdToFind = 201;
                DialogResource dlgRC = (DialogResource)rcInfo[Kernel32.ResourceTypes.RT_DIALOG].Find(FindRes);
                if (dlgRC == null)
                    throw new ApplicationException(AppResource.ResDialog201NotFound);

                // Find the designated label control
                msCtrlIdToFind = 1010;
                DialogTemplateControlBase ctrl = dlgRC.Template.Controls.Find(FindDlgControl);
                ctrl.CaptionId.Name = string.Format(ctrl.CaptionId.Name, DownloaderDisplayName);
                // Find the designated link control
                msCtrlIdToFind = 1006;
                ctrl = dlgRC.Template.Controls.Find(FindDlgControl);
                ctrl.CaptionId.Name = string.Format(ctrl.CaptionId.Name, DownloaderHomeUrl);
                dlgRC.SaveTo(filenameDownloaderSatelliteResource);
                // ===================================================
                // Embed the specified .Torrent file into the resource
                // ===================================================
                // Look up the torrent resource
                msResIdToFind = 1021;
                GenericResource torrentRC = (GenericResource)rcInfo.Resources[new ResourceId(1022)].Find(FindRes);
                if (torrentRC == null)
                    throw new ApplicationException(AppResource.ResTorrentSlot2011NotFound);

                FileStream fs = new FileStream(MetafileFullPath, FileMode.Open);
                temp = new byte[fs.Length];
                fs.Read(temp, 0, temp.Length);
                fs.Close();
                torrentRC.Data = temp;
                torrentRC.SaveTo(filenameDownloaderSatelliteResource);
                // ===================================================
                // Embed the specified disclaimer file into the resource
                // ===================================================
                // Look up the disclaimer resource
                if (DisclaimerFullPath.Trim() != "")
                {
                    msResIdToFind = 40111;
                    GenericResource disclaimerRC = (GenericResource)rcInfo.Resources[new ResourceId(40112)].Find(FindRes);
                    if (disclaimerRC == null)
                        throw new ApplicationException(AppResource.ResDisclaimerSlot40112NotFound);

                    fs = new FileStream(DisclaimerFullPath.Trim(), FileMode.Open);
                    temp = new byte[fs.Length];
                    fs.Read(temp, 0, temp.Length);
                    fs.Close();
                    disclaimerRC.Data = temp;
                    disclaimerRC.SaveTo(filenameDownloaderSatelliteResource);
                }
                // ==================================
                // Modify the strings in the resource
                // ==================================
                // Display Name
                StringResource stringRC = null;
                int[] stringID = new int[] { 1112, 13015, 13016, 13017, 13018, 13019, 13027 };
                for (int i = 0; i < stringID.Length; i++)
                {
                    int sID = stringID[i];
                    // Check if the string resource has been loaded in the last string block or not.
                    if (stringRC == null || !stringRC.Strings.ContainsKey((ushort)sID))
                    {
                        msResIdToFind = StringResource.GetBlockId(sID);
                        stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                        if (stringRC == null)
                            throw new ApplicationException(
                                string.Format(AppResource.ResStringTemplateNotFound, sID));
                    }
                    stringRC.Strings[(ushort)sID] = string.Format(stringRC.Strings[(ushort)sID], DownloaderDisplayName);
                    // Leave the modified string resource unsaved until all the strings in the string block are done.
                    if (stringID.Length == (i + 1) || !stringRC.Strings.ContainsKey((ushort)stringID[i + 1]))
                    {
                        stringRC.SaveTo(filenameDownloaderSatelliteResource);
                    }
                }
                // Google Analytics Profile ID
                msResIdToFind = StringResource.GetBlockId(1113);
                stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                if (stringRC == null)
                    throw new ApplicationException(
                        string.Format(AppResource.ResStringTemplateNotFound, 1113));

                stringRC.Strings[1113] = string.Format(stringRC.Strings[1113], GoogleAnalyticsProfileID);
                stringRC.SaveTo(filenameDownloaderSatelliteResource);
                // Downloader GUID
                msResIdToFind = StringResource.GetBlockId(40401);
                stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                if (stringRC == null)
                    throw new ApplicationException(
                        string.Format(AppResource.ResStringTemplateNotFound, 40401));

                stringRC.Strings[40401] = string.Format(stringRC.Strings[40401], DownloaderGuid);
                stringRC.SaveTo(filenameDownloaderSatelliteResource);
                // Online FAQ URL
                if (OnlineFaqUrl.Trim() != "")
                {
                    msResIdToFind = StringResource.GetBlockId(13020);
                    stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                    if (stringRC == null)
                        throw new ApplicationException(
                            string.Format(AppResource.ResStringTemplateNotFound, 13020));

                    stringRC.Strings[13020] = OnlineFaqUrl;
                    stringRC.SaveTo(filenameDownloaderSatelliteResource);
                }
                // Event ID
                if (PromotionEventID.Trim() != "")
                {
                    msResIdToFind = StringResource.GetBlockId(1104);
                    stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                    if (stringRC == null)
                        throw new ApplicationException(
                            string.Format(AppResource.ResStringTemplateNotFound, 1104));

                    stringRC.Strings[1104] = string.Format(stringRC.Strings[1104], PromotionEventID);
                    stringRC.SaveTo(filenameDownloaderSatelliteResource);
                }
                // Event Server URL
                if (PromotionEventID.Trim() != "")
                {
                    msResIdToFind = StringResource.GetBlockId(1101);
                    stringRC = (StringResource)rcInfo[Kernel32.ResourceTypes.RT_STRING].Find(FindRes);
                    if (stringRC == null)
                        throw new ApplicationException(
                            string.Format(AppResource.ResStringTemplateNotFound, 1101));

                    // Conver the URL to base64 encoded string
                    stringRC.Strings[1101] = Convert.ToBase64String(Encoding.ASCII.GetBytes(PromotionEventServerUrl));
                    stringRC.SaveTo(filenameDownloaderSatelliteResource);
                }
            }
            catch (Exception oEx)
            {
                throw oEx;
            }
            finally
            {
                // Commit the batch updates to the module
                ResourceInfo.EndBatchUpdate();
            }
        }

        private bool validateData()
        {
            if (!Directory.Exists(WorkingDir))
            {
                throw new ApplicationException(
                    string.Format(AppResource.DirectoryNotExist, WorkingDir));
            }
            if (DownloaderGuid.Trim() == "")
            {
                throw new ApplicationException(AppResource.InvalidDownloaderGuid);
            }
            if (DownloaderDisplayName.Trim() == "")
            {
                throw new ApplicationException(AppResource.InvalidDownloaderDisplayName);
            }
            if (DownloaderHomeUrl.Trim() == "")
            {
                throw new ApplicationException(AppResource.InvalidDownloaderHomeUrl);
            }
            if (!File.Exists(MetafileFullPath))
            {
                throw new ApplicationException(AppResource.InvalidMetafileFullPath);
            }
            if (DisclaimerFullPath.Trim() != "" && !File.Exists(DisclaimerFullPath))
            {
                throw new ApplicationException(AppResource.InvalidDisclaimerFullPath);
            }
            if (GoogleAnalyticsProfileID.Trim() == "")
            {
                throw new ApplicationException(AppResource.InvalidGoogleAnalyticsProfileID);
            }
            if (PromotionEventID.Trim() != "" && PromotionEventID.Trim().Length != 36)
            {
                throw new ApplicationException(AppResource.InvalidPromotionEventID);
            }
            if (PromotionEventID.Trim() != "" && PromotionEventServerUrl.Trim() == "")
            {
                throw new ApplicationException(AppResource.InvalidPromotionEventServerUrl);
            }
            return true;
        }

        public void Generate()
        {
            if (validateData())
            {
                string filenameDownloaderFileName = WorkingDir + DownloaderFileName;
                // Extract the corresponding Downloader Template from the resource & Save it
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream DownloaderStream = assembly.GetManifestResourceStream(
                        assembly.GetName().Name + ".Resources." +
                        (PromotionEventID.Trim() == "" ? constGLoaderResName : constGLoaderPromoResName)
                        ))
                {
                    //Create the decompressed file.
                    using (FileStream fileDownloader = File.Create(filenameDownloaderFileName))
                    {
                        using (GZipStream Decompress = new GZipStream(DownloaderStream, CompressionMode.Decompress))
                        {
                            readWriteStream(Decompress, fileDownloader);
                        }
                    }
                }
                try
                {
                    // Update the Downloader Template accordingly
                    updateResource();
                    updateMainResource();
                    if (UPXCompression) doUPXCompression();
                }
                catch (Exception oEx)
                {
                    // Wait for the file to be released
                    Thread.Sleep(1000);
                    // Clear up the generating downloader if failed
                    if (File.Exists(filenameDownloaderFileName))
                    {
                        File.Delete(filenameDownloaderFileName);
                    }
                    throw oEx;
                }
                finally
                {
                    // Wait for the file to be released
                    Thread.Sleep(1000);
                    // Clear up the satellite RC file if any
                    if(File.Exists(WorkingDir + constDownloaderSatelliteResource))
                    {
                        File.Delete(WorkingDir + constDownloaderSatelliteResource);
                    }
                    // Clear up the UPX exe file if any
                    if (File.Exists(WorkingDir + constUPX))
                    {
                        File.Delete(WorkingDir + constUPX);
                    }
                }
            }
        }

        #region IManagementTask Members

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public TaskMethod Method
        {
            get;
            private set;
        }

        public object Result
        {
            get;
            set;
        }

        #endregion
    }
}
