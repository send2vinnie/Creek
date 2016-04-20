using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public class RetrieveTorrentDetailsTask : IManagementTask
    {
        public RetrieveTorrentDetailsTask()
        {
            Method = TaskMethod.RetrieveTorrentDetails;
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
