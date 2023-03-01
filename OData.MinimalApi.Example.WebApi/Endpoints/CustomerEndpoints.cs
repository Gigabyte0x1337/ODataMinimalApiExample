using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OData.Deltas;
using OData.MinimalApi.Example.WebApi.Models;
using OData.MinimalApi.Example.WebApi.OData.Request;
using OData.MinimalApi.Example.WebApi.OData.Results;

namespace OData.MinimalApi.Example.WebApi.Endpoints;

public static class CustomerEndpoints
{
    public static List<Customer> Customers = new()
    {
        new() { Id = 1, Name = "Customer1" },
        new() { Id = 2, Name = "Customer2" }
    };

    public static ODataResult GetAll()
    {
        return Results.Extensions.ODataQuery(Customers.AsQueryable());
    }

    public static Results<BadRequest, ODataResult> Get(int id)
    {
        var customersFilteredById = Customers.Where(c => c.Id == id);

        return Results.Extensions.ODataQuery(customersFilteredById);
    }

    public static Results<BadRequest, ODataResult> UpdatePatch(int id, ODataRequestBody<Delta<Customer>> request)
    {
        var requestCustomer = request.Entity;
        if (requestCustomer == null)
        {
            return TypedResults.BadRequest();
        }

        var customer = Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
        {
            return TypedResults.BadRequest();
        }

        requestCustomer.Patch(customer);
        
        return Results.Extensions.ODataUpdated(customer);
    }

    public static Results<BadRequest, ODataResult> UpdatePut(int id, ODataRequestBody<Delta<Customer>> request)
    {
        var requestCustomer = request.Entity;
        if (requestCustomer == null)
        {
            return TypedResults.BadRequest();
        }

        var customer = Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
        {
            return TypedResults.BadRequest();
        }

        requestCustomer.Put(customer);

        return Results.Extensions.ODataUpdated(customer);
    }

    public static Results<BadRequest, ODataResult> Create(ODataRequestBody<Customer> request)
    {
        var requestCustomer = request.Entity;
        if (requestCustomer == null)
        {
            return TypedResults.BadRequest();
        }

        var customer = new Customer()
        {
            Id = Customers.Max(c => c.Id) + 1,
            Name = requestCustomer.Name
        };
        Customers.Add(customer);

        return Results.Extensions.ODataCreated(customer);
    }

    public static Results<BadRequest, NoContent> Delete(int id)
    {
        var customer = Customers.FirstOrDefault(c => c.Id == id);
        if (customer == null)
        {
            return TypedResults.BadRequest();
        }

        Customers.Remove(customer);
        
        return TypedResults.NoContent();
    }

    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("customers", GetAll);
        builder.MapGet("customers/{id:int}", Get);
        builder.MapGet("customers({id:int})", Get);
        builder.MapPatch("customers/{id:int}", UpdatePatch);
        builder.MapPatch("customers({id:int})", UpdatePatch);
        builder.MapPut("customers/{id:int}", UpdatePut);
        builder.MapPut("customers({id:int})", UpdatePut);
        builder.MapPost("customers", Create);
        builder.MapDelete("customers/{id:int}", Delete);
        builder.MapDelete("customers({id:int})", Delete);

        return builder;
    }
}