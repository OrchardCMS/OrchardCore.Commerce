using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.Settings;
using OrchardCore.ContentManagement;

namespace OrchardCore.Commerce.Services
{
    public class PredefinedValuesProductAttributeService : IPredefinedValuesProductAttributeService
    {
        private readonly IProductAttributeService _productAttributeService;

        public PredefinedValuesProductAttributeService(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        public IEnumerable<IEnumerable<string>> GetProductAttributesPredefinedValues(ContentItem product)
        {
            return _productAttributeService.GetProductAttributeFields(product)
                .Where(x => x.Settings is TextProductAttributeFieldSettings textSettings && textSettings.RestrictToPredefinedValues)
                .Select(x => (x.Settings as TextProductAttributeFieldSettings).PredefinedValues.ToList())
                .ToList();
        }

        public IEnumerable<string> GetProductAttributesCombinations(ContentItem product)
        {
            return CartesianProduct(GetProductAttributesPredefinedValues(product))
                .Select(x => string.Join("-", x));
        }

        private IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }
    }
}
