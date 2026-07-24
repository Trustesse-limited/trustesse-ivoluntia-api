namespace Trustesse.Ivoluntia.Domain.Enums
{
    public enum SecurityQuestionOperation
    {
        ACCOUNT_RECOVERY = 1,
        PIN_RESET,
        WITHDRAWAL,
    }

    public enum SecurityQuestionMatchType
    {
        Full,
        Partial
    }
}
