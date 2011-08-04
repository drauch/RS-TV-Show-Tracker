﻿namespace RoliSoft.TVShowTracker
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Text.RegularExpressions;

    using Starksoft.Net.Proxy;

    /// <summary>
    /// Provides an HTTP proxy and forwards the incoming requests to an external SOCKSv5 proxy.
    /// </summary>
    public class HttpToSocks
    {
        /// <summary>
        /// A regular expression to find the URL in the HTTP request.
        /// </summary>
        public static Regex URLRegex = new Regex(@"https?://(?<host>[^/:]+)(?:\:(?<port>[0-9]+))?(?<path>/[^\s$]+)");

        /// <summary>
        /// Gets the local HTTP proxy.
        /// </summary>
        public WebProxy LocalProxy
        {
            get
            {
                return new WebProxy("127.0.0.1:" + ((IPEndPoint)_server.LocalEndpoint).Port);
            }
        }

        /// <summary>
        /// Gets or sets the remote SOCKS proxy.
        /// </summary>
        /// <value>
        /// The remote SOCKS proxy.
        /// </value>
        public string RemoteProxy { get; set; }

        private TcpListener _server;
        
        /// <summary>
        /// Starts listening for incoming connections at 127.0.0.1 on a random port.
        /// </summary>
        public void Listen()
        {
            _server = new TcpListener(IPAddress.Loopback, 0);
            _server.Start();
            _server.BeginAcceptSocket(AcceptClient, null);
        }

        /// <summary>
        /// Accepts the incoming request, processes it, and shuts down the whole server.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        private void AcceptClient(IAsyncResult asyncResult)
        {
            try
            {
                using (var client = _server.EndAcceptTcpClient(asyncResult))
                {
                    client.ReceiveTimeout = 250;

                    using (var stream = client.GetStream())
                    {
                        ProcessHTTPRequest(stream);

                        stream.Close();
                        client.Close();
                    }
                }
            }
            finally
            {
                if (_server != null)
                {
                    try { _server.Stop(); } catch { }
                    _server = null;
                }
            }
        }

        /// <summary>
        /// Reads and processes the HTTP request from the stream.
        /// </summary>
        /// <param name="requestStream">The request stream.</param>
        private void ProcessHTTPRequest(NetworkStream requestStream)
        {
            var request = new string[3];
            var headers = new StringBuilder();
            var postData = string.Empty;
            var host = string.Empty;
            var path = string.Empty;
            var port = 80;

            using (var ms = new MemoryStream())
            {
                CopyStreamToStream(requestStream, ms);

                ms.Position = 0;

                using (var sr = new StreamReader(ms, Encoding.UTF8, false))
                {
                    // [0]GET [1]/index.php [2]HTTP/1.1
                    request = (sr.ReadLine() ?? string.Empty).Split(' ');

                    if (request[0] == "CONNECT")
                    {
                        throw new Exception();
                    }

                    var m = URLRegex.Match(request[1]);

                    host = m.Groups["host"].Value;
                    path = m.Groups["path"].Value;

                    if (m.Groups["port"].Success)
                    {
                        port = m.Groups["port"].Value.ToInteger();
                    }

                    // read headers

                    while (true)
                    {
                        var header = (sr.ReadLine() ?? string.Empty).Trim();
                        
                        if (string.IsNullOrWhiteSpace(header))
                        {
                            break;
                        }

                        if (header.StartsWith("Connection:") || header.StartsWith("Proxy-Connection:"))
                        {
                            continue;
                        }

                        headers.AppendLine(header);
                    }

                    headers.AppendLine("Connection: close");

                    // read post data

                    if (request[0] == "POST")
                    {
                        postData = sr.ReadToEnd();
                    }
                }
            }

            var proxy = RemoteProxy.Split(':');
            var finalRequest = new StringBuilder();

            finalRequest.AppendLine(request[0] + " " + path + " " + request[2]);
            finalRequest.Append(headers);
            finalRequest.AppendLine();

            if (postData != string.Empty)
            {
                finalRequest.Append(postData);
            }

            TunnelRequest(Encoding.UTF8.GetBytes(finalRequest.ToString()), requestStream, host, port, proxy[0], proxy[1].ToInteger());
        }

        /// <summary>
        /// Opens a connection through the proxy and forwards the received request.
        /// </summary>
        /// <param name="requestData">The HTTP proxy's request data.</param>
        /// <param name="responseStream">The stream to write the SOCKS proxy's response to.</param>
        /// <param name="destHost">The destination host.</param>
        /// <param name="destPort">The destination port.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        private void TunnelRequest(byte[] requestData, NetworkStream responseStream, string destHost, int destPort, string proxyHost, int proxyPort)
        {
            var proxy = new Socks5ProxyClient(proxyHost, proxyPort);

            using (var client = proxy.CreateConnection(destHost, destPort))
            using (var stream = client.GetStream())
            {
                stream.Write(requestData, 0, requestData.Length);
                stream.Flush();

                CopyStreamToStream(stream, responseStream);
            }
        }

        /// <summary>
        /// Copies the first stream's content from the current position to the second stream.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destionation">The destionation stream.</param>
        /// <param name="bufferLength">Length of the buffer.</param>
        private void CopyStreamToStream(NetworkStream source, Stream destionation, int bufferLength = 4096)
        {
            var buffer = new byte[bufferLength];

            do
            {
                int i;

                try
                {
                    i = source.Read(buffer, 0, bufferLength);
                }
                catch
                {
                    break;
                }

                if (i == 0)
                {
                    break;
                }

                destionation.Write(buffer, 0, i);
            }
            while (source.DataAvailable);
        }
    }
}
