using Lombiq.HelpfulLibraries.OrchardCore.Contents;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Services;

public class FieldsOnlyDisplayManager : IFieldsOnlyDisplayManager
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IShapeFactory _shapeFactory;

    public FieldsOnlyDisplayManager(IContentDefinitionManager contentDefinitionManager, IShapeFactory shapeFactory)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _shapeFactory = shapeFactory;
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
}
