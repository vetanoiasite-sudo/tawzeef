using Microsoft.AspNetCore.Mvc;

namespace Tawzeef.Api.Controllers;

/// <summary>Same reasoning as UploadJobSeekerDocumentRequest — an IFormFile shape, so it stays in
/// Api, not Application (BACKEND_GUIDELINES.md §9).</summary>
public class UploadJobSeekerCourseCertificateRequest
{
    public IFormFile File { get; set; } = null!;
}
