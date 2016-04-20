using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public class ResumeTorrentTask : IManagementTask
    {
        public ResumeTorrentTask(string hash)
        {
            TorrentHash = hash;
            Method = TaskMethod.ResumeTorrent;
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
