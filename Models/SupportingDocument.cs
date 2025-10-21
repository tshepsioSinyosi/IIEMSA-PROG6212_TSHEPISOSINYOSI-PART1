// Models/SupportingDocument.cs
using System.ComponentModel.DataAnnotations;

public class SupportingDocument
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ClaimId { get; set; } // FK

    [Required]
    public string FileName { get; set; } // original filename

    [Required]
    public string StoredFileName { get; set; } // GUID name on disk

    [Required]
    public string FilePath { get; set; } // absolute or relative path

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public Claim Claim { get; set; }
}
