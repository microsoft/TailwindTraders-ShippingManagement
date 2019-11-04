
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TailwindTraders.ShippingManagement.Models;
using TailwindTraders.ShippingManagement.Services;
using TailwindTraders.ShippingManagement.Services.Contracts;

[assembly: FunctionsStartup(typeof(TailwindTraders.ShippingManagement.Startup))]

namespace TailwindTraders.ShippingManagement
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging();
            builder.Services.AddOptions<Settings>()
                    .Configure<IConfiguration>((settings, configuration) => {
                        configuration.Bind(settings);
                    });
            builder.Services.AddSingleton<IAnalysisService,AnalysisService>();
            builder.Services.AddSingleton<IRequestService, RequestService>();
            builder.Services.AddSingleton<IResponseService, ResponseService>();
        }
    }
}
