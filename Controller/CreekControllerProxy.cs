using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Creek.Jobs;

namespace Creek.Controller
{
    public class CreekControllerProxy
    {
        private string controllerHost;
        private IRemotableCreekController remoteController;

        public CreekControllerProxy(string sControllerHost)
        {
            controllerHost = sControllerHost;
        }

        private void InitProxy()
        {
            try
            {
                if (remoteController == null)
                {
                    remoteController = (IRemotableCreekController)Activator.GetObject(
                        typeof(IRemotableCreekController),
                        controllerHost);
                }
            }
            catch (Exception e)
            {
                throw new ApplicationException(string.Format("Could not get handle to remote controller: {0}", e.Message), e);
            }
        }

        private IRemotableCreekController RemoteController
        {
            get
            {
                InitProxy();
                return remoteController;
            }
        }

        public static IRemotableCreekController ProxyFactory(string sControllerHost)
        {
            CreekControllerProxy oProxy = new CreekControllerProxy(sControllerHost);
            return oProxy.RemoteController;
        }

    }
}
