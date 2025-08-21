namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class NotificationChannel : BaseEntity
    {
        public string ChannelName { get; set; }

        public ICollection<NotificationChannelSettings> ChannelSettings { get; set; } = new List<NotificationChannelSettings>();

    }

}
