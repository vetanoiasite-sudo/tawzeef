using System.ComponentModel.DataAnnotations;

namespace Tawzeef.Application.Features.Employers;

public class RespondEmployerHiringFollowUpDto
{
    [Required]
    public bool Satisfied { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }
}
