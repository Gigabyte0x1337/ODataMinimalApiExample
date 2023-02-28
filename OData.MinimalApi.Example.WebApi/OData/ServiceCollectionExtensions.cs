using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using OData.MinimalApi.Example.WebApi.Models;

namespace OData.MinimalApi.Example.WebApi.OData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddODataMinimalApi(this IServiceCollection services, Action<ODataOptions> configureOptions)
    {
        services.AddSingleton((Func<IServiceProvider, ODataUriResolver>)(_ => new UnqualifiedODataUriResolver
        {
            EnableCaseInsensitive = true
        }));

        services.AddSingleton<ODataModelResolver>();
        
        // TODO create custom serializers instead of relying on mvc input/output formatters
        services.AddSingleton<ODataOutputSerializer>();
        services.AddSingleton<ODataInputDeserializer>();

        // ODataRoutingMatcherPolicy.ApplyAsync -> new ODataTemplateTranslateContext(...) requires candidate.Values to be not null
        // Can be fixed in Microsoft.AspNetCore.OData.Routing.ODataRoutingMatcherPolicy.ApplyAsync
        services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, CustomRoutingMatcherPolicy>());
        
        // TODO improvement use services.AddODataCore() if can be made public because mvc is not needed
        services.AddControllers()
            .AddOData(configureOptions);

        return services;
    }
}
