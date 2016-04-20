using System.Collections.Generic;

namespace System.Net
{
    public class RequestBuilder
    {
        public HttpWebRequest BuildRequest(string userName, string password, string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Credentials = new NetworkCredential(userName, password);
            request.Timeout = 1000 * 5; // 5 seconds - get response timeout
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; .NET4.0)";
            request.KeepAlive = false;
            request.ServicePoint.ConnectionLeaseTimeout = 1000;
            request.ServicePoint.ConnectionLimit = 10;
            request.ServicePoint.BindIPEndPointDelegate = new BindIPEndPoint(BindIPEndPointCallback);
            return request;
        }

        protected static int m_LastBindPortUsed = 5001;
        protected static IPEndPoint BindIPEndPointCallback(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
        {
            int port = Threading.Interlocked.Increment(ref m_LastBindPortUsed);
            Threading.Interlocked.CompareExchange(ref m_LastBindPortUsed, 5001, 65534);
            if (remoteEndPoint.AddressFamily == Sockets.AddressFamily.InterNetwork)
            {
                return new IPEndPoint(IPAddress.Any, port);
            }
            else
            {
                return new IPEndPoint(IPAddress.IPv6Any, port);
            }
        }

        public virtual HttpWebRequest BuildRequest(string userName, string password, string uri, Dictionary<string, string> parameters)
        {
            string query = CreateRequestString(parameters);
            string url = String.Format("{0}?{1}", uri, query);
            return BuildRequest(userName, password, url);
        }

        protected string CreateRequestString(Dictionary<string, string> parameters)
        {
            return parameters.Join("&", "=");
        }
    }
}
