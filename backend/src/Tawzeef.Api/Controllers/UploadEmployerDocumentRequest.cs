using Microsoft.AspNetCore.Mvc;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Api.Controllers;

/// <summary>An ASP.NET Core model-binding shape (IFormFile), so it can't live in Application —
/// BACKEND_GUIDELINES.md §9: Application never touches IFormFile directly. Kept in Api, own file,
/// next to the one controller that binds it.</summary>
public class UploadEmployerDocumentRequest
{
    public EmployerDocumentType DocumentType { get; set; }
    public IFormFile File { get; set; } = null!;
}
