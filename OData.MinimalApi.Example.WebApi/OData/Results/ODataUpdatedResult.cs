using System.Net;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;

namespace OData.MinimalApi.Example.WebApi.OData.Results;

public class ODataUpdatedResult : ODataResult
{
    public const string EntityIdHeaderName = "OData-EntityId";
    public const string ODataServiceVersionHeader = "OData-Version";

    public ODataUpdatedResult(object result = null, HttpStatusCode statusCode = HttpStatusCode.OK) : base(result, statusCode)
    {
    }

    public ODataUpdatedResult() : base(null)
    {
    }

    public new async Task ExecuteAsync(HttpContext httpContext)
    {
        if (httpContext.GetEndpoint()?.Metadata.FirstOrDefault(o => o is ODataRoutingMetadata) is not
            ODataRoutingMetadata endPointMetaData)
            throw new InvalidOperationException(
                "ODataQuery endpoint metadata not found maybe this is not a valid endpoint.");

        if (Result == null)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        var serializer = httpContext.RequestServices.GetRequiredService<ODataOutputSerializer>();
        if (ODataResultHelpers.RequestPrefersReturnContent(httpContext.Request.Headers))
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            await serializer.WriteAsync(httpContext, Result);
        }
        else
        {
            httpContext.Response.Headers.Add(ODataServiceVersionHeader, ODataUtils.ODataVersionToString(httpContext.Request.GetODataVersion()));
            httpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
    }


}