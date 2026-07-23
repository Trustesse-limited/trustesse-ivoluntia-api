# Transaction PIN Setup — Design

Date: 2026-07-23
Branch: feature/transaction-pin

## Acceptance Criteria

1. PIN must be 6 digits, numbers only.
2. PIN is encrypted/hashed before storage in db.
3. Users can only set a PIN if one does not already exist.
4. Audit information is recorded when a PIN is created.
5. Include PIN setup status on login with property `HasSetUpPin` to determine whether a user has configured a transaction PIN.
6. User must set security question before setting transaction pin.

## Scope

Setup (create) only. Change/reset PIN, and PIN verification at transaction time, are explicitly out of scope for this piece of work and will be separate follow-up features (mirroring how security questions have a separate OTP-gated reset flow).

## Architecture Decision

Mirror the existing `UserSecurityQuestion` feature (the most recently built, most analogous feature in this codebase — see `Services/BusinessLogics/Service/SecurityQuestionService.cs`):

- Dedicated entity + table (not columns bolted onto `User`). The `Users` table already carries several auth-related columns (`OTP`, `OtpSubmittedTime`, `HasChangedDefaultPassword`, and an existing-but-unwired `HasSecurityQuestionsConfigured` flag that is never actually set anywhere). A new dedicated table avoids continuing that pattern.
- `HasSetUpPin` is **computed** at login by checking row existence, not read from a stored boolean. This deliberately avoids the same class of bug as the unwired `HasSecurityQuestionsConfigured` flag.
- Reuse the already-registered `IPasswordHasher<string>` (ASP.NET Core Identity's salted PBKDF2-HMAC-SHA256) for hashing — same call shape already used for security-question answers. No new hashing dependency.

Rejected alternatives:
- Columns on `User` entity — simpler, but perpetuates the existing overloaded-Users-table pattern.
- ASP.NET Identity `UserTokens` table — technically possible but no other feature in this codebase uses it; would be the only auth feature not following the custom-entity/repository/UnitOfWork convention.

## Data Model

`Domain/Entities/TransactionPin.cs`:

```csharp
public class TransactionPin : BaseEntity
{
    public string UserId { get; set; }
    public string PinHash { get; set; }
    public DateTime CreatedDate { get; set; }

    public virtual User User { get; set; } = null!;
}
```

- `BaseEntity` provides `Id`, `CreatedBy`, `DateCreated`, `DateUpdated`, `IsDeprecated`.
- Unique index on `UserId` in EF configuration (defense in depth against a race producing two rows — the service-layer "already exists" check is the primary guard).
- New EF Core migration `AddTransactionPin`; new `DbSet<TransactionPin>` on `iVoluntiaDataContext`.
- New `ITransactionPinRepository` / `TransactionPinRepository`, same shape as `ISecurityQuestionRepository`/`SecurityQuestionRepository`, registered on `IUnitOfWork` as `transactionPinRepo` (interface + implementation, wired in `UnitOfWork` constructor next to `securityQuestionRepo`).

## Validation Rules (evaluated in order — first failure returned)

1. **Authenticated user resolved** — `_currentUserService.GetUserId()` null-check, same pattern as `SecurityQuestionService`.
2. **Security question prerequisite** (criterion 6) — `_uow.userSecurityQuestionRepo.GetListByExpressionAsync(x => x.UserId == userId).Any()`. If false → 400 `"You must set up your security questions before creating a transaction PIN."`
3. **PIN not already set** (criterion 3) — `_uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == userId)`. If found → 400 `"Transaction PIN has already been set."`
4. **Format** (criterion 1) — regex `^\d{6}$` against the raw PIN string. Operates on the string directly (never parsed to int), so leading zeros (e.g. `"000123"`) are preserved and validated correctly.
5. **Confirmation match** — request carries `Pin` + `ConfirmPin`; mismatch → 400 `"PIN and confirmation do not match."`
6. **Weak-PIN blocklist** — reject if all 6 digits are identical (`000000`…`999999`), or the PIN is a 6-digit run of `"0123456789"` or `"9876543210"` (covers ascending runs like `012345`…`456789` and descending runs like `987654`…`432109`).

Only after all six checks pass: hash via `_passwordHasher.HashPassword(userId, request.Pin)` and persist. The raw PIN is never logged and never stored — only passed to the hasher call.

## DTOs

`Commons/DTOs/Auth/TransactionPinModel.cs`:

```csharp
public class SetupTransactionPinRequest
{
    public string Pin { get; set; }
    public string ConfirmPin { get; set; }
}

public class SetupTransactionPinResponse
{
    public bool PinSetupComplete { get; set; }
}
```

## Service

`ITransactionPinService` / `TransactionPinService`:

```csharp
Task<GlobalRequestReponse<SetupTransactionPinResponse>> SetupTransactionPinAsync(SetupTransactionPinRequest request)
```

Same try/catch → `ResponseHelper.BuildResponse(...)` shape as every other service in this codebase (e.g. `SecurityQuestionService`). Runs the six validation checks, then:

```csharp
var pin = new TransactionPin
{
    UserId = userId,
    PinHash = _passwordHasher.HashPassword(userId, request.Pin),
    CreatedDate = DateTime.UtcNow
};
await _uow.transactionPinRepo.AddAsync(pin);
await _uow.CompleteAsync();
```

`AuditSaveChangesInterceptor` fires automatically on `CompleteAsync()` — no bespoke audit code needed (see Audit section below).

## Controller

`iVoluntia/Controllers/v1/TransactionPinController.cs`, mirrors `SecurityQuestionsController`:

```csharp
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TransactionPinController : BaseController
{
    private readonly ITransactionPinService _transactionPinService;

    public TransactionPinController(ITransactionPinService transactionPinService)
    {
        _transactionPinService = transactionPinService;
    }

    [HttpPost("setup")]
    public async Task<IActionResult> SetupTransactionPin([FromBody] SetupTransactionPinRequest request)
        => BuildHttpResponse(await _transactionPinService.SetupTransactionPinAsync(request));
}
```

Route: `POST api/TransactionPin/setup`. `[Authorize]` only — any authenticated user manages their own PIN, no special role required.

## DI Wiring

- Register `ITransactionPinService → TransactionPinService` in `iVoluntia/Extensions/ServiceCollectionExtensions.cs`.
- Add `ITransactionPinRepository transactionPinRepo { get; set; }` to `IUnitOfWork`, implement in `UnitOfWork` constructor next to `securityQuestionRepo`.

## Login Integration (criterion 5)

Add to `Commons/DTOs/Auth/LoginRequestModel.cs`:

```csharp
public bool HasSetUpPin { get; init; }
```
(top-level on `LoginResponseModel`, alongside `HasCompletedOnboarding` — same pattern.)

In `AuthenticationService.LoginAsync`, after the user is resolved:

```csharp
var hasSetUpPin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == user.Id) != null;
```

set on the constructed `longinResponse`. Computed from the actual table at login time — not a stored/stale flag, consistent with the prerequisite check in Validation Rule 2.

## Audit (criterion 4)

No bespoke code required. `Data/Repositories/Implementation/AuditSaveChangesInterceptor .cs` already intercepts `SaveChangesAsync` and writes an `AuditLog` row for every entity in `EntityState.Added/Modified/Deleted` except `AuditLog` itself. `TransactionPin` is not excluded, so creating the row automatically produces:

- `Event = "TransactionPin Created"`
- `PerformedBy` = current user id (from `ICurrentUserRepository`)
- `NewData` = serialized entity

Since `TransactionPin` only ever holds `PinHash` (never the raw PIN), the audit trail cannot leak the plaintext PIN.

## Testing Plan

To be driven test-first when implementation starts:

- `TransactionPinService` unit tests: happy path; PIN already exists; security questions not configured; bad format (non-digit chars, wrong length); leading zeros preserved; confirm/PIN mismatch; each weak-PIN case (all-same-digit, ascending run, descending run); stored value is never equal to the raw PIN; concurrent double-submit is rejected by the unique index rather than producing a duplicate row.
- `AuthenticationService.LoginAsync` test: `HasSetUpPin` is `false` before setup and `true` after.
- EF/integration test: migration applies cleanly; unique constraint on `TransactionPin.UserId` is enforced.

## Known Pre-existing Issue (not fixed by this work, flagged for awareness)

`User.HasSecurityQuestionsConfigured` exists on the `User` entity but is never set by `SetupSecurityQuestionsAsync` — it appears to be dead/unwired. This design deliberately avoids repeating that mistake for `HasSetUpPin` by computing it from the `TransactionPin` table directly rather than introducing an equivalent stored flag. Fixing the existing flag is out of scope here.
