using System.ComponentModel.DataAnnotations;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.DTOs;

public sealed class CreateWorkRequestDto
{
    [Required, StringLength(150, MinimumLength = 1)]
    public string Title { get; init; } = "";
    [Required, StringLength(150, MinimumLength = 1)]
    public string ClientName { get; init; } = "";
    [Required, StringLength(2000)]
    public string Description { get; init; } = "";
    [EnumDataType(typeof(WorkRequestPriority))]
    public WorkRequestPriority? Priority { get; init; }
    [EnumDataType(typeof(WorkRequestStatus))]
    public WorkRequestStatus? Status { get; init; }
    [Required]
    public DateTime? DueDate { get; init; }
}

public sealed class UpdateStatusDto
{
    [Required, EnumDataType(typeof(WorkRequestStatus))]
    public WorkRequestStatus? Status { get; init; }
}

public sealed class AddNoteDto
{
    [Required, StringLength(1000, MinimumLength = 1)]
    public string Text { get; init; } = "";
}

public sealed record WorkRequestResponseDto(
    int Id, string Title, string ClientName, string Description,
    string Priority, string Status, DateTime DueDate,
    DateTime CreatedAtUtc, DateTime UpdatedAtUtc,
    IReadOnlyList<NoteResponseDto> Notes);

public sealed record NoteResponseDto(int Id, string Text, DateTime CreatedAtUtc);
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
