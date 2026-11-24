using Microsoft.Extensions.Configuration;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Courses;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Courses;

public class BannerService(IUnitOfWork uow, IFileStorage fileStorage, IConfiguration configuration) : IBannerService
{
    private const long DefaultMaxFileSizeBytes = 2 * 1024 * 1024;
    private static readonly string[] AllowedImageMimeTypes = ["image/jpeg", "image/png"];

    public async Task<ServiceResult<List<OfficeBannerDto>>> ListForOfficeAsync(Guid officeId, CancellationToken ct = default)
    {
        var banners = await uow.Repository<OfficeBanner>().ListAsync(b => b.OfficeId == officeId, ct: ct);
        return ServiceResult<List<OfficeBannerDto>>.Success(
            banners.OrderByDescending(b => b.CreationDate).Select(ToDto).ToList());
    }

    public async Task<ServiceResult<OfficeBannerDto>> CreateAsync(
        Guid officeId, Guid createdBy, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default)
    {
        if (!AllowedImageMimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            return ServiceResult<OfficeBannerDto>.Invalid("نوع الملف غير مدعوم — يُسمح فقط بـ JPG وPNG.");

        var maxSizeBytes = configuration.GetValue<long?>("Uploads:MaxFileSizeBytes") ?? DefaultMaxFileSizeBytes;
        if (fileSizeBytes <= 0 || fileSizeBytes > maxSizeBytes)
            return ServiceResult<OfficeBannerDto>.Invalid($"حجم الملف كبير جدًا — الحد الأقصى {maxSizeBytes / (1024 * 1024)} ميجابايت.");

        var storageKey = await fileStorage.SaveAsync(content, originalFileName, contentType, ct);

        var banner = new OfficeBanner
        {
            Id = Guid.NewGuid(),
            OfficeId = officeId,
            ImageStorageKey = storageKey,
            ImageOriginalFileName = originalFileName,
            ImageContentType = contentType,
            Status = PublishStatus.Active,
            CreatedBy = createdBy,
        };
        await uow.Repository<OfficeBanner>().AddAsync(banner, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OfficeBannerDto>.Success(ToDto(banner));
    }

    public async Task<ServiceResult<OfficeBannerDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var banner = await uow.Repository<OfficeBanner>().FindAsync(b => b.Id == id && b.OfficeId == officeId, ct: ct);
        if (banner is null) return ServiceResult<OfficeBannerDto>.NotFound("البانر غير موجود.");
        if (banner.Status != PublishStatus.Active)
            return ServiceResult<OfficeBannerDto>.Conflict("البانر متوقف بالفعل.");

        banner.Status = PublishStatus.Paused;
        uow.Repository<OfficeBanner>().Update(banner);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OfficeBannerDto>.Success(ToDto(banner));
    }

    public async Task<ServiceResult<OfficeBannerDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default)
    {
        var banner = await uow.Repository<OfficeBanner>().FindAsync(b => b.Id == id && b.OfficeId == officeId, ct: ct);
        if (banner is null) return ServiceResult<OfficeBannerDto>.NotFound("البانر غير موجود.");
        if (banner.Status != PublishStatus.Paused)
            return ServiceResult<OfficeBannerDto>.Conflict("البانر نشط بالفعل.");

        banner.Status = PublishStatus.Active;
        uow.Repository<OfficeBanner>().Update(banner);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OfficeBannerDto>.Success(ToDto(banner));
    }

    public async Task<ServiceResult<List<PublicBannerDto>>> ListPublicAsync(CancellationToken ct = default)
    {
        var banners = await uow.Repository<OfficeBanner>().ListAsync(b => b.Status == PublishStatus.Active, ct: ct);
        return ServiceResult<List<PublicBannerDto>>.Success(
            banners.OrderByDescending(b => b.CreationDate)
                .Select(b => new PublicBannerDto { Id = b.Id, ImageUrl = $"/api/banners/{b.Id}/image" })
                .ToList());
    }

    public async Task<ImageFileDto?> GetImageFileAsync(Guid id, CancellationToken ct = default)
    {
        var banner = await uow.Repository<OfficeBanner>().FindAsync(b => b.Id == id, ct: ct);
        if (banner is null) return null;

        var content = await fileStorage.OpenReadAsync(banner.ImageStorageKey, ct);
        return new ImageFileDto
        {
            Content = content,
            FileName = banner.ImageOriginalFileName,
            ContentType = banner.ImageContentType,
        };
    }

    private static OfficeBannerDto ToDto(OfficeBanner b) => new()
    {
        Id = b.Id,
        ImageUrl = $"/api/banners/{b.Id}/image",
        Status = b.Status.ToString(),
        CreationDate = b.CreationDate,
    };
}
