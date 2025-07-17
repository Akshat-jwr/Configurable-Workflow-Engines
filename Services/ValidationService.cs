using Infonetica_Task.Models;
using Infonetica_Task.Exceptions;

namespace Infonetica_Task.Services
{
    public class ValidationService
    {
        public async Task ValidateWorkflowDefinitionAsync(WorkflowDefinition workflow)
        {
            await Task.Run(() => ValidateWorkflowDefinition(workflow));
        }

        public async Task ValidateWorkflowUpdateAsync(WorkflowDefinition existingWorkflow, WorkflowDefinition updatedWorkflow)
        {
            await Task.Run(() => ValidateWorkflowUpdate(existingWorkflow, updatedWorkflow));
        }

        public async Task ValidateTransitionExecutionAsync(WorkflowDefinition workflow, WorkflowInstance instance, string transitionId)
        {
            await Task.Run(() => ValidateTransitionExecution(workflow, instance, transitionId));
        }

        private void ValidateWorkflowDefinition(WorkflowDefinition workflow)
        {
            if (string.IsNullOrEmpty(workflow.Id))
                throw new ValidationException("Workflow ID cannot be empty");

            if (string.IsNullOrEmpty(workflow.Name))
                throw new ValidationException("Workflow name cannot be empty");

            if (workflow.States == null || !workflow.States.Any())
                throw new ValidationException("Workflow must have at least one state");

            if (workflow.Transitions == null)
                workflow.Transitions = new List<Transition>();

            // For Unique state IDs
            var stateIds = workflow.States.Select(s => s.Id).ToList();
            var duplicateStates = stateIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateStates.Any())
                throw new ValidationException($"Duplicate state IDs found: {string.Join(", ", duplicateStates)}");

            // Validating to ensure only one initial state
            var initialStates = workflow.States.Where(s => s.IsInitial).ToList();
            if (initialStates.Count == 0)
                throw new ValidationException("Workflow must have exactly one initial state");
            if (initialStates.Count > 1)
                throw new ValidationException("Workflow cannot have multiple initial states");

            // Validating non-empty state IDs and names
            foreach (var state in workflow.States)
            {
                if (string.IsNullOrEmpty(state.Id))
                    throw new ValidationException("State ID cannot be empty");
                if (string.IsNullOrEmpty(state.Name))
                    throw new ValidationException($"State name cannot be empty for state {state.Id}");
            }

            // Validating unique transition IDs
            var transitionIds = workflow.Transitions.Select(t => t.Id).ToList();
            var duplicateTransitions = transitionIds.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key);
            if (duplicateTransitions.Any())
                throw new ValidationException($"Duplicate transition IDs found: {string.Join(", ", duplicateTransitions)}");

            // Validating transitions reference valid states
            foreach (var transition in workflow.Transitions)
            {
                if (string.IsNullOrEmpty(transition.Id))
                    throw new ValidationException("Transition ID cannot be empty");
                if (string.IsNullOrEmpty(transition.Name))
                    throw new ValidationException($"Transition name cannot be empty for transition {transition.Id}");

                if (!stateIds.Contains(transition.ToState))
                    throw new ValidationException($"Transition {transition.Id} references invalid target state: {transition.ToState}");

                foreach (var fromState in transition.FromStates)
                {
                    if (!stateIds.Contains(fromState))
                        throw new ValidationException($"Transition {transition.Id} references invalid source state: {fromState}");
                }

                if (!transition.FromStates.Any())
                    throw new ValidationException($"Transition {transition.Id} must have at least one source state");
            }
        }

        private void ValidateWorkflowUpdate(WorkflowDefinition existingWorkflow, WorkflowDefinition updatedWorkflow)
        {
            ValidateWorkflowDefinition(updatedWorkflow);
        }

        private void ValidateTransitionExecution(WorkflowDefinition workflow, WorkflowInstance instance, string transitionId)
        {
            // To Find the transition
            var transition = workflow.Transitions.FirstOrDefault(t => t.Id == transitionId);
            if (transition == null)
                throw new ValidationException($"Transition {transitionId} not found in workflow definition");

            // Checking if transition is enabled
            if (!transition.Enabled)
                throw new ValidationException($"Transition {transitionId} is disabled");

            // Check for current state is in fromStates
            if (!transition.FromStates.Contains(instance.CurrentState))
                throw new ValidationException($"Transition {transitionId} cannot be executed from current state {instance.CurrentState}. Valid states: {string.Join(", ", transition.FromStates)}");

            // Check if current state is final - We cannot execute transitions from final states
            var currentState = workflow.States.FirstOrDefault(s => s.Id == instance.CurrentState);
            if (currentState?.IsFinal == true)
                throw new ValidationException($"Cannot execute transitions on final state {instance.CurrentState}");

            // To Check if target state exists and is enabled
            var targetState = workflow.States.FirstOrDefault(s => s.Id == transition.ToState);
            if (targetState == null)
                throw new ValidationException($"Target state {transition.ToState} not found");
            if (!targetState.Enabled)
                throw new ValidationException($"Target state {transition.ToState} is disabled");
        }
    }
}
