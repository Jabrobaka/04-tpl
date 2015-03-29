using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HashServer;
using log4net;

namespace Balancer
{
    class Balancer
    {
        public readonly string[] Servers;

        private readonly Random random;
        private readonly Listener listener;
        private const string RequestPattern = @"http://{0}/method{1}";
        private const int ListenerPort = 22822;
        private ILog log = LogManager.GetLogger(typeof (Balancer));

        public Balancer(string[] balancerSettings)
        {
            Servers = balancerSettings;
            random = new Random();
            listener = new Listener(ListenerPort, "/method", OnContextAsync);
        }

        public void Start()
        {
            listener.Start();
        }

        private async Task OnContextAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var query = request.Url.Query;

            var serverResponse = await RequestReplicas(query);
            Response(serverResponse, context);
        }

        private static  void Response(string result, HttpListenerContext context)
        {
            var encoding = context.Request.Headers["Accept-Encoding"];
            using (var response = context.Response)
            {
                if (result == null)
                {
                    response.StatusCode = 500;
                }
                else
                {
                    var responseStream = context.Response.OutputStream;
                    var sendable = Encoding.UTF8.GetBytes(result);
                    if (encoding.Contains("deflate"))
                    {
                        responseStream = new DeflateStream(responseStream, CompressionLevel.Fastest);
                        response.Headers.Remove("Content-Encoding");
                        response.AppendHeader("Content-Encoding", "deflate");
                    }

                    responseStream.Write(sendable, 0, sendable.Length);
                    responseStream.Dispose();
                }
            }
        }

        private async Task<string> RequestReplicas(string query)
        {
            var usedReplicas = new List<int>(Servers.Length);
            while (true)
            {
                var rnd = GetRandomReplicaNumber();
                if (!usedReplicas.Contains(rnd))
                {
                    usedReplicas.Add(rnd);
                    var result = await RequestServer(Servers[rnd], query);
                    if (result != null)
                        return result;
                }
                if (usedReplicas.Count == Servers.Length)
                {
                    return null;
                }
            }

        }

        private int GetRandomReplicaNumber()
        {
            lock (random)
            {
                return random.Next(Servers.Length);
            }
        }

        private async Task<string> RequestServer(string server, string query)
        {
            var request = (HttpWebRequest) WebRequest.Create(string.Format(RequestPattern, server, query));
            var response = Task.Run(() => TryGetResponse(request));
            var timeout = Task.Delay(3000);

            var ended = await Task.WhenAny(response, timeout);

            if (ended != response)
            {
                return null;
            }
            return response.Result;
        }

        private static async Task<string> TryGetResponse(WebRequest request)
        {
            try
            {
                using (var response = await request.GetResponseAsync())
                using (var respStream = new StreamReader(response.GetResponseStream()))
                {
                    return respStream.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                return null;
            }
        }
    }
}
