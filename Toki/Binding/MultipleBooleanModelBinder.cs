using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Toki.Binding;

/// <summary>
/// A model binder that binds from multiple values that might be a boolean. (e.g. true/false, 0/1, null)
/// </summary>
public class MultipleBooleanModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider
            .GetValue(bindingContext.ModelName);
        var value = valueProviderResult.FirstValue;

        if (int.TryParse(value, out var intVal))
            bindingContext.Result = ModelBindingResult.Success(intVal == 1);
        else if (bool.TryParse(value, out var boolVal))
            bindingContext.Result = ModelBindingResult.Success(boolVal);
        else if (string.IsNullOrWhiteSpace(value))
            bindingContext.Result = ModelBindingResult.Success(false);
        else
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName,
                $"{bindingContext.ModelName} wasn't either an empty string, a numeric bool or a string bool");
            
        return Task.CompletedTask;
    }
}