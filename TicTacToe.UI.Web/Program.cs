using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TicTacToe.UI.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            var functionUrl = builder.Configuration["FunctionUrl"];
            if (functionUrl == null)
            {
                throw new InvalidOperationException(functionUrl);
            }

            builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(functionUrl) });

            await builder.Build().RunAsync();
        }
    }
}