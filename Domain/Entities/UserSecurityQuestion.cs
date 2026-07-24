namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class UserSecurityQuestion : BaseEntity
    {
        public string UserId { get; set; }
        public string SecurityQuestionId { get; set; }
        public string AnswerHash { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual SecurityQuestion SecurityQuestion { get; set; } = null!;
    }
}
