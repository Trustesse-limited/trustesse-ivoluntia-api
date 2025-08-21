namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class NotificationPriority : BaseEntity
    {
        public string Description { get; set; }
        public int PriorityValue { get; set; } // e.g., 1 for High, 2 for Medium, 3 for Low


    }

}
