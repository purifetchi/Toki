using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Toki.Binding;

/// <summary>
/// The provider for the hybrid binder.
/// </summary>
public class HybridModelBinderProvider(
    IModelBinderProvider bodyProvider,
    IModelBinderProvider complexProvider) : IModelBinderProvider
{
    /// <inheritdoc/>
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.BindingInfo.BindingSource == null ||
            !context.BindingInfo.BindingSource.CanAcceptDataFrom(HybridBindingSource.Singleton))
        {
            return null;
        }
        
        context.BindingInfo.BindingSource = BindingSource.Body;
        var bodyBinder = bodyProvider.GetBinder(context);
        
        context.BindingInfo.BindingSource = BindingSource.ModelBinding;
        var complexBinder = complexProvider.GetBinder(context);

        return new HybridModelBinder(bodyBinder, complexBinder);
    }
}