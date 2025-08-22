namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public bool IsSent { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public bool IsPickedUp { get; set; }
        public bool HasAttachment { get; set; }
        public string AttachmentUrl { get; set; }
        public int Priority { get; set; }
        public string MailType { get; set; }
        public int RetryCount { get; set; }
        public string Channel { get; set; }
    }
}
