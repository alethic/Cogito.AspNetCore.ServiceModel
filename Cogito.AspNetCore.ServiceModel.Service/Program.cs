using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Cogito.AspNetCore.ServiceModel.Service
{

    public static class Program
    {

        public static void Main(string[] args)
        {
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

    }

}
