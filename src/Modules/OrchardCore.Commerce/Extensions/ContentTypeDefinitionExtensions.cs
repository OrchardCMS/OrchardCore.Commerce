﻿using System;
using System.Linq;

namespace OrchardCore.ContentManagement.Metadata.Models;

public static class ContentTypeDefinitionExtensions
{
    public static (ContentTypePartDefinition PartDefinition, ContentPartFieldDefinition FieldDefinition)
        GetFieldDefinition(this ContentTypeDefinition type, string attributeName)
    {
        var (partName, _, fieldName) = attributeName.Partition(".");

        return type
            .Parts
            .Where(partDefinition => partDefinition.Name == partName)
            .SelectMany(partDefinition => partDefinition
                .PartDefinition
                .Fields
                .Select(fieldDefinition => (PartDefinition: partDefinition, FieldDefinition: fieldDefinition))
                .Where(pair => pair.FieldDefinition.Name == fieldName))
            .FirstOrDefault();
    }
}
