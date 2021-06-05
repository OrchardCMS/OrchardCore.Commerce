using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Commerce.Activities
{
    public class ProductAddedToCartEvent : EventActivity
    {
        private readonly IStringLocalizer<ProductAddedToCartEvent> S;

        public ProductAddedToCartEvent(IStringLocalizer<ProductAddedToCartEvent> localizer)
        {
            S = localizer;
        }

        public override string Name => nameof(ProductAddedToCartEvent);

        public override LocalizedString DisplayText => S["Product addded to cart"];

        public override LocalizedString Category => S["Commerce"];

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
            => Outcomes(S["Done"]);
    }
}
