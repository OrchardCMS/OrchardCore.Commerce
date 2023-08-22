using Lombiq.HelpfulLibraries.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json;
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

    private readonly JsonSerializerSettings _settings;

    // After all the default events, but it should be still possible to add different ordered event handlers after it.
    public int Order => int.MaxValue / 2;

    public WorkflowShoppingCartEvents(
        IWorkflowManager workflowManager,
        IWorkflowTypeStore workflowTypeStore)
    {
        _workflowManager = workflowManager;
        _workflowTypeStore = workflowTypeStore;

        var defaultConverters = JsonConvert.DefaultSettings?.Invoke()?.Converters ?? new List<JsonConverter>();
        defaultConverters.Add(new LocalizedHtmlStringConverter());
        _settings = new JsonSerializerSettings { Converters = defaultConverters };
    }

    public async Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        ShoppingCartDisplayingEventContext eventContext)
    {
        var results = await TriggerEventAsync<CartDisplayingEvent>(eventContext);

        if (!results.Any()) return (eventContext.Headers, eventContext.Lines);

        var headers = GetOutput<IList<LocalizedHtmlString>>(results, nameof(eventContext.Headers)) ?? eventContext.Headers;
        var lines = GetOutput<IList<ShoppingCartLineViewModel>>(results, nameof(eventContext.Lines)) ?? eventContext.Lines;

        return (headers, lines);
    }

    public async Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item) =>
        GetOutput<LocalizedHtmlString>(
            await TriggerEventAsync<CartVerifyingItemEvent>(item),
            "Error");

    public async Task<ShoppingCart> LoadedAsync(ShoppingCart shoppingCart) =>
        GetOutput<ShoppingCart>(
            await TriggerEventAsync<CartLoadedEvent>(shoppingCart),
            nameof(ShoppingCart)) ?? shoppingCart;

    private async Task<IList<WorkflowExecutionContext>> TriggerEventAsync<T>(object input)
    {
        var name = typeof(T).Name;
        var values = new Dictionary<string, object>
        {
            ["Context"] = input,
            ["JSON"] = JsonConvert.SerializeObject(input),
        };

        // Start new workflows whose types have a corresponding starting activity.
        var workflowTypesToStart = await _workflowTypeStore.GetByStartActivityAsync(name);
        var contexts = new List<WorkflowExecutionContext>();
        foreach (var workflowType in workflowTypesToStart)
        {
            var startActivity = workflowType.Activities.First(x => x.IsStart && x.Name == name);
            contexts.Add(await _workflowManager.StartWorkflowAsync(workflowType, startActivity, values));
        }

        return contexts;
    }

    private T GetOutput<T>(IEnumerable<WorkflowExecutionContext> contexts, string outputName)
        where T : class
    {
        var output = contexts
            .Where(context => context.Status is not (WorkflowStatus.Faulted or WorkflowStatus.Halted or WorkflowStatus.Aborted))
            .SelectWhere(context => context.Output.GetMaybe(outputName))
            .FirstOrDefault();

        return output switch
        {
            string outputLocalizedHtmlString when typeof(T) == typeof(LocalizedHtmlString) =>
                new LocalizedHtmlString(outputLocalizedHtmlString, outputLocalizedHtmlString) as T,
            string outputString => JsonConvert.DeserializeObject<T>(outputString, _settings),
            { } outputObject => JsonConvert.DeserializeObject<T>(
                JsonConvert.SerializeObject(outputObject, _settings),
                _settings),
            _ => default,
        };
    }
}
