namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class NotificationTemplate : BaseEntity
    {
        public string NotificationType { get; set; }
        public string NotificationChannel { get; set; }  // e.g., "Email", "SMS"
        public string Template { get; set; }

    }

}
