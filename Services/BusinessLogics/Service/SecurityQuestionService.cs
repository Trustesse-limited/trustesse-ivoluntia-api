using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class SecurityQuestionService : ISecurityQuestionService
    {
        private readonly IMapper _mapper;
        private readonly iVoluntiaDataContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly IPasswordHasher<string> _passwordHasher;
        private readonly SecurityQuestionPolicy _policy;
        private readonly IUnitOfWork _uow;
        public SecurityQuestionService(
            iVoluntiaDataContext context,
            ICurrentUserService currentUserService,
            IOtpService otpService,
            IEmailService emailService,
            INotificationService notificationService,
            IPasswordHasher<string> passwordHasher,
            IOptions<SecurityQuestionPolicy> options,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _context = context;
            _currentUserService = currentUserService;
            _otpService = otpService;
            _emailService = emailService;
            _notificationService = notificationService;
            _passwordHasher = passwordHasher;
            _policy = options.Value;
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<GlobalRequestReponse<SecurityQuestionDto>> AddSecurityQuestion(CreateSecurityQuestionRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (userId == null)
                    return ResponseHelper.BuildResponse<SecurityQuestionDto>("Invalid user", StatusCodes.Status400BadRequest, null, false);

                var normalizedQuestion = request.Question.Trim().ToUpper();

                var exists = await _uow.securityQuestionRepo.GetByExpressionAsync(x => x.Question.Trim().ToUpper() == normalizedQuestion);

                if (exists != null)
                    return ResponseHelper.BuildResponse<SecurityQuestionDto>("Security question already exists", StatusCodes.Status400BadRequest, null, false);

                var question = new SecurityQuestion
                {
                    Question = request.Question.Trim(),
                    CreatedBy = userId,
                    IsActive = true,
                };

                await _uow.securityQuestionRepo.AddAsync(question);

                await _uow.CompleteAsync();

                var resultDto = _mapper.Map<SecurityQuestionDto>(question);

                return ResponseHelper.BuildResponse("Security question added successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<SecurityQuestionDto>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<IEnumerable<SecurityQuestionDto>>> GetSecurityQuestions()
        {
            try
            {
                var query = _uow.securityQuestionRepo.GetQueryable();

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<SecurityQuestionDto>>(response);

                return ResponseHelper.BuildResponse("Security questions retrieved successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<IEnumerable<SecurityQuestionDto>>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<bool>> RemoveSecurityQuestion(string questionId)
        {
            try
            {
                var question = await _uow.securityQuestionRepo.GetByExpressionAsync(x => x.Id == questionId);

                if (question == null)
                    return ResponseHelper.BuildResponse("Security question not found", StatusCodes.Status404NotFound, false, false);

                var isUsed = await _uow.userSecurityQuestionRepo.GetByExpressionAsync(x => x.SecurityQuestionId == questionId);

                if (isUsed != null)
                    return ResponseHelper.BuildResponse("Security question is in use and cannot be removed", StatusCodes.Status400BadRequest, false, false);

                question.IsActive = false;

                await _uow.CompleteAsync();

                return ResponseHelper.BuildResponse("Security question removed successfully", StatusCodes.Status200OK, true, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse(ex.Message, StatusCodes.Status500InternalServerError, false, false);
            }
        }

        public async Task<GlobalRequestReponse<SetupSecurityQuestionsResponse>> SetupSecurityQuestionsAsync(SetupSecurityQuestionsRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (userId == null)
                    return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>("Invalid user", StatusCodes.Status400BadRequest, null, false);

                const int minimumQuestions = 3;

                if (request.Questions == null || request.Questions.Count < minimumQuestions)
                    return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>($"A minimum of {minimumQuestions} security questions must be selected.", StatusCodes.Status400BadRequest, null, false);

                if (request.Questions.Any(x => string.IsNullOrWhiteSpace(x.Answer)))
                    return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>("Answers cannot be empty.", StatusCodes.Status400BadRequest, null, false);

                var alreadyConfigured = await _uow.userSecurityQuestionRepo.GetByExpressionAsync(x => x.UserId == userId);

                if (alreadyConfigured != null)
                    return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>("Security questions have already been configured.", StatusCodes.Status400BadRequest, null, false);

                var questionIds = request.Questions.Select(x => x.QuestionId).ToList();

                if (questionIds.Count != questionIds.Distinct().Count())
                    return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>("Duplicate security questions are not allowed.", StatusCodes.Status400BadRequest, null, false);

                var validQuestionCount = await _uow.securityQuestionRepo.CountAsync(x => x.IsActive && questionIds.Contains(x.Id));

                if (validQuestionCount != questionIds.Count)
                    return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>("One or more selected security questions are invalid.", StatusCodes.Status400BadRequest, null, false);

                var userQuestions = request.Questions.Select(q => CreateUserSecurityQuestion(userId, q)).ToList();

                await _uow.userSecurityQuestionRepo.AddManyAsync(userQuestions);

                await _uow.CompleteAsync();

                var resultDto = new SetupSecurityQuestionsResponse { Configured = true };

                return ResponseHelper.BuildResponse("Security questions configured successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<SetupSecurityQuestionsResponse>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
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

        public async Task<GlobalRequestReponse<ValidateSecurityQuestionsResponse>> ValidateSecurityQuestionsAsync(ValidateSecurityQuestionsRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (string.IsNullOrWhiteSpace(userId))
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("Invalid user.", StatusCodes.Status401Unauthorized, null, false);

                var now = DateTime.UtcNow;
                const int maxAttempts = 5;
                const int lockoutMinutes = 15;

                if (request.Answers == null || !request.Answers.Any())
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("At least one security question answer must be provided.", StatusCodes.Status400BadRequest, null, false);

                if (request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Answer)))
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("Answers cannot be empty.", StatusCodes.Status400BadRequest, null, false);

                if (!_policy.Rules.TryGetValue(request.Operation.ToString(), out var rule))
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("No security policy configured for this operation.", StatusCodes.Status400BadRequest, null, false);

                var attempt = await _uow.userSecurityValidationAttemptRepo.GetByExpressionAsync(x => x.UserId == userId);

                if (attempt?.LockedUntil > now)
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>($"Too many failed attempts. Try again after {attempt.LockedUntil:yyyy-MM-dd HH:mm:ss} UTC.", StatusCodes.Status423Locked, null, false);

                if (attempt == null)
                {
                    attempt = new UserSecurityValidationAttempt
                    {
                        UserId = userId,
                        AttemptCount = 0,
                        LastAttemptDate = now
                    };

                    await _uow.userSecurityValidationAttemptRepo.AddAsync(attempt);
                }

                var userSecurityQuestions = await _uow.userSecurityQuestionRepo.GetListByExpressionAsync(x => x.UserId == userId);

                if (!userSecurityQuestions.Any())
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("Security questions have not been configured.", StatusCodes.Status400BadRequest, null, false);

                var requestedQuestionIds = request.Answers.Select(x => x.QuestionId).ToList();

                if (requestedQuestionIds.Count != requestedQuestionIds.Distinct().Count())
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("Duplicate security questions are not allowed.", StatusCodes.Status400BadRequest, null, false);

                var matchedQuestions = userSecurityQuestions.Where(x => requestedQuestionIds.Contains(x.SecurityQuestionId)).ToList();

                if (matchedQuestions.Count != requestedQuestionIds.Count)
                    return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>("One or more security questions are invalid.", StatusCodes.Status400BadRequest, null, false);

                var requiredMatches = rule.MatchType == SecurityQuestionMatchType.Full ? userSecurityQuestions.Count : rule.MinimumRequiredMatches;

                if (request.Answers.Count < requiredMatches)
                {
                    attempt.AttemptCount++;
                    attempt.LastAttemptDate = now;

                    if (attempt.AttemptCount >= maxAttempts)
                        attempt.LockedUntil = now.AddMinutes(lockoutMinutes);

                    await _uow.CompleteAsync();

                    return ResponseHelper.BuildResponse("Insufficient security answers provided.", StatusCodes.Status200OK,
                        new ValidateSecurityQuestionsResponse
                        {
                            IsValid = false,
                            CanProceed = false,
                            RemainingAttempts = Math.Max(0, maxAttempts - attempt.AttemptCount)
                        }, true);
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

                    await _uow.CompleteAsync();

                    return ResponseHelper.BuildResponse("Security questions validated successfully.", StatusCodes.Status200OK,
                        new ValidateSecurityQuestionsResponse
                        {
                            IsValid = true,
                            CanProceed = true,
                            RemainingAttempts = maxAttempts
                        }, true);
                }

                attempt.AttemptCount++;
                attempt.LastAttemptDate = now;

                if (attempt.AttemptCount >= maxAttempts)
                    attempt.LockedUntil = now.AddMinutes(lockoutMinutes);

                await _uow.CompleteAsync();

                return ResponseHelper.BuildResponse("Security question validation failed.", StatusCodes.Status200OK,
                    new ValidateSecurityQuestionsResponse
                    {
                        IsValid = false,
                        CanProceed = false,
                        RemainingAttempts = Math.Max(0, maxAttempts - attempt.AttemptCount)
                    }, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<ValidateSecurityQuestionsResponse>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<string>> RequestSecurityQuestionResetAsync()
        {
            var userId = _currentUserService.GetUserId();

            var user = await _uow.userRepo.GetByExpressionAsync(x => x.Id == userId);

            if (user == null)
                return ResponseHelper.BuildResponse<string>("Invalid user.", StatusCodes.Status400BadRequest, null, false);

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

            return ResponseHelper.BuildResponse("OTP sent successfully.", StatusCodes.Status200OK, "OTP sent successfully.", true);
        }

        public async Task<GlobalRequestReponse<ResetSecurityQuestionsResponse>> ResetSecurityQuestionsAsync(ResetSecurityQuestionsRequest request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();
                var now = DateTime.UtcNow;

                const int minimumQuestions = 3;

                if (request.Questions.Count < minimumQuestions)
                    return ResponseHelper.BuildResponse<ResetSecurityQuestionsResponse>($"Minimum of {minimumQuestions} security questions required.", StatusCodes.Status400BadRequest, null, false);

                var questionIds = request.Questions.Select(x => x.QuestionId).ToList();

                if (questionIds.Count != questionIds.Distinct().Count())
                    return ResponseHelper.BuildResponse<ResetSecurityQuestionsResponse>("Duplicate security questions are not allowed.", StatusCodes.Status400BadRequest, null, false);

                var validQuestionCount = await _uow.securityQuestionRepo.CountAsync(x => x.IsActive && questionIds.Contains(x.Id));

                if (validQuestionCount != questionIds.Count)
                    return ResponseHelper.BuildResponse<ResetSecurityQuestionsResponse>("One or more security questions are invalid.", StatusCodes.Status400BadRequest, null, false);

                var otp = await _uow.otpRepo
                    .GetByExpressionAsync(x => x.UserId == userId && x.OtpCode == request.Verification.Otp && !x.IsUsed && x.ExpiresAt > now && x.Purpose == OtpPurpose.ResetSecurityQuestion.ToString());

                if (otp == null)
                    return ResponseHelper.BuildResponse<ResetSecurityQuestionsResponse>("Invalid or expired OTP.", StatusCodes.Status400BadRequest, null, false);

                await using var transaction = await _context.Database.BeginTransactionAsync();

                var existingQuestions = await _uow.userSecurityQuestionRepo.GetListByExpressionAsync(x => x.UserId == userId);

                await _uow.userSecurityQuestionRepo.DeleteManyAsync(existingQuestions);

                var newQuestions = request.Questions.Select(q => CreateUserSecurityQuestion(userId, q)).ToList();

                await _uow.userSecurityQuestionRepo.AddManyAsync(newQuestions);

                otp.IsUsed = true;

                await _uow.CompleteAsync();

                await transaction.CommitAsync();

                return ResponseHelper.BuildResponse("Security questions reset successfully.", StatusCodes.Status200OK,
                    new ResetSecurityQuestionsResponse
                    {
                        ResetSuccessful = true
                    }, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<ResetSecurityQuestionsResponse>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
            }
        }
    }
}
