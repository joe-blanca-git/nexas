namespace Nexas.Application.Certificates.DTOs;

public class MyCertificateDto
{
    public string Hash { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int Workload { get; set; }
    public string CompletionDate { get; set; } = string.Empty;
    public string Status { get; set; } = "Concluído";
    public string? CoverImage { get; set; }
}

public class MyCertificatesResponseDto
{
    public int TotalCertificates { get; set; }
    public int TotalHours { get; set; }
    public List<MyCertificateDto> Certificates { get; set; } = new();
}
