using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartClinic.BuildingBlocks.Outbox;

namespace SmartClinic.BuildingBlocks.BackgroundJobs
{
    public class EntityFrameworkOutboxReader<TDbContext> : IOutboxReader
        where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public EntityFrameworkOutboxReader(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyCollection<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<OutboxMessage>()
                .Where(m => m.ProcessedOn == null)
                .OrderBy(m => m.OccurredOn)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsProcessedAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            message.ProcessedOn = DateTime.UtcNow;
            // Ensure the message is tracked
            if (_dbContext.Entry(message).State == EntityState.Detached)
            {
                _dbContext.Set<OutboxMessage>().Attach(message);
                _dbContext.Entry(message).Property(m => m.ProcessedOn).IsModified = true;
            }
            
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
