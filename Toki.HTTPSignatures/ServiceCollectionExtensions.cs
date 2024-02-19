using Microsoft.Extensions.DependencyInjection;

namespace Toki.HTTPSignatures;

/// <summary>
/// Extensions for the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HTTP signatures.
    /// </summary>
    public static IServiceCollection AddHttpSignatures(this IServiceCollection collection)
    {
        collection.AddTransient<HttpSignatureValidator>();

        return collection;
    }
}