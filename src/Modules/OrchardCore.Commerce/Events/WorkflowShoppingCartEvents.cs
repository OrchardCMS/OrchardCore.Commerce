using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Commerce.ViewModels;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

/// <summary>
/// Event handler that just triggers the matching workflows.
/// </summary>
public class WorkflowShoppingCartEvents : IShoppingCartEvents
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowTypeStore _workflowTypeStore;

    // After all the default events, but it should be still possible to add different ordered event handlers after it.
    public int Order => int.MaxValue / 2;

    public WorkflowShoppingCartEvents(
        IWorkflowManager workflowManager,
        IWorkflowTypeStore workflowTypeStore)
    {
        _workflowManager = workflowManager;
        _workflowTypeStore = workflowTypeStore;
    }

    public async Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        ShoppingCartDisplayingEventContext eventContext)
    {
        var results = await TriggerEventAsync<CartDisplayingEvent>(eventContext);

        return (eventContext.Headers, eventContext.Lines);
    }

    public async Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item)
    {
        var results = await TriggerEventAsync<CartVerifyingItemEvent>(item);

        return null;
    }

    public async Task<ShoppingCart> LoadedAsync(ShoppingCart shoppingCart)
    {
        var results = await TriggerEventAsync<CartLoadedEvent>(shoppingCart);

        return shoppingCart;
    }

    private async Task<IList<WorkflowExecutionContext>> TriggerEventAsync<T>(object input)
    {
        var name = typeof(T).Name;
        var values = new RouteValueDictionary(input);
        var contexts = new List<WorkflowExecutionContext>();

        // Start new workflows whose types have a corresponding starting activity.
        var workflowTypesToStart = await _workflowTypeStore.GetByStartActivityAsync(name);
        foreach (var workflowType in workflowTypesToStart)
        {
            var startActivity = workflowType.Activities.First(x => x.IsStart && x.Name == name);
            contexts.Add(await _workflowManager.StartWorkflowAsync(workflowType, startActivity, values));
        }

        return contexts;
    }
}
