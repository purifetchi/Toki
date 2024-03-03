using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Toki.Binding;

/// <summary>
/// Tells the model that it can be bound either from the form or from the body.
/// <br/>
/// Massive thanks to Zotan who's implemented that in her IceShrimp.NET project
/// from which this was heavily inspired :)
/// </summary>
/// <param name="name">The name to bind from</param>
public class FromHybridAttribute(string? name = null) : 
    Attribute, 
    IBindingSourceMetadata, 
    IModelNameProvider
{
    /// <inheritdoc />
    public BindingSource? BindingSource => HybridBindingSource.Singleton;
    
    /// <inheritdoc />
    public string? Name => name;
}