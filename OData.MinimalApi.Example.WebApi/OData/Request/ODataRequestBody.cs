namespace OData.MinimalApi.Example.WebApi.OData.Request;

public class ODataRequestBody<T>
{
    public T? Entity { get; set; }

    public static async ValueTask<ODataRequestBody<T>> BindAsync(HttpContext context)
    {
        var inputDeserializer = context.RequestServices.GetRequiredService<ODataInputDeserializer>();

        return new ODataRequestBody<T>
        {
            Entity = await inputDeserializer.ReadAsync<T>(context)
        };
    }
}