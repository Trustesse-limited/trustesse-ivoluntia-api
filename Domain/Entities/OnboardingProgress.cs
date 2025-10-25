namespace Trustesse.Ivoluntia.Domain.Entities
{
    public class OnboardingProgress : BaseEntity
    {
        public int TotalPages { get; set; }
        public int LastCompletedPage { get; set; }
        public bool HasCompletedOnboarding { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
