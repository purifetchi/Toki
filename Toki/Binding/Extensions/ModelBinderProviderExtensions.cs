using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Toki.Binding.Extensions;

/// <summary>
/// Extensions for the model binder provider.
/// </summary>
public static class ModelBinderProviderExtensions
{
    /// <summary>
    /// Adds the hybrid binding.
    /// </summary>
    /// <param name="providers">The providers list.</param>
    public static void AddHybridBindingProvider(this IList<IModelBinderProvider> providers)
    {
        var bodyProvider = providers.FirstOrDefault(p => p is BodyModelBinderProvider);
        var complexProvider = providers.FirstOrDefault(p => p is ComplexObjectModelBinderProvider);

        if (bodyProvider is null || complexProvider is null)
            return;
        
        providers.Insert(0,
            new HybridModelBinderProvider(bodyProvider, complexProvider));
    }
}