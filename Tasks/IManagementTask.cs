using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public enum TaskMethod
    {
        RetrieveTorrentDetails = 0,
        AddTorrentByUrl = 1,
        AddTorrentByMagnetUrl = 2,
        AddTorrentByFile = 3,
        RemoveTorrent = 4,
        PauseTorrent = 5,
        PauseAllTorrents = 6,
        ResumeTorrent = 7,
        ResumeAllTorrents = 8,
        StopTorrent = 9,
        StopAllTorrents = 10,
        StartTorrent = 11,
        StartAllTorrents = 12,
        GetTorrentFileList = 13,
        SetTorrentFilePriorities = 14,
        SetTorrentTransferRates = 15,
        SetTorrentLabel = 16,
        SetTorrentDownloadLocation = 17,
        GetTorrentDetails = 18,
        SetTorrentTrackers = 19,
        SetTorrentAlternativeMode = 20,
        CreateDownloader = 21,
        CreateTorrent = 22
    }

    public interface IManagementTask
    {
        void Execute();
        TaskMethod Method { get; }
        object Result { get; set; }
    }
}
