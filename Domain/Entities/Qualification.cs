using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Domain.Entities;

public class Qualification : BaseEntity
{
    public string Title { get; set; }
    public string SupportingDocumentFormat { get; set; }
    public int SupportingDocumentMaxSize { get; set; }
    public FileSizeUnit SupportingDocumentFileSizeUnit { get; set; }
    public ICollection<User> Users { get; set; }    
    public ICollection<UserQualification> UserQualifications { get; set; }
}
