namespace Nexas.Application.Certificates.Queries.ValidateCertificate;

public class CertificateDetailDto
{
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int WorkloadHours { get; set; }
    public string ValidationCode { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
}
