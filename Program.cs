using Infonetica_Task.Models;
using Infonetica_Task.Services;
using Infonetica_Task.Storage;
using Infonetica_Task.Exceptions;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<Storage>();
builder.Services.AddSingleton<ValidationService>();
builder.Services.AddSingleton<WorkflowService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

var app = builder.Build();


app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ValidationException ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Validation Error", message = ex.Message });
    }
    catch (WorkflowException ex)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Workflow Error", message = ex.Message });
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error", message = ex.Message });
    }
});


// To Create a new workflow definition
app.MapPost("/api/workflows", async (WorkflowService workflowService, WorkflowDefinition workflow) =>
{
    var result = await workflowService.CreateWorkflowAsync(workflow);
    return Results.Created($"/api/workflows/{result.Id}", result);
})
.WithName("CreateWorkflow")
.WithSummary("Create a new workflow definition")
.WithTags("Workflow Definitions");

// To Get workflow definition by ID
app.MapGet("/api/workflows/{id}", async (string id, WorkflowService workflowService) =>
{
    var workflow = await workflowService.GetWorkflowAsync(id);
    return workflow != null ? Results.Ok(workflow) : Results.NotFound($"Workflow {id} not found");
})
.WithName("GetWorkflow")
.WithSummary("Get workflow definition by ID")
.WithTags("Workflow Definitions");

// To Get all workflow definitions
app.MapGet("/api/workflows", async (WorkflowService workflowService) =>
{
    var workflows = await workflowService.GetAllWorkflowsAsync();
    return Results.Ok(workflows);
})
.WithName("GetAllWorkflows")
.WithSummary("Get all workflow definitions")
.WithTags("Workflow Definitions");

// To Update workflow definition
app.MapPut("/api/workflows/{id}", async (string id, WorkflowDefinition workflow, WorkflowService workflowService) =>
{
    var result = await workflowService.UpdateWorkflowAsync(id, workflow);
    return result != null ? Results.Ok(result) : Results.NotFound($"Workflow {id} not found");
})
.WithName("UpdateWorkflow")
.WithSummary("Update workflow definition")
.WithTags("Workflow Definitions");




// To Start a new workflow instance
app.MapPost("/api/workflows/{workflowId}/instances", async (string workflowId, WorkflowService workflowService, StartInstanceRequest? request = null) =>
{
    var instanceId = request?.InstanceId;
    var instance = await workflowService.StartInstanceAsync(workflowId, instanceId);
    return Results.Created($"/api/instances/{instance.Id}", instance);
})
.WithName("StartInstance")
.WithSummary("Start a new workflow instance")
.WithTags("Workflow Instances");

// To Get workflow instance by ID
app.MapGet("/api/instances/{id}", async (string id, WorkflowService workflowService) =>
{
    var instance = await workflowService.GetInstanceAsync(id);
    return instance != null ? Results.Ok(instance) : Results.NotFound($"Instance {id} not found");
})
.WithName("GetInstance")
.WithSummary("Get workflow instance by ID")
.WithTags("Workflow Instances");

// To Get all workflow instances
app.MapGet("/api/instances", async (WorkflowService workflowService) =>
{
    var instances = await workflowService.GetAllInstancesAsync();
    return Results.Ok(instances);
})
.WithName("GetAllInstances")
.WithSummary("Get all workflow instances")
.WithTags("Workflow Instances");

// To Get instances by workflow ID
app.MapGet("/api/workflows/{workflowId}/instances", async (string workflowId, WorkflowService workflowService) =>
{
    var instances = await workflowService.GetInstancesByWorkflowAsync(workflowId);
    return Results.Ok(instances);
})
.WithName("GetInstancesByWorkflow")
.WithSummary("Get all instances for a specific workflow")
.WithTags("Workflow Instances");

// To Execute transition on workflow instance
app.MapPost("/api/instances/{instanceId}/transitions", async (string instanceId, ExecuteTransitionRequest request, WorkflowService workflowService) =>
{
    var instance = await workflowService.ExecuteTransitionAsync(instanceId, request.TransitionId);
    return Results.Ok(new
    {
        success = true,
        instanceId = instance.Id,
        previousState = instance.History.LastOrDefault()?.FromState,
        currentState = instance.CurrentState,
        executedAt = instance.UpdatedAt,
        isCompleted = instance.IsCompleted
    });
})
.WithName("ExecuteTransition")
.WithSummary("Execute a transition on a workflow instance")
.WithTags("Workflow Instances");

app.Run();


public record StartInstanceRequest(string? InstanceId);
public record ExecuteTransitionRequest(string TransitionId);
