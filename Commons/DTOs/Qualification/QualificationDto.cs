namespace Trustesse.Ivoluntia.Commons.DTOs.Qualification;

public class CreateQualificationDto
{
    public string Title { get; set; }
    public string SupportingDocumentFormat { get; set; }
    public int SupportingDocumentMaxSize { get; set; }
    public string SupportingDocumentFileSizeUnit { get; set; }
}

public class UpdateQualificationDto
{
    public string Title { get; set; }
    public string SupportingDocumentFormat { get; set; }
    public int SupportingDocumentMaxSize { get; set; }
    public string SupportingDocumentFileSizeUnit { get; set; }
}

public class QualificationDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string SupportingDocumentFormat { get; set; }
    public int SupportingDocumentMaxSize { get; set; }
    public string SupportingDocumentFileSizeUnit { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string CreatedBy { get; set; }
    public string Status { get; set; }
}
