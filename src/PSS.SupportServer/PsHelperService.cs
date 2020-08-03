using Topshelf;

namespace PSS.SupportServer
{
    public class PsHelperService : ServiceControl
    {
        private Server? _server;

        public bool Start(HostControl hostControl)
        {
            _server = new Server();
            _server.Listen();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _server?.Shutdown();

            return true;
        }
    }
}