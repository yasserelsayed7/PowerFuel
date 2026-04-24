using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PowerFuel.API.Services.FitnessCoach;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFitnessCoach(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<FitnessCoachOptions>()
            .Bind(configuration.GetSection(FitnessCoachOptions.SectionName))
            .Validate(o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out _), "FitnessCoach:BaseUrl must be an absolute URL.")
            .ValidateOnStart();

        services.AddHttpClient<IFitnessCoachClient, FitnessCoachClient>((sp, client) =>
        {
            var o = sp.GetRequiredService<IOptions<FitnessCoachOptions>>().Value;
            var baseUrl = o.BaseUrl.Trim().TrimEnd('/') + "/";
            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromMinutes(2);
            client.DefaultRequestHeaders.Remove("X-API-Key");
            if (!string.IsNullOrWhiteSpace(o.ApiKey))
                client.DefaultRequestHeaders.Add("X-API-Key", o.ApiKey);
        });

        return services;
    }
}
