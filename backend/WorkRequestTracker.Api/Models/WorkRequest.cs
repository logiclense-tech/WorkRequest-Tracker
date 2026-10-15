namespace WorkRequestTracker.Api.Models;

public enum WorkRequestPriority { Low, Medium, High }
public enum WorkRequestStatus { New, InProgress, Blocked, Completed }

public sealed class WorkRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string Description { get; set; } = "";
    public WorkRequestPriority Priority { get; set; }
    public WorkRequestStatus Status { get; set; } = WorkRequestStatus.New;
    public DateTime DueDate { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<WorkRequestNote> Notes { get; set; } = [];
}

public sealed class WorkRequestNote
{
    public int Id { get; set; }
    public int WorkRequestId { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public WorkRequest? WorkRequest { get; set; }
}
