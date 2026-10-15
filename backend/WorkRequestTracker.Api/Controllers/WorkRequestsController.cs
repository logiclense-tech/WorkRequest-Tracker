using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Data;
using WorkRequestTracker.Api.DTOs;
using WorkRequestTracker.Api.Models;

namespace WorkRequestTracker.Api.Controllers;

[ApiController]
[Route("api/work-requests")]
public sealed class WorkRequestsController(WorkRequestDbContext db, ILogger<WorkRequestsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResponse<WorkRequestResponseDto>>> GetList(
        [FromQuery] string? status, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(ApiError("invalid_pagination", "page must be >= 1 and pageSize must be between 1 and 100."));

        var query = db.WorkRequests.AsNoTracking().Include(x => x.Notes).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<WorkRequestStatus>(status, true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus))
                return BadRequest(ApiError("invalid_status", "Status must be New, InProgress, Blocked, or Completed."));
            query = query.Where(x => x.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.ClientName.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var entities = await query.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);
        var items = entities.Select(ToDto).ToList();

        return Ok(new PagedResponse<WorkRequestResponseDto>(items, page, pageSize, total));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkRequestResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await db.WorkRequests.AsNoTracking().Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item is null ? NotFound(ApiError("not_found", $"Work request {id} was not found.")) : Ok(ToDto(item));
    }

    [HttpPost]
    public async Task<ActionResult<WorkRequestResponseDto>> Create(
        [FromBody] CreateWorkRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.ClientName))
            return BadRequest(ApiError("validation_error", "Title and client name are required."));
        if (request.Priority is null || !Enum.IsDefined(request.Priority.Value))
            return BadRequest(ApiError("invalid_priority", "Priority must be Low, Medium, or High."));
        if (request.Status is null || !Enum.IsDefined(request.Status.Value))
            return BadRequest(ApiError("invalid_status", "Status must be New, InProgress, Blocked, or Completed."));
        if (request.DueDate is null)
            return BadRequest(ApiError("validation_error", "Due date is required."));

        var item = new WorkRequest
        {
            Title = request.Title.Trim(),
            ClientName = request.ClientName.Trim(),
            Description = request.Description?.Trim() ?? "",
            Priority = request.Priority.Value,
            Status = request.Status.Value,
            DueDate = DateTime.SpecifyKind(request.DueDate.Value, DateTimeKind.Utc),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        db.WorkRequests.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToDto(item));
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<WorkRequestResponseDto>> UpdateStatus(
        int id, [FromBody] UpdateStatusDto request, CancellationToken cancellationToken)
    {
        var item = await db.WorkRequests.Include(x => x.Notes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound(ApiError("not_found", $"Work request {id} was not found."));
        if (request.Status is null || !Enum.IsDefined(request.Status.Value))
            return BadRequest(ApiError("invalid_status", "Status must be New, InProgress, Blocked, or Completed."));

        item.Status = request.Status.Value;
        item.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(item));
    }

    [HttpPost("{id:int}/notes")]
    public async Task<ActionResult<NoteResponseDto>> AddNote(
        int id, [FromBody] AddNoteDto request, CancellationToken cancellationToken)
    {
        var item = await db.WorkRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (item is null) return NotFound(ApiError("not_found", $"Work request {id} was not found."));
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(ApiError("validation_error", "Note text is required."));

        var note = new WorkRequestNote
        {
            WorkRequestId = id,
            Text = request.Text.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        db.WorkRequestNotes.Add(note);
        item.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/work-requests/{id}", new NoteResponseDto(note.Id, note.Text, note.CreatedAtUtc));
    }

    private static WorkRequestResponseDto ToDto(WorkRequest x) =>
        new(x.Id, x.Title, x.ClientName, x.Description, x.Priority.ToString(), x.Status.ToString(),
            x.DueDate, x.CreatedAtUtc, x.UpdatedAtUtc,
            x.Notes.OrderByDescending(n => n.CreatedAtUtc)
                .Select(n => new NoteResponseDto(n.Id, n.Text, n.CreatedAtUtc)).ToList());

    private static object ApiError(string code, string message) =>
        new { error = new { code, message, traceId = Guid.NewGuid().ToString("N") } };
}
