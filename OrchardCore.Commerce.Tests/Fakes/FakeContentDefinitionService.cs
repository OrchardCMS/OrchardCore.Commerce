using System;
using System.Collections.Generic;
using OrchardCore.Commerce.Fields;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Services;
using OrchardCore.ContentTypes.ViewModels;

namespace OrchardCore.Commerce.Tests.Fakes
{
    public class FakeContentDefinitionService : IContentDefinitionService
    {
        public void AddFieldToPart(string fieldName, string fieldTypeName, string partName) => throw new NotImplementedException();

        public void AddFieldToPart(string fieldName, string displayName, string fieldTypeName, string partName) => throw new NotImplementedException();

        public EditPartViewModel AddPart(CreatePartViewModel partViewModel) => throw new NotImplementedException();

        public void AddPartToType(string partName, string typeName) => throw new NotImplementedException();

        public void AddReusablePartToType(string name, string displayName, string description, string partName, string typeName) => throw new NotImplementedException();

        public ContentTypeDefinition AddType(string name, string displayName) => throw new NotImplementedException();

        public void AlterField(EditPartViewModel partViewModel, EditFieldViewModel fieldViewModel) => throw new NotImplementedException();

        public void AlterPartFieldsOrder(ContentPartDefinition partDefinition, string[] fieldNames) => throw new NotImplementedException();

        public void AlterTypePart(EditTypePartViewModel partViewModel) => throw new NotImplementedException();

        public void AlterTypePartsOrder(ContentTypeDefinition typeDefinition, string[] partNames) => throw new NotImplementedException();

        public string GenerateContentTypeNameFromDisplayName(string displayName) => throw new NotImplementedException();

        public string GenerateFieldNameFromDisplayName(string partName, string displayName) => throw new NotImplementedException();

        public IEnumerable<Type> GetFields()
            => new[] {
                typeof(BooleanProductAttributeField),
                typeof(TextProductAttributeField),
                typeof(BooleanField),
                typeof(TextField)
            };

        public EditPartViewModel GetPart(string name) => throw new NotImplementedException();

        public IEnumerable<EditPartViewModel> GetParts(bool metadataPartsOnly) => throw new NotImplementedException();

        public EditTypeViewModel GetType(string name) => throw new NotImplementedException();

        public IEnumerable<EditTypeViewModel> GetTypes() => throw new NotImplementedException();

        public EditPartViewModel LoadPart(string name) => throw new NotImplementedException();

        public IEnumerable<EditPartViewModel> LoadParts(bool metadataPartsOnly) => throw new NotImplementedException();

        public EditTypeViewModel LoadType(string name) => throw new NotImplementedException();

        public IEnumerable<EditTypeViewModel> LoadTypes() => throw new NotImplementedException();

        public void RemoveFieldFromPart(string fieldName, string partName) => throw new NotImplementedException();

        public void RemovePart(string name) => throw new NotImplementedException();

        public void RemovePartFromType(string partName, string typeName) => throw new NotImplementedException();

        public void RemoveType(string name, bool deleteContent) => throw new NotImplementedException();
    }
}
