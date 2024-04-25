using Toki.MastodonApi.Renderers;

namespace Toki.MastodonApi;

/// <summary>
/// The extensions for the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds helpers for the Mastodon API.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <returns>The same services collection.</returns>
    public static IServiceCollection AddMastodonApiHelpers(this IServiceCollection services)
    {
        services.AddScoped<AccountRenderer>()
            .AddScoped<StatusRenderer>();

        return services;
    }
}