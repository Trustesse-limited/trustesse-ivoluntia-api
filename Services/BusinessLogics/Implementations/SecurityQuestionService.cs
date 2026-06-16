using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class SecurityQuestionService : ISecurityQuestionService
    {
        private readonly IMapper _mapper;
        private readonly ISecurityQuestionRepository _securityQuestionRepository;
        private readonly iVoluntiaDataContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly SecurityQuestionPolicy _policy;
        public SecurityQuestionService(
            ISecurityQuestionRepository securityQuestionRepository,
            iVoluntiaDataContext context,
            ICurrentUserService currentUserService,
            IOtpService otpService,
            IEmailService emailService,
            INotificationService notificationService,
            IPasswordHasher<string> passwordHasher,
            IOptions<SecurityQuestionPolicy> options,
            IMapper mapper)
        {
            _securityQuestionRepository = securityQuestionRepository;
            _context = context;
            _currentUserService = currentUserService;
            _otpService = otpService;
            _emailService = emailService;
            _notificationService = notificationService;
            _passwordHasher = passwordHasher;
            _policy = options.Value;
            _mapper = mapper;
        }

        public async Task<ApiResponse<SecurityQuestionDto>> AddSecurityQuestion(CreateSecurityQuestionRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (userId == null)
                    return ApiResponse<SecurityQuestionDto>.Failure(StatusCodes.Status400BadRequest, "Invalid user");

                var normalizedQuestion = request.Question.Trim().ToUpper();

                var exists = await _context.SecurityQuestions
                    .AnyAsync(x => x.Question.Trim().ToUpper() == normalizedQuestion);

                if (exists)
                    return ApiResponse<SecurityQuestionDto>.Failure(StatusCodes.Status400BadRequest, "Security question already exists");

                var question = new SecurityQuestion
                {
                    Question = request.Question.Trim(),
                    CreatedBy = userId,
                    IsActive = true,
                };

                await _securityQuestionRepository.AddSecurityQuestion(question);

                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<SecurityQuestionDto>(question);

                return ApiResponse<SecurityQuestionDto>.Success("Security question added successfully", resultDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<SecurityQuestionDto>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions()
        {
            try
            {
                var query = _securityQuestionRepository.GetSecurityQuestions();

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<SecurityQuestionDto>>(response);

                return ApiResponse<IEnumerable<SecurityQuestionDto>>.Success("Security questions retrieved successfully", resultDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<SecurityQuestionDto>>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> RemoveSecurityQuestion(string questionId)
        {
            try
            {
                var question = await _securityQuestionRepository.GetSecurityQuestions().FirstOrDefaultAsync(x => x.Id == questionId);

                if (question == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Security question not found");

                var isUsed = await _context.UserSecurityQuestions.AnyAsync(x => x.SecurityQuestionId == questionId);

                if (isUsed)
                    return ApiResponse<bool>.Failure(StatusCodes.Status400BadRequest, "Security question is in use and cannot be removed");

                question.IsActive = false;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success("Security question removed successfully", true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<SetupSecurityQuestionsResponse>> SetupSecurityQuestionsAsync(SetupSecurityQuestionsRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (userId == null)
                    return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Invalid user");

                const int minimumQuestions = 3;

                if (request.Questions == null || request.Questions.Count < minimumQuestions)
                    return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, $"A minimum of {minimumQuestions} security questions must be selected.");

                if (request.Questions.Any(x => string.IsNullOrWhiteSpace(x.Answer)))
                    return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Answers cannot be empty.");

                var alreadyConfigured = await _context.UserSecurityQuestions.AnyAsync(x => x.UserId == userId);

                if (alreadyConfigured)
                    return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Security questions have already been configured.");

                var questionIds = request.Questions.Select(x => x.QuestionId).ToList();

                if (questionIds.Count != questionIds.Distinct().Count())
                    return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Duplicate security questions are not allowed.");

                var validQuestionCount = await _context.SecurityQuestions
                   .CountAsync(x => x.IsActive && questionIds.Contains(x.Id));

                if (validQuestionCount != questionIds.Count)
                    return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "One or more selected security questions are invalid.");

                var userQuestions = request.Questions.Select(q => CreateUserSecurityQuestion(userId, q)).ToList();

                await _context.UserSecurityQuestions.AddRangeAsync(userQuestions);

                await _context.SaveChangesAsync();

                var resultDto = new SetupSecurityQuestionsResponse { Configured = true };

                return ApiResponse<SetupSecurityQuestionsResponse>.Success("Security questions configured successfully", resultDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<SetupSecurityQuestionsResponse>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private UserSecurityQuestion CreateUserSecurityQuestion(string userId, SecurityQuestionAnswerRequest question)
        {
            return new UserSecurityQuestion
            {
                UserId = userId,
                SecurityQuestionId = question.QuestionId,
                AnswerHash = _passwordHasher.HashPassword(userId, question.Answer.Trim()),
                CreatedDate = DateTime.UtcNow
            };
        }

        public async Task<ApiResponse<ValidateSecurityQuestionsResponse>> ValidateSecurityQuestionsAsync(ValidateSecurityQuestionsRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (string.IsNullOrWhiteSpace(userId))
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status401Unauthorized, "Invalid user.");

                var now = DateTime.UtcNow;
                const int maxAttempts = 5;
                const int lockoutMinutes = 15;

                if (request.Answers == null || !request.Answers.Any())
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "At least one security question answer must be provided.");

                if (request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Answer)))
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Answers cannot be empty.");

                if (!_policy.Rules.TryGetValue(request.Operation.ToString(), out var rule))
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "No security policy configured for this operation.");

                var attempt = await _context.UserSecurityValidationAttempts.FirstOrDefaultAsync(x => x.UserId == userId);

                if (attempt?.LockedUntil > now)
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status423Locked, $"Too many failed attempts. Try again after {attempt.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC.");

                if (attempt == null)
                {
                    attempt = new UserSecurityValidationAttempt
                    {
                        UserId = userId,
                        AttemptCount = 0,
                        LastAttemptDate = now
                    };

                    await _context.UserSecurityValidationAttempts.AddAsync(attempt);
                }

                var userSecurityQuestions = await _context.UserSecurityQuestions.Where(x => x.UserId == userId).ToListAsync();

                if (!userSecurityQuestions.Any())
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Security questions have not been configured.");

                var requestedQuestionIds = request.Answers.Select(x => x.QuestionId).ToList();

                if (requestedQuestionIds.Count != requestedQuestionIds.Distinct().Count())
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Duplicate security questions are not allowed.");

                var matchedQuestions = userSecurityQuestions.Where(x => requestedQuestionIds.Contains(x.SecurityQuestionId)).ToList();

                if (matchedQuestions.Count != requestedQuestionIds.Count)
                    return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "One or more security questions are invalid.");

                var requiredMatches = rule.MatchType == SecurityQuestionMatchType.Full ? userSecurityQuestions.Count : rule.MinimumRequiredMatches;

                if (request.Answers.Count < requiredMatches)
                {
                    attempt.AttemptCount++;
                    attempt.LastAttemptDate = now;

                    if (attempt.AttemptCount >= maxAttempts)
                        attempt.LockedUntil = now.AddMinutes(lockoutMinutes);

                    await _context.SaveChangesAsync();

                    return ApiResponse<ValidateSecurityQuestionsResponse>.Success("Insufficient security answers provided.",
                        new ValidateSecurityQuestionsResponse
                        {
                            IsValid = false,
                            CanProceed = false,
                            RemainingAttempts = Math.Max(0, maxAttempts - attempt.AttemptCount)
                        });
                }

                var storedQuestions = matchedQuestions.ToDictionary(x => x.SecurityQuestionId);
                var matches = 0;

                foreach (var answer in request.Answers)
                {
                    if (!storedQuestions.TryGetValue(answer.QuestionId, out var storedQuestion))
                        continue;

                    var verificationResult = _passwordHasher.VerifyHashedPassword(userId, storedQuestion.AnswerHash, answer.Answer.Trim());

                    if (verificationResult == PasswordVerificationResult.Success || verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                        matches++;
                }

                var isValid = matches >= requiredMatches;

                if (isValid)
                {
                    attempt.AttemptCount = 0;
                    attempt.LockedUntil = null;
                    attempt.LastAttemptDate = now;

                    await _context.SaveChangesAsync();

                    return ApiResponse<ValidateSecurityQuestionsResponse>.Success("Security questions validated successfully.",
                        new ValidateSecurityQuestionsResponse
                        {
                            IsValid = true,
                            CanProceed = true,
                            RemainingAttempts = maxAttempts
                        });
                }

                attempt.AttemptCount++;
                attempt.LastAttemptDate = now;

                if (attempt.AttemptCount >= maxAttempts)
                    attempt.LockedUntil = now.AddMinutes(lockoutMinutes);

                await _context.SaveChangesAsync();

                return ApiResponse<ValidateSecurityQuestionsResponse>.Success("Security question validation failed.",
                    new ValidateSecurityQuestionsResponse
                    {
                        IsValid = false,
                        CanProceed = false,
                        RemainingAttempts = Math.Max(0, maxAttempts - attempt.AttemptCount)
                    });
            }
            catch (Exception ex)
            {
                return ApiResponse<ValidateSecurityQuestionsResponse>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<string>> RequestSecurityQuestionResetAsync()
        {
            var userId = _currentUserService.GetUserId();

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid user.");

            var otpCode = await _otpService.GenerateOtpAsync(userId, OtpPurpose.ResetSecurityQuestion);

            var placeholders = new Dictionary<string, string>
            {
                { "title", "Security Question Reset" },
                { "otp", otpCode },
                { "expiry", "10 minutes" }
            };

            var otpMessage = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.OtpRequest.ToString(), NotificationChannelEnum.Email.ToString(), placeholders);

            EmailModel OtpEmailModel = new EmailModel
            {
                Receivers = [user.Email],
                Subject = "Otp Request",
                Message = HttpUtility.HtmlDecode(otpMessage.Data)
            };

            await _emailService.SendEmailASync(OtpEmailModel);

            return ApiResponse<string>.Success("OTP sent successfully.", "OTP sent successfully.");
        }

        public async Task<ApiResponse<ResetSecurityQuestionsResponse>> ResetSecurityQuestionsAsync(ResetSecurityQuestionsRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var now = DateTime.UtcNow;

                const int minimumQuestions = 3;

                if (request.Questions.Count < minimumQuestions)
                    return ApiResponse<ResetSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, $"Minimum of {minimumQuestions} security questions required.");

                var questionIds = request.Questions.Select(x => x.QuestionId).ToList();

                if (questionIds.Count != questionIds.Distinct().Count())
                    return ApiResponse<ResetSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Duplicate security questions are not allowed.");

                var validQuestionCount = await _context.SecurityQuestions.CountAsync(x => x.IsActive && questionIds.Contains(x.Id));

                if (validQuestionCount != questionIds.Count)
                    return ApiResponse<ResetSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "One or more security questions are invalid.");

                var otp = await _context.Otps.
                    FirstOrDefaultAsync(x => x.UserId == userId && x.OtpCode == request.Verification.Otp && !x.IsUsed && x.ExpiresAt > now && x.Purpose == OtpPurpose.ResetSecurityQuestion.ToString());

                if (otp == null)
                    return ApiResponse<ResetSecurityQuestionsResponse>.Failure(StatusCodes.Status400BadRequest, "Invalid or expired OTP.");

                await using var transaction = await _context.Database.BeginTransactionAsync();

                var existingQuestions = await _context.UserSecurityQuestions.Where(x => x.UserId == userId).ToListAsync();

                _context.UserSecurityQuestions.RemoveRange(existingQuestions);

                var newQuestions = request.Questions.Select(q => CreateUserSecurityQuestion(userId, q)).ToList();

                await _context.UserSecurityQuestions.AddRangeAsync(newQuestions);

                otp.IsUsed = true;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ApiResponse<ResetSecurityQuestionsResponse>.Success("Security questions reset successfully.",
                    new ResetSecurityQuestionsResponse
                    {
                        ResetSuccessful = true
                    });
            }
            catch (Exception ex)
            {
                return ApiResponse<ResetSecurityQuestionsResponse>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
