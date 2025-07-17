namespace Infonetica_Task.Models
{
    public class WorkflowInstance // Represents an instance of a workflow, tracking its current state, history, and completion status.
    {
        public string Id { get; set; } = string.Empty;
        public string WorkflowDefinitionId { get; set; } = string.Empty;
        public string CurrentState { get; set; } = string.Empty;
        public List<HistoryEntry> History { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; } 
        public bool IsCompleted { get; set; } = false;
    }
}
