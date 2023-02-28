using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.OData.Formatter;

namespace OData.MinimalApi.Example.WebApi.OData;
public class ODataInputDeserializer
{
    private readonly IModelMetadataProvider _modelMetadataProvider;
    private readonly ICompositeMetadataDetailsProvider _compositeMetadataProvider;
    private readonly ODataInputFormatter _inputFormatter;
    public ODataInputDeserializer(IModelMetadataProvider modelMetadataProvider, ICompositeMetadataDetailsProvider compositeMetadataProvider)
    {
        _modelMetadataProvider = modelMetadataProvider;
        _compositeMetadataProvider = compositeMetadataProvider;
        _inputFormatter = ODataInputFormatterFactory.Create().Reverse()
            .First(o => o.SupportedMediaTypes.Contains("application/json"));
    }

    public async ValueTask<object> ReadAsync(HttpContext context, Type type)
    {
        var modelState = new ModelStateDictionary();

        // Create custom formatter that's easier to consume
        var result = await _inputFormatter.ReadAsync(
            new InputFormatterContext(
                context, "",
                modelState,
                new DefaultModelMetadata(_modelMetadataProvider,
                    _compositeMetadataProvider,
                    new DefaultMetadataDetails(ModelMetadataIdentity.ForType(type),
                        ModelAttributes.GetAttributesForType(type))),
                (stream, encoding) => new StreamReader(stream, encoding)));

        return result.Model;
    }

    public async ValueTask<T> ReadAsync<T>(HttpContext context)
    {
        var modelState = new ModelStateDictionary();

        var result = await _inputFormatter.ReadAsync(
            new InputFormatterContext(
                context, "",
                modelState,
                new DefaultModelMetadata(_modelMetadataProvider,
                    _compositeMetadataProvider,
                    new DefaultMetadataDetails(ModelMetadataIdentity.ForType(typeof(T)),
                        ModelAttributes.GetAttributesForType(typeof(T)))),
                (stream, encoding) => new StreamReader(stream, encoding)));

        return (T)(result.Model ?? default(T));
    }
}