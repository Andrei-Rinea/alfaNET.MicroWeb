using System.Net;

namespace alfaNET.MicroWeb.Abstractions
{
    public interface IHttpListenerHandler
    {
        void Handle(HttpListenerContext context);
    }
}