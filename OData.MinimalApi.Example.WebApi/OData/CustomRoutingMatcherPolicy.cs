using Microsoft.AspNetCore.Routing.Matching;

namespace OData.MinimalApi.Example.WebApi.OData;

public class CustomRoutingMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 800;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
    {
        return true;
    }

    /// <summary>
    /// ODataRoutingMatcherPolicy.ApplyAsync -> new ODataTemplateTranslateContext(...) requires candidate.Values to be not null
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="candidates"></param>
    /// <returns></returns>
    public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        for (var i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];

            if (candidate.Values == null) 
                candidates.ReplaceEndpoint(i, candidate.Endpoint, new RouteValueDictionary());
        }

        return Task.CompletedTask;
    }
}