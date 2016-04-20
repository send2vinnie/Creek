using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Security;
using Common.Logging;
using Creek.Jobs;


namespace Creek.Controller
{
    /// <summary>
    /// Controller exporter that exports controller to remoting context.
    /// </summary>
    /// <author>Vinnie Hsu</author>
    public class RemotingControllerExporter
    {
        private readonly ILog log;

        public RemotingControllerExporter()
        {
            log = LogManager.GetLogger(GetType());
        }

        public virtual void Bind(IRemotableCreekController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }
            if (!typeof(MarshalByRefObject).IsAssignableFrom(controller.GetType()))
            {
                throw new ArgumentException("Exported controller must be of type MarshallByRefObject", "controller");
            }

            try
            {
                // Expose the object directly by leveraging the already registered channels done by Quartz Scheduler
                RemotingServices.Marshal((MarshalByRefObject) controller, controller.GetType().Name);
                log.Info(string.Format(CultureInfo.InvariantCulture, "Successfully marhalled remotable controller under name '{0}'", controller.GetType().Name));
            }
            catch (RemotingException ex)
            {
                log.Error("RemotingException during Bind", ex);
            }
            catch (SecurityException ex)
            {
                log.Error("SecurityException during Bind", ex);
            }
            catch (Exception ex)
            {
                log.Error("Exception during Bind", ex);
            } 
        }

        public virtual void UnBind(IRemotableCreekController controller)
        {
            if (controller == null)
            {
                throw new ArgumentNullException("controller");
            }
            if (!typeof(MarshalByRefObject).IsAssignableFrom(controller.GetType()))
            {
                throw new ArgumentException("Exported controller must be of type MarshallByRefObject", "controller");
            } 
            
            try
            {
                RemotingServices.Disconnect((MarshalByRefObject) controller);
                log.Info("Successfully disconnected remotable controller");
            }
            catch (ArgumentException ex)
            {
                log.Error("ArgumentException during Unbind", ex);
            }
            catch (SecurityException ex)
            {
                log.Error("SecurityException during Unbind", ex);
            }
            catch (Exception ex)
            {
                log.Error("Exception during Unbind", ex);
            } 
        }
    }
}
