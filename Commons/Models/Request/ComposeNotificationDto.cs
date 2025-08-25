namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class ComposeNotificationDto
    {
        public string NotificationType { get; set; }
        public string NotificationChannel { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
    }
}
