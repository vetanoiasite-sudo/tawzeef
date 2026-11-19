using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.Interviews;

/// <summary>Creating this record is how the office marks a JobApplication "hired" —
/// API_DOCUMENTATION.md §3.9's POST route is annotated "create, on marking hired".</summary>
public class CreateHiringFollowUpDto
{
    /// <summary>Defaults to now if omitted — lets the office backdate a hire entered late.</summary>
    public DateTime? HiredAt { get; set; }
}

public class CompleteHiringFollowUpDto
{
    public bool? StillEmployed { get; set; }
    public bool? EmployerSatisfied { get; set; }
    public bool? JobSeekerSatisfied { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}

public class HiringFollowUpDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public DateTime HiredAt { get; set; }
    public DateTime DueAt { get; set; }

    /// <summary>Overdue is computed at read time (PendingDue past DueAt) — no background job exists
    /// to flip the stored column, same convention as JobAdvertisement expiry.</summary>
    public string Status { get; set; } = string.Empty;

    public bool? StillEmployed { get; set; }
    public bool? EmployerSatisfied { get; set; }
    public bool? JobSeekerSatisfied { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class HiringFollowUpListItemDto
{
    public Guid Id { get; set; }
    public Guid JobApplicationId { get; set; }
    public string JobSeekerName { get; set; } = string.Empty;
    public string EmployerDisplayName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateTime HiredAt { get; set; }
    public DateTime DueAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class HiringFollowUpQueryDto
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}
