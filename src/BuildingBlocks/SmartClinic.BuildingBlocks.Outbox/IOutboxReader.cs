using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmartClinic.BuildingBlocks.Outbox
{
    public interface IOutboxReader
    {
        Task<IReadOnlyCollection<OutboxMessage>> GetUnprocessedMessagesAsync(int batchSize, CancellationToken cancellationToken = default);
        Task MarkAsProcessedAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }
}
