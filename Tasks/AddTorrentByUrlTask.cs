using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public class AddTorrentByUrlTask : IManagementTask
    {
        public AddTorrentByUrlTask(string torrentFileUrl)
        {
            TorrentFileUrl = torrentFileUrl;
            Method = TaskMethod.AddTorrentByUrl;
        }

        public string TorrentFileUrl
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
