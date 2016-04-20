using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using MonoTorrent;
using MonoTorrent.Common;
using MonoTorrent.Client.PieceWriters;

namespace MonoTorrent.Client
{
    class HttpRangeReader : PieceWriter
    {
        public HttpRangeReader()
        {
        }

        public override void Close(TorrentFile file)
        {
        }

        public override void Dispose()
        {
        }

        //
        // Summary:
        //     When overridden in a derived class, reads a sequence of bytes from the current
        //     stream and advances the position within the stream by the number of bytes
        //     read.
        //
        // Parameters:
        //   offset:
        //     The zero-based byte offset in buffer at which to begin storing the data read
        //     from the current stream.
        //
        //   buffer:
        //     An array of bytes. When this method returns, the buffer contains the specified
        //     byte array with the values between offset and (offset + count - 1) replaced
        //     by the bytes read from the current source.
        //
        //   count:
        //     The maximum number of bytes to be read from the current stream.
        //
        // Returns:
        //     The total number of bytes read into the buffer. This can be less than the
        //     number of bytes requested if that many bytes are not currently available,
        //     or zero (0) if the end of the stream has been reached.
        public override int Read(TorrentFile file, long offset, byte[] buffer, int bufferOffset, int count)
        {
            Check.File(file);
            Check.Buffer(buffer);

            if (offset < 0 || offset + count > file.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (bufferOffset < 0 || bufferOffset + count > buffer.Length)
                throw new ArgumentOutOfRangeException("bufferOffset");

            int nBytes = 0;
            int bufOff = bufferOffset;

            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(file.FullPath);
            webReq.AddRange(offset, offset + count - 1);
            webReq.Timeout = 1000 * 60 * 5; // 5 minutes

            using (HttpWebResponse webResp = (HttpWebResponse)webReq.GetResponse())
            {
                if ("bytes" != webResp.Headers[HttpResponseHeader.AcceptRanges].ToLower())
                {
                    throw new System.Net.WebException(
                        string.Format("The content hosted on the web ({0}) doesn't support range request", file.FullPath),
                        WebExceptionStatus.RequestCanceled);
                }
                using (Stream streamResp = webResp.GetResponseStream())
                {
                    BinaryReader contentReader = new BinaryReader(streamResp);
                    do
                    {
                        nBytes = contentReader.Read(buffer, bufOff, count - (bufOff - bufferOffset));
                        bufOff += nBytes;
                        Thread.Sleep(1);
                    } while (nBytes > 0);
                }
            }

            //if ((bufOff - bufferOffset) != count)
            //{
            //    throw new System.Net.WebException(
            //        string.Format("Failed to read the specified length of data from {0}", file.FullPath),
            //        WebExceptionStatus.ReceiveFailure);
            //}

            return (bufOff - bufferOffset);
        }

        public override void Move(string oldPath, string newPath, bool ignoreExisting)
        {
            throw new NotImplementedException();
        }

        public override void Write(TorrentFile file, long offset, byte[] buffer, int bufferOffset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool Exists(TorrentFile file)
        {
            throw new NotImplementedException();
        }

        public override void Flush(TorrentFile file)
        {
            throw new NotImplementedException();
        }
    }
}
