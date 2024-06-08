using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VirtualRadar.Server.Components;

namespace VirtualRadar.Server
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMvc(options => {
                options.EnableEndpointRouting = false;
            });

            builder.Services.AddControllers();
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();

            builder.Logging.ClearProviders();

            var app = builder.Build();

            if(!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseMvcWithDefaultRoute();
            app.UseAntiforgery();
            app.MapRazorComponents<App>()
               .AddInteractiveServerRenderMode();

            Console.WriteLine($"Starting server");
            var cancellationSource = new CancellationTokenSource();
            var task = app.StartAsync(cancellationSource.Token);

            Console.TreatControlCAsInput = true;
            Console.WriteLine("Press Q to quit");

            while(!Console.KeyAvailable) {
                if(cancellationSource.IsCancellationRequested) {
                    break;
                }
                Thread.Sleep(1);

                while(Console.KeyAvailable) {
                    if(Console.ReadKey(intercept: true).Key == ConsoleKey.Q) {
                        cancellationSource.Cancel();
                        break;
                    }
                }
            }

            await task;

            return 0;
        }
    }
}
