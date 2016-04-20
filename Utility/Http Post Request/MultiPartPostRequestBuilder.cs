using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Net
{
    class MultiPartPostRequestBuilder : RequestBuilder
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        public HttpWebRequest BuildMultiPartRequest(string userName, string password, string uri, Dictionary<string, object> parameters)
        {
            var request = BuildRequest(userName, password, uri);
            string formDataBoundary = "CK28947758029299";
            byte[] formData = GetMultipartFormData(parameters, formDataBoundary);
            // Set up the request properties
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + formDataBoundary;
            request.CookieContainer = new CookieContainer();
            request.ContentLength = formData.Length;  // We need to count how many bytes we're sending. 
            using (Stream requestStream = request.GetRequestStream())
            {
                // Push it out there
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            return request;
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();

            foreach (var param in postParameters)
            {
                if (param.Value is MultiPartFileParameter)
                {
                    MultiPartFileParameter fileToUpload = (MultiPartFileParameter)param.Value;
                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");
                    formDataStream.Write(encoding.GetBytes(header), 0, header.Length);
                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    // Thanks to feedback from commenters, add a CRLF to allow multiple files to be uploaded
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, 2);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, postData.Length);
                }
            }
            // Add the end of the request
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, footer.Length);
            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();
            return formData;
        }

        public class MultiPartFileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public MultiPartFileParameter(byte[] file) : this(file, null) { }
            public MultiPartFileParameter(byte[] file, string filename) : this(file, filename, null) { }
            public MultiPartFileParameter(byte[] file, string filename, string contenttype)
            {
                File = file;
                FileName = filename;
                ContentType = contenttype;
            }
        }
    }
}
