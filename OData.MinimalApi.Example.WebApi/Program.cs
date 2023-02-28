using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData;
using OData.MinimalApi.Example.WebApi.Endpoints;
using OData.MinimalApi.Example.WebApi.Models;
using OData.MinimalApi.Example.WebApi.OData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddODataMinimalApi(options =>
{
    var conventionModelBuilder = new ODataConventionModelBuilder();
    conventionModelBuilder.EnableLowerCamelCase();
    conventionModelBuilder.EntitySet<Customer>("customers");
    options.AddRouteComponents("api/v1.0", conventionModelBuilder.GetEdmModel(), new DefaultODataBatchHandler());
});

var app = builder.Build();

app.MapGroup("api/v1.0")
    .MapCustomerEndpoints();

app.Run();