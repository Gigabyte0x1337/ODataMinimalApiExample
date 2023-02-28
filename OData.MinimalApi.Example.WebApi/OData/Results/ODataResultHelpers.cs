using System.Diagnostics.Contracts;
using Microsoft.AspNetCore.OData.Edm;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;

namespace OData.MinimalApi.Example.WebApi.OData.Results;

public class ODataResultHelpers
{
    public const string PreferHeaderName = "Prefer";
    public const string ReturnContentHeaderValue = "return=representation";
    public const string ReturnNoContentHeaderValue = "return=minimal";
    public const string ODataMaxPageSize = "odata.maxpagesize";
    public const string MaxPageSize = "maxpagesize";

    public static Uri GenerateODataLink(HttpRequest request, object entity, bool isEntityId)
    {
        var model = request.GetModel();
        if (model == null)
        {
            throw new InvalidOperationException("RequestMustHaveModel");
        }

        var path = request.ODataFeature().Path;
        if (path == null)
        {
            throw new InvalidOperationException("ODataPathMissing");
        }

        var navigationSource = path.GetNavigationSource();
        if (navigationSource == null)
        {
            throw new InvalidOperationException("NavigationSourceMissingDuringSerialization");
        }

        var serializerContext = new ODataSerializerContext
        {
            NavigationSource = navigationSource,
            Model = model,
            MetadataLevel = ODataMetadataLevel.Full, // Used internally to always calculate the links.
            Request = request,
            Path = path
        };

        var entityType = GetEntityType(model, entity);
        var resourceContext = new ResourceContext(serializerContext, entityType, entity);

        return GenerateODataLink(resourceContext, isEntityId);
    }

    internal static bool RequestPrefersReturnContent(IHeaderDictionary headers)
    {
        StringValues preferences;
        if (headers.TryGetValue(PreferHeaderName, out preferences))
        {
            return preferences.FirstOrDefault(s => s.IndexOf(ReturnContentHeaderValue, StringComparison.OrdinalIgnoreCase) >= 0) != null;
        }
        return false;
    }

    internal static bool RequestPrefersReturnNoContent(IHeaderDictionary headers)
    {
        StringValues preferences;
        if (headers.TryGetValue(PreferHeaderName, out preferences))
        {
            return preferences.FirstOrDefault(s => s.IndexOf(ReturnNoContentHeaderValue, StringComparison.OrdinalIgnoreCase) >= 0) != null;
        }
        return false;
    }

    public static Uri GenerateODataLink(ResourceContext resourceContext, bool isEntityId)
    {
        Contract.Assert(resourceContext != null);

        // // Generate location or entityId header from request Uri and key, if Post to a containment.
        // // Link builder is not used, since it is also for generating ID, Edit, Read links, etc. scenarios, where
        // // request Uri is not used.
        // if (resourceContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet)
        // {
        //     return GenerateContainmentODataPathSegments(resourceContext, isEntityId);
        // }

        var linkBuilder =
            resourceContext.EdmModel.GetNavigationSourceLinkBuilder(resourceContext.NavigationSource);
        Contract.Assert(linkBuilder != null);

        var idLink = linkBuilder.BuildIdLink(resourceContext, ODataMetadataLevel.Full);
        if (isEntityId)
        {
            return idLink;
        }

        var editLink = linkBuilder.BuildEditLink(resourceContext, ODataMetadataLevel.Full, idLink);


        return editLink ?? idLink ?? throw new Exception("Unable to find odata path");
    }

    private static IEdmEntityTypeReference GetEntityType(IEdmModel model, object entity)
    {
        var entityType = entity.GetType();
        var edmType = model.GetTypeMapper().GetEdmTypeReference(model, entityType);
        if (edmType == null)
        {
            throw new Exception("ResourceTypeNotInModel " + entityType.FullName);
        }
        if (!edmType.IsEntity())
        {
            throw new Exception("TypeMustBeEntity " + edmType.FullName());
        }

        return edmType.AsEntity();
    }
}