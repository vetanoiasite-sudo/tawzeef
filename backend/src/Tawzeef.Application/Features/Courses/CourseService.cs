using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Courses;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Courses;

public class CourseService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration) : ICourseService
{
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;
    private static readonly string[] AllowedImageMimeTypes = ["image/jpeg", "image/png"];

    public async Task<ServiceResult<PagedResult<CourseDto>>> ListForOfficeAsync(Guid officeId, CourseQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<Course>().PageAsync(
            pageIndex, pageSize,
            c => c.OfficeId == officeId && (string.IsNullOrWhiteSpace(query.Keyword) || c.Title.Contains(query.Keyword)),
            orderBy: c => c.CreationDate, descending: true, ct: ct);

        return ServiceResult<PagedResult<CourseDto>>.Success(new PagedResult<CourseDto>
        {
            Items = page.Items.Select(ToDto).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<CourseDto>> GetByIdForOfficeAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (course is null) return ServiceResult<CourseDto>.NotFound("الكورس غير موجود.");
        return ServiceResult<CourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<CourseDto>> CreateAsync(Guid officeId, Guid createdBy, CreateCourseDto dto, CancellationToken ct = default)
    {
        var course = new Course
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            ContactPhone = dto.ContactPhone.Trim(),
            Status = PublishStatus.Active,
            CreatedBy = createdBy,
        };
        await uow.Repository<Course>().AddAsync(course, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<CourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<CourseDto>> UpdateAsync(Guid officeId, Guid id, UpdateCourseDto dto, CancellationToken ct = default)
    {
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (course is null) return ServiceResult<CourseDto>.NotFound("الكورس غير موجود.");

        course.Title = dto.Title.Trim();
        course.Description = dto.Description?.Trim();
        course.ContactPhone = dto.ContactPhone.Trim();
        uow.Repository<Course>().Update(course);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<CourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<CourseDto>> UploadImageAsync(
        Guid officeId, Guid id, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default)
    {
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (course is null) return ServiceResult<CourseDto>.NotFound("الكورس غير موجود.");

        if (!AllowedImageMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<CourseDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ JPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<CourseDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var previousStorageKey = course.ImageStorageKey;

        course.ImageStorageKey = await fileStorage.SaveAsync(content, originalFileName, contentType, ct);
        course.ImageOriginalFileName = originalFileName;
        course.ImageContentType = contentType;
        uow.Repository<Course>().Update(course);
        await uow.SaveChangesAsync(ct);

        if (previousStorageKey is not null)
            await fileStorage.DeleteAsync(previousStorageKey, ct);

        return ServiceResult<CourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<CourseDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (course is null) return ServiceResult<CourseDto>.NotFound("الكورس غير موجود.");
        if (course.Status != PublishStatus.Active)
            return ServiceResult<CourseDto>.Conflict("الكورس متوقف بالفعل.");

        course.Status = PublishStatus.Paused;
        uow.Repository<Course>().Update(course);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<CourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<CourseDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id && c.OfficeId == officeId, ct: ct);
        if (course is null) return ServiceResult<CourseDto>.NotFound("الكورس غير موجود.");
        if (course.Status != PublishStatus.Paused)
            return ServiceResult<CourseDto>.Conflict("الكورس نشط بالفعل.");

        course.Status = PublishStatus.Active;
        uow.Repository<Course>().Update(course);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<CourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<PagedResult<PublicCourseListItemDto>>> SearchPublicAsync(CourseQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;

        var page = await uow.Repository<Course>().PageAsync(
            pageIndex, pageSize,
            c => c.Status == PublishStatus.Active && (string.IsNullOrWhiteSpace(query.Keyword) || c.Title.Contains(query.Keyword)),
            orderBy: c => c.CreationDate, descending: true, ct: ct);

        var officeNamesById = await LoadOfficeNamesAsync(page.Items.Select(c => c.OfficeId), ct);

        return ServiceResult<PagedResult<PublicCourseListItemDto>>.Success(new PagedResult<PublicCourseListItemDto>
        {
            Items = page.Items.Select(c => new PublicCourseListItemDto
            {
                Id = c.Id,
                Title = c.Title,
                ImageUrl = c.ImageStorageKey is null ? null : $"/api/courses/{c.Id}/image",
                OfficeName = officeNamesById.GetValueOrDefault(c.OfficeId, string.Empty),
            }).ToList(),
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }

    public async Task<ServiceResult<PublicCourseDto>> GetPublicByIdAsync(Guid id, CancellationToken ct = default)
    {
        // Same anonymity rule as a job ad: a paused course's id confirms nothing to an anonymous
        // caller — plain 404, same as "not found" (API_DOCUMENTATION.md §3.7's pattern).
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id && c.Status == PublishStatus.Active, ct: ct);
        if (course is null) return ServiceResult<PublicCourseDto>.NotFound("Course not found.");

        var office = await uow.Repository<EmploymentOffice>().FindAsync(o => o.Id == course.OfficeId, ct: ct);

        return ServiceResult<PublicCourseDto>.Success(new PublicCourseDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            ContactPhone = course.ContactPhone,
            ImageUrl = course.ImageStorageKey is null ? null : $"/api/courses/{course.Id}/image",
            OfficeName = office?.ArName ?? office?.Name ?? string.Empty,
        });
    }

    public async Task<ImageFileDto?> GetImageFileAsync(Guid id, CancellationToken ct = default)
    {
        var course = await uow.Repository<Course>().FindAsync(c => c.Id == id, ct: ct);
        if (course?.ImageStorageKey is null) return null;

        var content = await fileStorage.OpenReadAsync(course.ImageStorageKey, ct);
        return new ImageFileDto
        {
            Content = content,
            FileName = course.ImageOriginalFileName ?? "course",
            ContentType = course.ImageContentType ?? "application/octet-stream",
        };
    }

    private async Task<Dictionary<Guid, string>> LoadOfficeNamesAsync(IEnumerable<Guid> officeIds, CancellationToken ct)
    {
        var ids = officeIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        var offices = await uow.Repository<EmploymentOffice>().ListAsync(o => ids.Contains(o.Id), ct: ct);
        return offices.ToDictionary(o => o.Id, o => o.ArName ?? o.Name);
    }

    private static CourseDto ToDto(Course c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Description = c.Description,
        ContactPhone = c.ContactPhone,
        ImageUrl = c.ImageStorageKey is null ? null : $"/api/courses/{c.Id}/image",
        Status = c.Status.ToString(),
        CreationDate = c.CreationDate,
    };
}
