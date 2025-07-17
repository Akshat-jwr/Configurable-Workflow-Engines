namespace Infonetica_Task.Models
{
    public class HistoryEntry // Represents a history entry for a workflow instance, tracking transitions between states.
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string TransitionId { get; set; } = string.Empty; 
        public string TransitionName { get; set; } = string.Empty;
        public string FromState { get; set; } = string.Empty;
        public string ToState { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
