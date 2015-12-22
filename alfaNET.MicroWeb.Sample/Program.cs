using System.Net;
using System.Text;
using alfaNET.MicroWeb.Abstractions;
using alfaNET.MicroWeb.Server;

namespace alfaNET.MicroWeb.Sample
{
    public class Program
    {
        private class Handler : IHttpListenerHandler
        {
            private readonly byte[] _message;

            public Handler(byte[] message)
            {
                _message = message;
            }

            public void Handle(HttpListenerContext context)
            {
                if (context.Request.RawUrl != "/")
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                context.Response.ContentEncoding = Encoding.UTF8;
                context.Response.ContentType = "text/plain; charset=UTF-8";
                context.Response.OutputStream.Write(_message, 0, _message.Length);
            }
        }

        public static void Main()
        {
            var messageBytes = Encoding.UTF8.GetBytes("Bunã lume!");
            var handler = new Handler(messageBytes);
            var site = new MicroWebSite { Port = 1234, Handler = handler };
            var server = new MicroWebServer(site, 64);
            server.Start();
        }
    }
}