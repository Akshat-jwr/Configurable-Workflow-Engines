using Infonetica_Task.Models;
using System.Collections.Concurrent;

namespace Infonetica_Task.Storage
{
    public class Storage
    {
        private readonly ConcurrentDictionary<string, WorkflowDefinition> _workflows = new();
        private readonly ConcurrentDictionary<string, WorkflowInstance> _instances = new();

        public async Task<WorkflowDefinition> SaveWorkflowAsync(WorkflowDefinition workflow)
        {
            workflow.UpdatedAt = DateTime.UtcNow;
            _workflows[workflow.Id] = workflow;
            return await Task.FromResult(workflow);
        }

        public async Task<WorkflowDefinition?> GetWorkflowAsync(string id)
        {
            _workflows.TryGetValue(id, out var workflow);
            return await Task.FromResult(workflow);
        }

        public async Task<List<WorkflowDefinition>> GetAllWorkflowsAsync()
        {
            return await Task.FromResult(_workflows.Values.Where(w => w.IsActive).ToList());
        }

        public async Task<bool> WorkflowExistsAsync(string id)
        {
            return await Task.FromResult(_workflows.ContainsKey(id) && _workflows[id].IsActive);
        }

        public async Task<WorkflowDefinition?> UpdateWorkflowAsync(string id, WorkflowDefinition updatedWorkflow)
        {
            if (_workflows.TryGetValue(id, out var existingWorkflow))
            {
                updatedWorkflow.Id = id;
                updatedWorkflow.CreatedAt = existingWorkflow.CreatedAt;
                updatedWorkflow.UpdatedAt = DateTime.UtcNow;
                updatedWorkflow.Version = existingWorkflow.Version + 1;
                
                _workflows[id] = updatedWorkflow;
                return await Task.FromResult(updatedWorkflow);
            }
            return await Task.FromResult<WorkflowDefinition?>(null);
        }

        public async Task<WorkflowInstance> SaveInstanceAsync(WorkflowInstance instance)
        {
            instance.UpdatedAt = DateTime.UtcNow;
            _instances[instance.Id] = instance;
            return await Task.FromResult(instance);
        }

        public async Task<WorkflowInstance?> GetInstanceAsync(string id)
        {
            _instances.TryGetValue(id, out var instance);
            return await Task.FromResult(instance);
        }

        public async Task<List<WorkflowInstance>> GetAllInstancesAsync()
        {
            return await Task.FromResult(_instances.Values.ToList());
        }

        public async Task<List<WorkflowInstance>> GetInstancesByWorkflowAsync(string workflowId)
        {
            return await Task.FromResult(_instances.Values
                .Where(i => i.WorkflowDefinitionId == workflowId)
                .ToList());
        }

        public async Task<bool> InstanceExistsAsync(string id)
        {
            return await Task.FromResult(_instances.ContainsKey(id));
        }

        public async Task<WorkflowInstance?> UpdateInstanceAsync(string id, WorkflowInstance updatedInstance)
        {
            if (_instances.TryGetValue(id, out var existingInstance))
            {
                updatedInstance.Id = id;
                updatedInstance.CreatedAt = existingInstance.CreatedAt;
                updatedInstance.UpdatedAt = DateTime.UtcNow;
                
                _instances[id] = updatedInstance;
                return await Task.FromResult(updatedInstance);
            }
            return await Task.FromResult<WorkflowInstance?>(null);
        }
    }
}
