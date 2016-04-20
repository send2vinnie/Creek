using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Creek.Tasks
{
    public class PauseAllTorrentsTask : IManagementTask
    {
        public PauseAllTorrentsTask()
        {
            Method = TaskMethod.PauseAllTorrents;
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
