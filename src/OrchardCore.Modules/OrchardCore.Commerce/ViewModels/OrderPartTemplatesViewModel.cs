using System;
using System.Collections.Generic;

namespace OrchardCore.Commerce.ViewModels;

public class OrderPartTemplatesViewModel
{
    public IEnumerable<(Uri Url, string DisplayText, bool IsNew)> TemplateLinks { get; set; }
}
