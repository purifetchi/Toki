using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Toki.Middleware.Routing;

/// <summary>
/// Conditionally allows access to a route based on the Accept field.
/// </summary>
/// <param name="types">The accepted types.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ConditionalAcceptAttribute(params string[] types)
    : Attribute, IActionConstraint
{
    /// <inheritdoc />
    public bool Accept(ActionConstraintContext context)
    {
        context
            .RouteContext
            .HttpContext
            .Response
            .Headers
            .Append("Vary", "Accept");
        
        var headers = context
            .RouteContext
            .HttpContext
            .Request
            .Headers;

        if (!headers.TryGetValue("Accept", out var accept))
            return false;

        var accepts = accept.ToString()
            .Split(',')
            .Select(type => MediaTypeWithQualityHeaderValue.Parse(type)!.MediaType);
        
        return accepts.Any(types.Contains);
    }

    /// <inheritdoc />
    public int Order { get; } = HttpMethodActionConstraint.HttpMethodConstraintOrder + 1;
}