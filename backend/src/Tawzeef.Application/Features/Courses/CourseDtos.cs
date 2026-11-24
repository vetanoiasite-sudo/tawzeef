using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.Courses;

public class CreateCourseDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string ContactPhone { get; set; } = string.Empty;
}

/// <summary>Full-replace PUT, same convention as every other profile-update DTO in this
/// codebase.</summary>
public class UpdateCourseDto : CreateCourseDto;

/// <summary>Office-side detail/list item — includes Status, since the office needs to see its own
/// paused courses too (the public endpoints never return a paused one at all).</summary>
public class CourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
}

/// <summary>Anonymous public listing/detail — no Status (every row returned is already Active),
/// adds OfficeName since knowing which employment office is behind a course matters on a
/// trust-mediated platform.</summary>
public class PublicCourseListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string OfficeName { get; set; } = string.Empty;
}

public class PublicCourseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string OfficeName { get; set; } = string.Empty;
}

public class CourseQueryDto
{
    public string? Keyword { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 20;
}
