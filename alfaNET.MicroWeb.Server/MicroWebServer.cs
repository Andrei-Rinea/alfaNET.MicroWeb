using System;
using System.Collections;
using System.Net;
using System.Threading;
using alfaNET.MicroWeb.Abstractions;

namespace alfaNET.MicroWeb.Server
{
    public class MicroWebServer
    {
        private readonly int _queueLength;
        private readonly Thread _listenThread;
        private readonly Thread _processThread;
        private readonly HttpListener _listener;
        private readonly Queue _requestQueue;
        private readonly IHttpListenerHandler _handler;

        public MicroWebServer(MicroWebSite microWebSite, int queueLength)
        {
            if (microWebSite == null) throw new ArgumentNullException("microWebSite");
            if (queueLength < 8 || queueLength > 64) throw new ArgumentOutOfRangeException("queueLength");

            _queueLength = queueLength;
            _handler = microWebSite.Handler;

            _requestQueue = new Queue();
            _listenThread = new Thread(Listen);
            _processThread = new Thread(Process);
            var protocolPrefix = microWebSite.Https ? "https" : "http";
            _listener = new HttpListener(protocolPrefix, microWebSite.Port);
        }

        private void Listen()
        {
            _listener.Start();
            while (true)
            {
                var context = _listener.GetContext();
                if (_requestQueue.Count >= _queueLength)
                {
                    try
                    {
                        context.Response.StatusCode = 503;
                        context.Response.StatusDescription = "Server busy";
                        context.Response.Close();
                    }
                    // ReSharper disable once EmptyGeneralCatchClause
                    catch { }
                    finally
                    {
                        DroppedRequests++;
                    }
                    continue;
                }
                _requestQueue.Enqueue(context);
            }
        }

        private void Process()
        {
            while (true)
            {
                Thread.Sleep(0);
                if (_requestQueue.Count == 0)
                    continue;
                var item = _requestQueue.Dequeue();
                var context = (HttpListenerContext)item;
                try
                {
                    _handler.Handle(context);
                    context.Response.Close();
                    HandlerSuccessRequests++;
                }
                catch
                {
                    HandlerFailedRequests++;
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public MicroWebServerStatus Status { get; private set; }

        public int DroppedRequests { get; private set; }
        public int HandlerFailedRequests { get; private set; }
        public int HandlerSuccessRequests { get; private set; }

        public void Start()
        {
            if (Status != MicroWebServerStatus.NotStarted)
                throw new InvalidOperationException();

            Status = MicroWebServerStatus.Running;

            _processThread.Start();
            _listenThread.Start();
        }

        public void Stop()
        {
            if (Status != MicroWebServerStatus.Running)
                throw new InvalidOperationException();

            Status = MicroWebServerStatus.Stopped;

            _listener.Stop();
            _listenThread.Abort();
            _processThread.Abort();
        }
    }
}