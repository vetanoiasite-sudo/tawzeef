using Tawzeef.Domain.Entities.JobAdvertisements;

namespace Tawzeef.Application.Features.JobAdvertisements;

internal static class JobAdvertisementMapping
{
    public static JobAdvertisement ApplyFrom(this JobAdvertisement ad, CreateJobAdvertisementDto dto)
    {
        ad.Title = dto.Title.Trim();
        ad.Description = dto.Description?.Trim();
        ad.ExperienceRequirement = dto.ExperienceRequirement?.Trim();
        ad.WorkingHours = dto.WorkingHours?.Trim();
        ad.Address = dto.Address?.Trim();
        ad.Category = dto.Category?.Trim();
        ad.City = dto.City?.Trim();
        ad.EmploymentType = dto.EmploymentType;
        ad.SalaryMin = dto.SalaryMin;
        ad.SalaryMax = dto.SalaryMax;
        ad.VacancyCount = dto.VacancyCount <= 0 ? 1 : dto.VacancyCount;
        return ad;
    }

    public static JobAdvertisementListItemDto ToListItemDto(this JobAdvertisement ad, string? employerDisplayName = null) => new()
    {
        Id = ad.Id,
        Title = ad.Title,
        City = ad.City,
        Category = ad.Category,
        Status = ad.Status.ToString(),
        VacancyCount = ad.VacancyCount,
        SubmittedAt = ad.SubmittedAt,
        PublishedAt = ad.PublishedAt,
        ExpiresAt = ad.ExpiresAt,
        EmployerDisplayName = employerDisplayName,
    };

    public static JobAdvertisementDto ToDto(this JobAdvertisement ad, AdvertisementPayment? payment = null, string? employerDisplayName = null) => new()
    {
        Id = ad.Id,
        Title = ad.Title,
        Description = ad.Description,
        ExperienceRequirement = ad.ExperienceRequirement,
        WorkingHours = ad.WorkingHours,
        Address = ad.Address,
        Category = ad.Category,
        City = ad.City,
        EmploymentType = ad.EmploymentType?.ToString(),
        SalaryMin = ad.SalaryMin,
        SalaryMax = ad.SalaryMax,
        VacancyCount = ad.VacancyCount,
        Status = ad.Status.ToString(),
        SubmittedAt = ad.SubmittedAt,
        ApprovedAt = ad.ApprovedAt,
        RejectionReason = ad.RejectionReason,
        PublishedAt = ad.PublishedAt,
        ExpiresAt = ad.ExpiresAt,
        ClosedAt = ad.ClosedAt,
        RenewedFromAdvertisementId = ad.RenewedFromAdvertisementId,
        EmployerDisplayName = employerDisplayName,
        Payment = payment?.ToDto(),
    };

    public static AdvertisementPaymentDto ToDto(this AdvertisementPayment p) => new()
    {
        PaymentStatus = p.PaymentStatus.ToString(),
        Amount = p.Amount,
        Currency = p.Currency,
        PaymentMethod = p.PaymentMethod,
        PaidAt = p.PaidAt,
    };
}
