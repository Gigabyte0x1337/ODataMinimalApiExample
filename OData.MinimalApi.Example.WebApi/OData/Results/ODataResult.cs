using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Parser;
using Microsoft.Extensions.Options;
using Microsoft.OData.UriParser;

namespace OData.MinimalApi.Example.WebApi.OData.Results;

public class ODataResult<T> : IResult, IEndpointMetadataProvider, IEndpointParameterMetadataProvider
{
    private readonly HttpStatusCode _statusCode;

    public ODataResult(IQueryable<T> result, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _statusCode = statusCode;
        Result = result;
    }

    public IQueryable<T> Result { get; set; }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext.GetEndpoint()?.Metadata.FirstOrDefault(o => o is ODataRoutingMetadata) is not
            ODataRoutingMetadata endPointMetaData)
            throw new InvalidOperationException(
                "ODataQuery endpoint metadata not found maybe this is not a valid endpoint.");

        var serializer = httpContext.RequestServices.GetRequiredService<ODataOutputSerializer>();

        object? result = Result;
        if (Result is IQueryable queryable)
        {
            var odataFeature = httpContext.ODataFeature();
            var queryOptions =
                new ODataQueryOptions(
                    new ODataQueryContext(endPointMetaData.Model, queryable.ElementType, odataFeature.Path),
                    httpContext.Request
                );

            if (httpContext.GetEndpoint()?.Metadata.FirstOrDefault(o => o is ODataValidationSettings) is
                ODataValidationSettings validationSettings)
            {
                queryOptions.Validate(validationSettings);
            }

            result = httpContext.GetEndpoint()?.Metadata
               .FirstOrDefault(o => o is ODataQuerySettings) is not ODataQuerySettings querySettings
               ? queryOptions.ApplyTo(Result)
               : queryOptions.ApplyTo(Result, querySettings);

            if (odataFeature.Path.LastSegment is KeySegment)
            {
                var enumerator = ((IQueryable)result).GetEnumerator();

                try
                {
                    result = enumerator.MoveNext() ? enumerator.Current : null;
                }
                finally
                {
                    // Ensure any active/open database objects that were created
                    // iterating over the IQueryable object are properly closed.
                    var disposable = enumerator as IDisposable;
                    disposable?.Dispose();
                }
            }

            if (result == null)
            {
                httpContext.Response.StatusCode = 404;
                return;
            }
        }

        if (_statusCode != HttpStatusCode.OK)
        {
            httpContext.Response.StatusCode = (int)_statusCode;
        }

        await serializer.WriteAsync(httpContext, result);
    }

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        var routeEndPointBuilder = (RouteEndpointBuilder)builder;

        var modelResolver = builder.ApplicationServices.GetRequiredService<ODataModelResolver>();
        var pathTemplateParser = builder.ApplicationServices.GetRequiredService<IODataPathTemplateParser>();
        var odataOptions = builder.ApplicationServices.GetRequiredService<IOptions<ODataOptions>>();

        var odataPath = routeEndPointBuilder.RoutePattern.RawText;
        var routePrefix = odataOptions.Value.RouteComponents.Keys.FirstOrDefault(odataPath.StartsWith) ?? string.Empty;
        var model = modelResolver.GetModel(routePrefix);
        var odataPathTemplate = pathTemplateParser.Parse(model, odataPath.Replace(routePrefix + "/", ""), null);

        builder.Metadata.Add(new ODataRoutingMetadata(routePrefix, model, odataPathTemplate));
    }

    public static void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
    {

    }
}

public class ODataResult : IResult, IEndpointMetadataProvider, IEndpointParameterMetadataProvider
{
    private readonly HttpStatusCode _statusCode;

    public ODataResult(object? result, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _statusCode = statusCode;
        Result = result;
    }

    public object? Result { get; set; }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext.GetEndpoint()?.Metadata.FirstOrDefault(o => o is ODataRoutingMetadata) is not
            ODataRoutingMetadata endPointMetaData)
            throw new InvalidOperationException(
                "ODataQuery endpoint metadata not found maybe this is not a valid endpoint.");

        var serializer = httpContext.RequestServices.GetRequiredService<ODataOutputSerializer>();

        var result = Result;
        if (Result is IQueryable queryable)
        {
            var odataFeature = httpContext.ODataFeature();
            var queryOptions =
                new ODataQueryOptions(
                    new ODataQueryContext(endPointMetaData.Model, queryable.ElementType, odataFeature.Path),
                    httpContext.Request
                );

            if (httpContext.GetEndpoint()?.Metadata.FirstOrDefault(o => o is ODataValidationSettings) is
                ODataValidationSettings validationSettings)
            {
                queryOptions.Validate(validationSettings);
            }

            result = httpContext.GetEndpoint()?.Metadata
               .FirstOrDefault(o => o is ODataQuerySettings) is not ODataQuerySettings querySettings
               ? queryOptions.ApplyTo(queryable)
               : queryOptions.ApplyTo(queryable, querySettings);

            if (odataFeature.Path.LastSegment is KeySegment)
            {
                var enumerator = ((IQueryable)result).GetEnumerator();

                try
                {
                    result = enumerator.MoveNext() ? enumerator.Current : null;
                }
                finally
                {
                    // Ensure any active/open database objects that were created
                    // iterating over the IQueryable object are properly closed.
                    var disposable = enumerator as IDisposable;
                    disposable?.Dispose();
                }
            }

            if (result == null)
            {
                httpContext.Response.StatusCode = 404;
                return;
            }
        }

        if (_statusCode != HttpStatusCode.OK)
        {
            httpContext.Response.StatusCode = (int)_statusCode;
        }

        // TODO What to do when result is null
        await serializer.WriteAsync(httpContext, result);
    }

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        var routeEndPointBuilder = (RouteEndpointBuilder)builder;

        var modelResolver = builder.ApplicationServices.GetRequiredService<ODataModelResolver>();
        var pathTemplateParser = builder.ApplicationServices.GetRequiredService<IODataPathTemplateParser>();
        var odataOptions = builder.ApplicationServices.GetRequiredService<IOptions<ODataOptions>>();

        var odataPath = routeEndPointBuilder.RoutePattern.RawText;
        if (odataPath == null)
            return;

        var routePrefix = odataOptions.Value.RouteComponents.Keys.FirstOrDefault(odataPath.StartsWith) ?? string.Empty;
        var model = modelResolver.GetModel(routePrefix);
        var odataPathTemplate = pathTemplateParser.Parse(model, odataPath.Replace(routePrefix + "/", ""), null);

        builder.Metadata.Add(new ODataRoutingMetadata(routePrefix, model, odataPathTemplate));
    }

    public static void PopulateMetadata(ParameterInfo parameter, EndpointBuilder builder)
    {

    }
}

