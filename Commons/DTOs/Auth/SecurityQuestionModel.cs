using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Commons.DTOs.Auth;

public class CreateSecurityQuestionRequest
{
    public string Question { get; set; }

    public CreateSecurityQuestionRequest Validate()
    {
        if (this == null)
            throw new Exception("Invalid Request");
        if (string.IsNullOrWhiteSpace(Question))
            throw new Exception("Question should not be null");

        return this;
    }
}

public class SecurityQuestionDto : CreateSecurityQuestionRequest
{
    public string Id { get; set; }
}

public class SetupSecurityQuestionsRequest
{
    public List<SecurityQuestionAnswerRequest> Questions { get; set; } = new();
}

public class SecurityQuestionAnswerRequest
{
    public string QuestionId { get; set; }
    public string Answer { get; set; }
}

public class SetupSecurityQuestionsResponse
{
    public bool Configured { get; set; }
}

public class ValidateSecurityQuestionsRequest
{
    public SecurityQuestionOperation Operation { get; set; }
    public List<SecurityQuestionValidationRequest> Answers { get; set; } = new();
}

public class SecurityQuestionValidationRequest
{
    public string QuestionId { get; set; }
    public string Answer { get; set; }
}

public class ValidateSecurityQuestionsResponse
{
    public bool IsValid { get; set; }
    public bool CanProceed { get; set; }
    public int RemainingAttempts { get; set; }
}

public class SecurityQuestionPolicy
{
    public Dictionary<string, SecurityQuestionRule> Rules { get; set; } = new();
}

public class SecurityQuestionRule
{
    public SecurityQuestionMatchType MatchType { get; set; }
    public int MinimumRequiredMatches { get; set; }
}

public class ResetSecurityQuestionsRequest
{
    public List<SecurityQuestionAnswerRequest> Questions { get; set; } = new();
    public Verification Verification { get; set; }

    public ResetSecurityQuestionsRequest Validate()
    {
        if (this == null)
            throw new Exception("Invalid Request");
        if (Questions == null || !Questions.Any())
            throw new Exception("Questions should not be null");
        if (Verification == null || string.IsNullOrWhiteSpace(Verification.Otp))
            throw new Exception("Verification Otp should not be null");

        return this;
    }
}

public class Verification
{
    public string Otp { get; set; }
}

public class ResetSecurityQuestionsResponse
{
    public bool ResetSuccessful { get; set; }
}



