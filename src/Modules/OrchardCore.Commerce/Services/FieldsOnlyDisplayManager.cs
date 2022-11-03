using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.Templates.Controllers;
using OrchardCore.Templates.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class FieldsOnlyDisplayManager : IFieldsOnlyDisplayManager
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IHttpContextAccessor _hca;
    private readonly IShapeFactory _shapeFactory;
    private readonly TemplatesManager _templatesManager;

    public FieldsOnlyDisplayManager(
        IContentDefinitionManager contentDefinitionManager,
        IHttpContextAccessor hca,
        IShapeFactory shapeFactory,
        TemplatesManager templatesManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _hca = hca;
        _shapeFactory = shapeFactory;
        _templatesManager = templatesManager;
    }

    public IEnumerable<string> GetFieldShapeTypes(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail)
    {
        var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
        return typeDefinition
            .Parts
            .SelectMany(part =>
                part.PartDefinition.Fields.Select(field => new
                {
                    PartName = part.Name,
                    FieldName = field.Name,
                }))
            .Select(item => $"{contentItem.ContentType}_{displayType}__{item.PartName}__{item.FieldName}");
    }

    public async Task<IEnumerable<IShape>> DisplayFieldsAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail)
    {
        var fieldShapeTypes = GetFieldShapeTypes(contentItem, displayType);

        var shapes = new List<IShape>();
        foreach (var shapeType in fieldShapeTypes)
        {
            shapes.Add(await _shapeFactory.CreateAsync(shapeType));
        }

        return shapes;
    }

    public async Task<IEnumerable<string>> GetFieldTemplateEditorUrlsAsync(
        ContentItem contentItem,
        string displayType = CommonContentDisplayTypes.Detail)
    {
        var existingTemplates = (await _templatesManager.LoadTemplatesDocumentAsync()).Templates.Keys;

        if (_hca.HttpContext is not { } context) throw new InvalidOperationException("Missing HTTP context!");
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;

        var editAction = context.Action<TemplateController>(controller => controller.Edit(null, false, returnUrl));
        var createAction = context.Action<TemplateController>(controller => controller.Create(null, false, returnUrl));

        return GetFieldShapeTypes(contentItem, displayType)
            .Select(name => $"{(existingTemplates.Contains(name) ? editAction : createAction)}&name={name}");
    }
}
