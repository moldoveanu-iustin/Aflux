namespace CheltuieliApp.DTOs;

public class ImportValidationResult
{
    public ImportValidationStatus Status { get; set; }

    public DateTime? OverlapStart { get; set; }
    public DateTime? OverlapEnd { get; set; }

    public DateTime? AllowedStart { get; set; }
    public DateTime? AllowedEnd { get; set; }

    public string Message { get; set; } = "";
}

public enum ImportValidationStatus
{
    Ok,
    FullyCovered,
    PartiallyCovered
}