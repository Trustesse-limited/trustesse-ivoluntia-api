namespace Trustesse.Ivoluntia.Commons.DTOs.DataTransferObjects
{
    public class ComposeNotificationDto
    {
        public string NotificationType { get; set; }
        public string NotificationChannel { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }
    }
}
