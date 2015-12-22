using alfaNET.MicroWeb.Abstractions;

namespace alfaNET.MicroWeb.Server
{
    public class MicroWebSite
    {
        public IHttpListenerHandler Handler { get; set; }
        public int Port { get; set; }
        public bool Https { get; set; }
    }
}