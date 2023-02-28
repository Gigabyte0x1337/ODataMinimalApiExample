using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData.Formatter;

namespace OData.MinimalApi.Example.WebApi.OData;

public class ODataOutputSerializer
{
    private readonly IList<ODataOutputFormatter> _outputFormatters;

    public ODataOutputSerializer()
    {
        _outputFormatters = ODataOutputFormatterFactory.Create();
    }

    public async Task WriteAsync(HttpContext httpContext, object output)
    {
        // TODO create own formatter
        var acceptHeader = httpContext.Request.Headers.Accept.ToString()
            .Split(',')
            .Select(x => x.Trim());

        var formatter = _outputFormatters.FirstOrDefault(o =>
            acceptHeader.Any(x => x != null && o.SupportedMediaTypes.Contains(x))
        ) ?? _outputFormatters.FirstOrDefault(o =>
            o.SupportedMediaTypes.Contains("application/json")
        );

        if (formatter == null)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
            return;
        }

        var outputFormatterWriteContext = new OutputFormatterWriteContext(
             httpContext,
             (stream, encoding) => new StreamWriter(stream, encoding),
             output.GetType(),
             output
        )
        {
            ContentType = formatter.SupportedMediaTypes.FirstOrDefault(o => acceptHeader.Any(x => o == x)) ?? "application/json"
        };

        formatter.WriteResponseHeaders(outputFormatterWriteContext);
        await formatter.WriteResponseBodyAsync(outputFormatterWriteContext, Encoding.UTF8);
    }
}