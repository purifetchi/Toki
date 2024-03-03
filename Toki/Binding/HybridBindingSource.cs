using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Toki.Binding;

/// <summary>
/// A binding source for hybrid requests.
/// </summary>
public class HybridBindingSource : BindingSource
{
    /// <summary>
    /// Constructs a new hybrid binding source.
    /// </summary>
    public HybridBindingSource()
        : base("Hybrid", "Hybrid", true, true)
    {
        
    }

    /// <summary>
    /// The singleton for the hybrid binding source.
    /// </summary>
    public static HybridBindingSource Singleton { get; } = new();
    
    /// <inheritdoc />
    public override bool CanAcceptDataFrom(BindingSource bindingSource) =>
        bindingSource == this;
}