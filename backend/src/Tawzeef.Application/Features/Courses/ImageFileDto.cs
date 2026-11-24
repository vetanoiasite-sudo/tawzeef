namespace Tawzeef.Application.Features.Courses;

/// <summary>Shared by Course and OfficeBanner image streaming — same shape as
/// EmployerDocumentFileDto, local to this feature to avoid a cross-feature dependency.</summary>
public class ImageFileDto
{
    public Stream Content { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}
