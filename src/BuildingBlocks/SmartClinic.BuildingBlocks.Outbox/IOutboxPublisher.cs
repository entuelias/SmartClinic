using System.Threading;
using System.Threading.Tasks;

namespace SmartClinic.BuildingBlocks.Outbox
{
    public interface IOutboxPublisher
    {
        Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }
}
