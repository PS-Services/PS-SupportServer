using System;
using System.Security.Cryptography.X509Certificates;
using Topshelf;

namespace PSS.SupportServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<PsHelperService>();
            });
        }
    }
}
