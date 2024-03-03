using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Toki.Binding;

/// <summary>
/// The hybrid model binder.
/// </summary>
public class HybridModelBinder(
    IModelBinder? bodyBinder,
    IModelBinder? complexBinder) : IModelBinder
{
    /// <inheritdoc/>
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bodyBinder != null)
        {
            if (bindingContext is { IsTopLevelObject: true, HttpContext.Request: { HasFormContentType: false, ContentLength: > 0 } })
            {
                bindingContext.BindingSource = BindingSource.Body;
                await bodyBinder.BindModelAsync(bindingContext);
            }
        }

        if (complexBinder != null &&
            !bindingContext.Result.IsModelSet)
        {
            bindingContext.BindingSource = BindingSource.ModelBinding;
            await complexBinder.BindModelAsync(bindingContext);
        }

        if (bindingContext.Result.IsModelSet)
            bindingContext.Model = bindingContext.Result.Model;
    }
}