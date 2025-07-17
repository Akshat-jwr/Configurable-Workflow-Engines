namespace Infonetica_Task.Models
{
    public class WorkflowDefinition // Contains ID, Name, Description, States, Transitions, CreatedAt, UpdatedAt, Version and IsActive
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<State> States { get; set; } = new();
        public List<Transition> Transitions { get; set; } = new(); 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int Version { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }
}
