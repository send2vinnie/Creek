using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public class PauseTorrentTask : IManagementTask
    {
        public PauseTorrentTask(string hash)
        {
            TorrentHash = hash;
            Method = TaskMethod.PauseTorrent;
        }

        public string TorrentHash
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
