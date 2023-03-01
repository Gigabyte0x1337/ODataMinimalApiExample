using System.Net;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.OData;

namespace OData.MinimalApi.Example.WebApi.OData.Results;

public class ODataCreatedResult : ODataResult
{
    public const string EntityIdHeaderName = "OData-EntityId";
    public const string ODataServiceVersionHeader = "OData-Version";

    public ODataCreatedResult(object result = null, HttpStatusCode statusCode = HttpStatusCode.OK) : base(result, statusCode)
    {
    }

    public override async Task ExecuteAsync(HttpContext httpContext)
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

        httpContext.Response.Headers.Location = ODataResultHelpers.GenerateODataLink(httpContext.Request, Result, false)?.AbsoluteUri;

        if (ODataResultHelpers.RequestPrefersReturnNoContent(httpContext.Request.Headers))
        {
            httpContext.Response.Headers.Add(EntityIdHeaderName,
                ODataResultHelpers.GenerateODataLink(httpContext.Request, Result, true).AbsoluteUri);

            httpContext.Response.Headers.Add(ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(httpContext.Request.GetODataVersion()));

            httpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        else
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            await serializer.WriteAsync(httpContext, Result);
        }

    }
}