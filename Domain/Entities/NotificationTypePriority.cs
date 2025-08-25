namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class NotificationTypePriority : BaseEntity
    {
        public string NotificationType { get; set; }
        public int NotificationPriority { get; set; }
    }
}
