using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Application.Features.JobSeekers;

public class JobSeekerCourseService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration) : IJobSeekerCourseService
{
    // Same defaults/config keys as JobSeekerProfileService's document upload — one shared
    // "what can a job seeker attach" policy across the profile.
    private static readonly string[] DefaultAllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/msword",
    ];
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;

    public async Task<ServiceResult<List<JobSeekerCourseDto>>> GetMyCoursesAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<List<JobSeekerCourseDto>>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        var courses = await uow.Repository<JobSeekerCourse>().ListAsync(c => c.JobSeekerId == profile.Id && c.IsActive, ct: ct);
        return ServiceResult<List<JobSeekerCourseDto>>.Success(
            courses.OrderByDescending(c => c.CreationDate).Select(ToDto).ToList());
    }

    public async Task<ServiceResult<JobSeekerCourseDto>> CreateAsync(Guid userId, CreateJobSeekerCourseDto dto, CancellationToken ct = default)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return ServiceResult<JobSeekerCourseDto>.NotFound("لا يوجد ملف باحث عن عمل لهذا المستخدم.");

        var course = new JobSeekerCourse
        {
            Id = Guid.NewGuid(),
            JobSeekerId = profile.Id,
            Title = dto.Title.Trim(),
            Institution = dto.Institution.Trim(),
            WhatLearned = dto.WhatLearned?.Trim(),
            CreatedBy = userId,
        };
        await uow.Repository<JobSeekerCourse>().AddAsync(course, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerCourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<JobSeekerCourseDto>> UpdateAsync(Guid userId, Guid id, UpdateJobSeekerCourseDto dto, CancellationToken ct = default)
    {
        var course = await FindOwnCourseAsync(userId, id, ct);
        if (course is null) return ServiceResult<JobSeekerCourseDto>.NotFound("الكورس غير موجود.");

        course.Title = dto.Title.Trim();
        course.Institution = dto.Institution.Trim();
        course.WhatLearned = dto.WhatLearned?.Trim();
        course.ModifiedBy = userId;
        course.ModifiedDate = DateTime.UtcNow;
        uow.Repository<JobSeekerCourse>().Update(course);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<JobSeekerCourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var course = await FindOwnCourseAsync(userId, id, ct);
        if (course is null) return ServiceResult.NotFound("الكورس غير موجود.");

        course.IsActive = false;
        course.ModifiedBy = userId;
        course.ModifiedDate = DateTime.UtcNow;
        uow.Repository<JobSeekerCourse>().Update(course);
        await uow.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<JobSeekerCourseDto>> UploadCertificateAsync(
        Guid userId, Guid id, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default)
    {
        var course = await FindOwnCourseAsync(userId, id, ct);
        if (course is null) return ServiceResult<JobSeekerCourseDto>.NotFound("الكورس غير موجود.");

        var allowedMimeTypes = configuration.GetSection("Uploads:AllowedMimeTypes").Get<string[]>() ?? DefaultAllowedMimeTypes;
        if (!allowedMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<JobSeekerCourseDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ PDF وJPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<JobSeekerCourseDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var previousStorageKey = course.CertificateStorageKey;

        course.CertificateStorageKey = await fileStorage.SaveAsync(content, originalFileName, contentType, ct);
        course.CertificateOriginalFileName = originalFileName;
        course.CertificateContentType = contentType;
        course.ModifiedBy = userId;
        course.ModifiedDate = DateTime.UtcNow;
        uow.Repository<JobSeekerCourse>().Update(course);
        await uow.SaveChangesAsync(ct);

        if (previousStorageKey is not null)
            await fileStorage.DeleteAsync(previousStorageKey, ct);

        return ServiceResult<JobSeekerCourseDto>.Success(ToDto(course));
    }

    public async Task<ServiceResult<JobSeekerDocumentFileDto>> GetCertificateAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var course = await FindOwnCourseAsync(userId, id, ct);
        if (course is null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound("الكورس غير موجود.");
        if (course.CertificateStorageKey is null) return ServiceResult<JobSeekerDocumentFileDto>.NotFound("لا توجد شهادة مرفوعة لهذا الكورس.");

        var stream = await fileStorage.OpenReadAsync(course.CertificateStorageKey, ct);
        return ServiceResult<JobSeekerDocumentFileDto>.Success(new JobSeekerDocumentFileDto
        {
            Content = stream,
            FileName = course.CertificateOriginalFileName ?? "certificate",
            ContentType = course.CertificateContentType ?? "application/octet-stream",
        });
    }

    private async Task<JobSeekerCourse?> FindOwnCourseAsync(Guid userId, Guid id, CancellationToken ct)
    {
        var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.UserId == userId, ct: ct);
        if (profile is null) return null;

        return await uow.Repository<JobSeekerCourse>().FindAsync(c => c.Id == id && c.JobSeekerId == profile.Id && c.IsActive, ct: ct);
    }

    private static JobSeekerCourseDto ToDto(JobSeekerCourse c) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Institution = c.Institution,
        WhatLearned = c.WhatLearned,
        HasCertificate = c.CertificateStorageKey != null,
        CertificateFileName = c.CertificateOriginalFileName,
        CreationDate = c.CreationDate,
    };
}
