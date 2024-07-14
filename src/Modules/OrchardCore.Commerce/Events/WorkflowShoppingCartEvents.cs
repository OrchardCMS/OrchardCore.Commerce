using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Commerce.Abstractions;
using OrchardCore.Commerce.Abstractions.Models;
using OrchardCore.Commerce.Abstractions.ViewModels;
using OrchardCore.Commerce.Activities;
using OrchardCore.Commerce.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

/// <summary>
/// Event handler that just triggers the matching workflows.
/// </summary>
public class WorkflowShoppingCartEvents : IShoppingCartEvents
{
    private readonly IWorkflowManager _workflowManager;
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IHtmlLocalizer<WorkflowShoppingCartEvents> H;

    // After all the default events, but it should be still possible to add different ordered event handlers after it.
    public int Order => int.MaxValue / 2;

    public WorkflowShoppingCartEvents(
        IWorkflowManager workflowManager,
        IWorkflowTypeStore workflowTypeStore,
        IHtmlLocalizer<WorkflowShoppingCartEvents> localizer)
    {
        _workflowManager = workflowManager;
        _workflowTypeStore = workflowTypeStore;
        H = localizer;
    }

    public async Task<(IList<LocalizedHtmlString> Headers, IList<ShoppingCartLineViewModel> Lines)> DisplayingAsync(
        ShoppingCartDisplayingEventContext eventContext)
    {
        var results = await TriggerEventAsync<CartDisplayingEvent>(eventContext);

        if (!results.Any()) return (eventContext.Headers, eventContext.Lines);

        var headers = GetOutput<IList<SerializableLocalizedHtmlString>>(results, nameof(eventContext.Headers))?
            .Select(item => (LocalizedHtmlString)item)
            .ToList() ?? eventContext.Headers;
        var lines = GetOutput<IList<ShoppingCartLineViewModel>>(results, nameof(eventContext.Lines)) ?? eventContext.Lines;

        return (headers, lines);
    }

    public async Task<LocalizedHtmlString> VerifyingItemAsync(ShoppingCartItem item) =>
        GetOutput<SerializableLocalizedHtmlString>(
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
            ["JSON"] = JsonSerializer.Serialize(input, JOptions.Default),
        };

        // Start new workflows whose types have a corresponding starting activity.
        var workflowTypesToStart = await _workflowTypeStore.GetByStartActivityAsync(name);
        var contexts = new List<WorkflowExecutionContext>();
        foreach (var workflowType in workflowTypesToStart)
        {
            var startActivity = workflowType.Activities.First(activity => activity.IsStart && activity.Name == name);
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

        if (output is T correctTypeOutput) return correctTypeOutput;

        if (output is string text && typeof(T) == typeof(SerializableLocalizedHtmlString))
        {
            return Localize(text) as T;
        }

        var node = output switch
        {
            string json => JNode.Parse(json),
            not null => JNode.FromObject(output),
            _ => null,
        };

        if (node is null) return default;

        if (typeof(T) == typeof(IList<SerializableLocalizedHtmlString>))
        {
            return node
                .AsArray()
                .Select(item => item.GetValueKind() == JsonValueKind.String
                    ? Localize(item.GetValue<string>())
                    : item.ToObject<SerializableLocalizedHtmlString>(JOptions.Default))
                .ToList() as T;
        }

        return node.ToObject<T>(JOptions.Default);
    }

    private SerializableLocalizedHtmlString Localize(string text)
    {
        var localized = H[text];
        return new SerializableLocalizedHtmlString(localized.Name, localized.Value);
    }

    internal sealed record SerializableLocalizedHtmlString(string Name, string Value)
    {
        public static implicit operator LocalizedHtmlString(SerializableLocalizedHtmlString item) =>
            item is null ? null : new LocalizedHtmlString(item.Name ?? item.Value, item.Value ?? item.Name);
    }
}
