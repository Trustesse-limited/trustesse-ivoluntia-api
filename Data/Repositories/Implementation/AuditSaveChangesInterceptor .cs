using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using System.Text.Json;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class AuditSaveChangesInterceptor: SaveChangesInterceptor
    {
        private readonly ICurrentUserRepository _currentUserRepository;
        public AuditSaveChangesInterceptor(ICurrentUserRepository currentUserRepository)
        {
            _currentUserRepository = currentUserRepository;
        }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context is null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            var auditLogs = new List<AuditLog>();
            var user = _currentUserRepository.GetUserId() ?? "system";
            foreach (var entry in context.ChangeTracker.Entries() 
                         .Where(e =>
                             e.Entity is not AuditLog &&
                             e.State != EntityState.Detached &&
                             e.State != EntityState.Unchanged))
            {
                var audit = new AuditLog
                {
                    EventDate = DateTime.UtcNow,
                    PerformedBy = user
                };
                switch (entry.State)
                {
                    case EntityState.Added:
                        audit.Event = $"{entry.Metadata.ClrType.Name} Created";
                        audit.NewData = JsonSerializer.Serialize(
                            entry.CurrentValues.ToObject());
                        break;
                    case EntityState.Modified:
                        audit.Event = $"{entry.Metadata.ClrType.Name} Updated";
                        audit.OldData = JsonSerializer.Serialize(
                            entry.OriginalValues.ToObject());
                        audit.NewData = JsonSerializer.Serialize(
                            entry.CurrentValues.ToObject());
                        break;
                    case EntityState.Deleted:
                        audit.Event = $"{entry.Metadata.ClrType.Name} Deleted";
                        audit.OldData = JsonSerializer.Serialize(
                            entry.OriginalValues.ToObject());
                        break;
                }
                auditLogs.Add(audit);
            }
            if (auditLogs.Count > 0)
            {
                context.Set<AuditLog>().AddRange(auditLogs);
            }
            return base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }
    }
}
