using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    class AddTorrentByMagnetUrlTask : IManagementTask
    {
        public AddTorrentByMagnetUrlTask(string torrentMagnetUrl)
        {
            TorrentMagnetUrl = torrentMagnetUrl;
            Method = TaskMethod.AddTorrentByMagnetUrl;
        }

        public string TorrentMagnetUrl
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
