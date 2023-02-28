using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;

namespace OData.MinimalApi.Example.WebApi.OData;

public class ODataModelResolver
{
    private readonly IOptions<ODataOptions> _options;

    public ODataModelResolver(IOptions<ODataOptions> options)
    {
        _options = options;
    }

    public IEdmModel? GetModel(string? prefix = null)
    {
        var routeComponent = _options.Value.RouteComponents[prefix ?? string.Empty];
        return routeComponent.EdmModel;
    }
}