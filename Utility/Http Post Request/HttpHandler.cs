using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Net
{
    public class HttpHandler
    {
        public event EventHandler<GenericEventArgs<HttpWebResponse>> RequestCompleted;

        public HttpWebResponse MultiPartPost(string userName, string password, string uri, Dictionary<string, object> parameters)
        {
            MultiPartPostRequestBuilder builder = new MultiPartPostRequestBuilder();
            var request = builder.BuildMultiPartRequest(userName, password, uri, parameters);
            //Wait for request to return before exiting
            return ProcessRequest(request);
        }

        public HttpWebResponse Post(string userName, string password, string uri, Dictionary<string, string> parameters)
        {
            PostRequestBuilder builder = new PostRequestBuilder();
            var request = builder.BuildRequest(userName, password, uri, parameters);
            //Wait for request to return before exiting
            return ProcessRequest(request);
        }

        public void PostAsync(string userName, string password, string uri, Dictionary<string, string> parameters)
        {
            PostRequestBuilder builder = new PostRequestBuilder();
            var request = builder.BuildRequest(userName, password, uri, parameters);
            //Don't bother waiting - if it is there, it is there
            Task.Factory.StartNew(() => ProcessRequest(request));
        }

        public HttpWebResponse Get(string userName, string password, string uri)
        {
            RequestBuilder builder = new RequestBuilder();
            var request = builder.BuildRequest(userName, password, uri);
            //Wait for request to return before exiting
            return ProcessRequest(request);
        }

        public HttpWebResponse Get(string userName, string password, string uri, Dictionary<string, string> parameters)
        {
            RequestBuilder builder = new RequestBuilder();
            var request = builder.BuildRequest(userName, password, uri, parameters);
            //Wait for request to return before exiting
            return ProcessRequest(request);
        }

        public void GetAsync(string userName, string password, string uri)
        {
            RequestBuilder builder = new RequestBuilder();
            var request = builder.BuildRequest(userName, password, uri);
            //Don't bother waiting - if it is there, it is there
            Task.Factory.StartNew(() => ProcessRequest(request));
        }

        public void GetAsync(string userName, string password, string uri, Dictionary<string, string> parameters)
        {
            RequestBuilder builder = new RequestBuilder();
            var request = builder.BuildRequest(userName, password, uri, parameters);
            //Don't bother waiting - if it is there, it is there
            Task.Factory.StartNew(() => ProcessRequest(request));
        }

        private HttpWebResponse ProcessRequest(HttpWebRequest request)
        {
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                OnRequestCompleted(response);
                return response;
            }
            catch (Exception ex)
            {
                var exception = new ApplicationException(String.Format("Failed to process the request to {0}", request.RequestUri), ex);
                throw exception;
            }
        }

        protected void OnRequestCompleted(HttpWebResponse response)
        {
            if (null != RequestCompleted)
            {
                RequestCompleted(this, new GenericEventArgs<HttpWebResponse>(response));
            }
        }
    }
}
