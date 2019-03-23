using Microsoft.AspNetCore.Hosting;
using Schubert.Framework;
using System.IO;

namespace Schubert.MvcSample
{
    public class Program
    {
        public static void Main(string[] args)
        { 
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5002")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }

    public class ServerEntryPoint : IDependency
    {

    }
}   
