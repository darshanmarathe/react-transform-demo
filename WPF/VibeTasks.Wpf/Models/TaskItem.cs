namespace VibeTasks.Wpf.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsArchived { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceInterval? RecurrenceInterval { get; set; }
    public int? RecurrenceCount { get; set; }
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public string AssignedTo => AssignedUser?.Name ?? "Unassigned";
}

public enum TaskItemStatus
{
    Todo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum RecurrenceInterval
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}
