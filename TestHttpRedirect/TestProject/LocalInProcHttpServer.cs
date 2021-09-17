using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TestProject
{
    public class LocalInProcHttpServer : IDisposable
    {
        private readonly IWebHost host;
        private readonly CancellationTokenSource cts;

        public RequestDelegate ServerLogic = async (httpContext) => await httpContext.Response.WriteAsync("Hello World!");

        public LocalInProcHttpServer(string url)
        {
            this.cts = new CancellationTokenSource();
            this.host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(url)
                .Configure((app) =>
                {
                    app.Run(ServerLogic);
                })
                .Build();

            Task.Run(() => this.host.RunAsync(this.cts.Token));
        }

        public void Dispose()
        {
            this.cts.Cancel(false);
            try
            {
                this.host.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}