using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Commerce.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Commerce.ViewModels
{
    public class PriceBookProductPartEditViewModel
    {
        // If there are two PriceBookEntries, there will be two Prefixes
        // Each value in Prefixes is a Guid that represents the unique
        // HtmlFieldPrefix value of its editor

        public string[] Prefixes { get; set; } = Array.Empty<string>();
        // Due ListPart.ContainerId not being prefixed, need to track price books manually
        public string[] PriceBookContentItemIds { get; set; } = Array.Empty<string>();

        [BindNever]
        public PriceBookProductPart PriceBookProductPart { get; set; }

        [BindNever]
        public IEnumerable<IContent> PriceBookEntries { get; set; }

        [BindNever]
        public IEnumerable<IContent> PriceBooks { get; set; }

        [BindNever]
        public IUpdateModel Updater { get; set; }
    }
}
