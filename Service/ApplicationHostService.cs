using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Otaku16.Service
{
    public class ApplicationHostService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        public ApplicationHostService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            _serviceProvider.GetRequiredService<Handler>();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _serviceProvider.GetRequiredService<Cache>().Save();
            await Task.CompletedTask;
        }
    }
}
