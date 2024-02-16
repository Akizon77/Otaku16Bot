using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Service
{
    public class Hosting
    {
        private static readonly IHost _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration( c =>
            {

            }).ConfigureServices((c,s) =>
            {
                s.AddHostedService<ApplicationHostService>();
                s.AddSingleton<Config>();
                s.AddSingleton<Bot>();
                s.AddSingleton<Cache>();
                s.AddSingleton<History>();
                s.AddSingleton<Handler>();
            }).Build();
        public static T GetService<T>()
            where T : class
        {
            return _host.Services.GetService(typeof(T)) as T ?? throw new ArgumentNullException($"{typeof(T)} 未注册");
        }
        public static void Start()
        {
            _host.Start();
        }
        public static void Stop()
        {
            _host.StopAsync();
        }
        public static void WaitForStop()
        {
            _host.WaitForShutdown();
        }
    }
}
