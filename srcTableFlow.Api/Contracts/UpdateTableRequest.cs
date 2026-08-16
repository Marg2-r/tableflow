using System.ComponentModel.DataAnnotations;

namespace TableFlow.Api.Contracts;

public class UpdateTableRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 50)]
    public int Capacity { get; set; }

    [Required]
    [StringLength(100)]
    public string Zone { get; set; } = string.Empty;

    [Range(0, 5000)]
    public int XPosition { get; set; }

    [Range(0, 5000)]
    public int YPosition { get; set; }

    public bool IsActive { get; set; }
}