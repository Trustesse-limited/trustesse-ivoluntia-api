namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class NotificationChannelSettings : BaseEntity
    {
        public string NotificationChannelId { get; set; }
        public NotificationChannel NotificationChannel { get; set; }
        public string Provider { get; set; }
        public string Settings { get; set; }
        public bool? IsActive { get; set; } = false;
    }
}
