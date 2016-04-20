using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public class AddTorrentByFileTask : IManagementTask
    {
        public AddTorrentByFileTask(string torrentFilePath)
        {
            TorrentFilePath = torrentFilePath;
            Method = TaskMethod.AddTorrentByFile;
        }

        public string TorrentFilePath
        {
            get;
            private set;
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
