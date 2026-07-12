namespace Trustesse.Ivoluntia.Commons.Models.Request
{
    public class ComposeNotificationDto
    {
        public string NotificationType { get; set; }
        public string NotificationChannel { get; set; }
        public Dictionary<string, string> Placeholders { get; set; }

        public ComposeNotificationDto Validate()
        {
            if (this == null)
                throw new Exception("Invalid Request");
            if (string.IsNullOrWhiteSpace(NotificationType))
                throw new Exception("NotificationType should not be null");
            if (string.IsNullOrWhiteSpace(NotificationChannel))
                throw new Exception("NotificationChannel should not be null");

            return this;
        }
    }
}
