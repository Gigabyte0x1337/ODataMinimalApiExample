using System.Net;

namespace OData.MinimalApi.Example.WebApi.OData.Results;

public static class ODataResults
{
    public static ODataResult ODataQuery<T>(this IResultExtensions resultExtensions, IQueryable<T> result)
    {
        return new ODataResult(result);
    }

    public static ODataResult ODataQuery<T>(this IResultExtensions resultExtensions, T result)
    {
        return new ODataResult(new EnumerableQuery<T>(new[] { result }));
    }

    public static ODataResult ODataEntity<T>(this IResultExtensions resultExtensions, T result)
    {
        return new ODataResult(result);
    }

    public static ODataCreatedResult ODataCreated<T>(this IResultExtensions resultExtensions, T result)
    {
        return new ODataCreatedResult(result);
    }

    public static ODataUpdatedResult ODataUpdated<T>(this IResultExtensions resultExtensions, T result)
    {
        return new ODataUpdatedResult(result);
    }

    public static ODataCreatedResult ODataCreated(this IResultExtensions resultExtensions, object result)
    {
        return new ODataCreatedResult(result);
    }

    public static ODataUpdatedResult ODataUpdated(this IResultExtensions resultExtensions, object result)
    {
        return new ODataUpdatedResult(result);
    }

    public static ODataResult ODataEntity(this IResultExtensions resultExtensions, object result)
    {
        return new ODataResult(result);
    }

    public static ODataResult OData(this IResultExtensions resultExtensions, object result, HttpStatusCode statusCode)
    {
        return new ODataResult(result, statusCode);
    }

    public static ODataResult OData<T>(this IResultExtensions resultExtensions, T result, HttpStatusCode statusCode)
    {
        return new ODataResult(result, statusCode);
    }
}