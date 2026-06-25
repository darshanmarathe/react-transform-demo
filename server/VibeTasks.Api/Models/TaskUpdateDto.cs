namespace VibeTasks.Api.Models;

public class TaskUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public bool? IsArchived { get; set; }
    public bool? IsRecurring { get; set; }
    public RecurrenceInterval? RecurrenceInterval { get; set; }
    public int? RecurrenceCount { get; set; }
    public int? AssignedUserId { get; set; }
}
