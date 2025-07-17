using Infonetica_Task.Models;
using Infonetica_Task.Storage;
using Infonetica_Task.Exceptions;

namespace Infonetica_Task.Services
{
    public class WorkflowService
    {
        private readonly Storage.Storage _storage;
        private readonly ValidationService _validationService;

        public WorkflowService(Storage.Storage storage, ValidationService validationService)
        {
            _storage = storage;
            _validationService = validationService;
        }

        public async Task<WorkflowDefinition> CreateWorkflowAsync(WorkflowDefinition workflow)
        {
            await _validationService.ValidateWorkflowDefinitionAsync(workflow);
            
            if (await _storage.WorkflowExistsAsync(workflow.Id))
                throw new ValidationException($"Workflow with ID {workflow.Id} already exists");

            return await _storage.SaveWorkflowAsync(workflow);
        }

        public async Task<WorkflowDefinition?> GetWorkflowAsync(string id)
        {
            return await _storage.GetWorkflowAsync(id);
        }

        public async Task<List<WorkflowDefinition>> GetAllWorkflowsAsync()
        {
            return await _storage.GetAllWorkflowsAsync();
        }

        public async Task<WorkflowDefinition?> UpdateWorkflowAsync(string id, WorkflowDefinition updatedWorkflow)
        {
            var existingWorkflow = await _storage.GetWorkflowAsync(id);
            if (existingWorkflow == null)
                throw new WorkflowException($"Workflow with ID {id} not found");

            await _validationService.ValidateWorkflowUpdateAsync(existingWorkflow, updatedWorkflow);
            
            return await _storage.UpdateWorkflowAsync(id, updatedWorkflow);
        }

        // Workflow Instance operations
        public async Task<WorkflowInstance> StartInstanceAsync(string workflowId, string? instanceId = null)
        {
            var workflow = await _storage.GetWorkflowAsync(workflowId);
            if (workflow == null)
                throw new WorkflowException($"Workflow definition {workflowId} not found");

            var initialState = workflow.States.FirstOrDefault(s => s.IsInitial);
            if (initialState == null)
                throw new WorkflowException($"No initial state found in workflow {workflowId}");

            var instance = new WorkflowInstance
            {
                Id = instanceId ?? Guid.NewGuid().ToString(),
                WorkflowDefinitionId = workflowId,
                CurrentState = initialState.Id,
                History = new List<HistoryEntry>()
            };

            if (await _storage.InstanceExistsAsync(instance.Id))
                throw new ValidationException($"Instance with ID {instance.Id} already exists");

            return await _storage.SaveInstanceAsync(instance);
        }

        public async Task<WorkflowInstance?> GetInstanceAsync(string id)
        {
            return await _storage.GetInstanceAsync(id);
        }

        public async Task<List<WorkflowInstance>> GetAllInstancesAsync()
        {
            return await _storage.GetAllInstancesAsync();
        }

        public async Task<List<WorkflowInstance>> GetInstancesByWorkflowAsync(string workflowId)
        {
            return await _storage.GetInstancesByWorkflowAsync(workflowId);
        }

        public async Task<WorkflowInstance> ExecuteTransitionAsync(string instanceId, string transitionId)
        {
            var instance = await _storage.GetInstanceAsync(instanceId);
            if (instance == null)
                throw new WorkflowException($"Instance {instanceId} not found");

            var workflow = await _storage.GetWorkflowAsync(instance.WorkflowDefinitionId);
            if (workflow == null)
                throw new WorkflowException($"Workflow definition {instance.WorkflowDefinitionId} not found");

            await _validationService.ValidateTransitionExecutionAsync(workflow, instance, transitionId);

            var transition = workflow.Transitions.First(a => a.Id == transitionId);
            var previousState = instance.CurrentState;

            instance.CurrentState = transition.ToState;
            instance.UpdatedAt = DateTime.UtcNow;

            var historyEntry = new HistoryEntry
            {
                WorkflowInstanceId = instance.Id,
                TransitionId = transition.Id,
                TransitionName = transition.Name,
                FromState = previousState,
                ToState = transition.ToState,
                Timestamp = DateTime.UtcNow
            };
            instance.History.Add(historyEntry);


            var targetState = workflow.States.First(s => s.Id == transition.ToState);
            if (targetState.IsFinal)
            {
                instance.IsCompleted = true;
                instance.CompletedAt = DateTime.UtcNow;
            }

            return await _storage.SaveInstanceAsync(instance);
        }
    }
}
