using Lombiq.HelpfulLibraries.OrchardCore.Workflow;
using OrchardCore.Commerce.Abstractions.Abstractions;
using OrchardCore.Commerce.Activities;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Services;
using System.Threading.Tasks;

namespace OrchardCore.Commerce.Events;

public class WorkflowOrderEvents : IOrderEvents
{
    private readonly IWorkflowManager _workflowManager;

    public WorkflowOrderEvents(IWorkflowManager workflowManager) =>
        _workflowManager = workflowManager;

    public Task OrderedAsync(ContentItem order, string shoppingCartId) =>
        _workflowManager.TriggerContentItemEventAsync<OrderCreatedEvent>(order);
}
