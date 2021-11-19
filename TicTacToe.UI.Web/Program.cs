using System;
using System.Net.Http;
using System.Threading.Tasks;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TicTacToe.UI.Web.Services;

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
            builder.Services.AddBlazoredSessionStorage();
            builder.Services.AddTransient<IQLearningService, QLearningService>();

            await builder.Build().RunAsync();
            Console.WriteLine("hello");
        }
    }
}