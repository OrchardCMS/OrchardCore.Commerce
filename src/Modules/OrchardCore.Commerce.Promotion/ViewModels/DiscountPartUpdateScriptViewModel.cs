using OrchardCore.Commerce.MoneyDataType;
using System.Collections.Generic;

namespace OrchardCore.Commerce.Promotion.ViewModels;

public class DiscountPartUpdateScriptViewModel
{
    public IList<DiscountPartUpdateScriptViewModelItem> Items { get; } = [];

    public void Add(string selector, Amount oldValue, Amount newValue) =>
        Items.Add(new DiscountPartUpdateScriptViewModelItem
        {
            QuerySelector = selector,
            OldValue = oldValue,
            NewValue = newValue,
        });

    public class DiscountPartUpdateScriptViewModelItem
    {
        public string QuerySelector { get; set; }
        public Amount OldValue { get; set; }
        public Amount NewValue { get; set; }
    }
}
