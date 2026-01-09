using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using SmartClinic.BuildingBlocks.Outbox;

namespace SmartClinic.BuildingBlocks.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class OutboxPublishingJob : IJob
    {
        private readonly IEnumerable<IOutboxReader> _outboxReaders;
        private readonly IOutboxPublisher _outboxPublisher;
        private readonly ILogger<OutboxPublishingJob> _logger;

        public OutboxPublishingJob(
            IEnumerable<IOutboxReader> outboxReaders,
            IOutboxPublisher outboxPublisher,
            ILogger<OutboxPublishingJob> logger)
        {
            _outboxReaders = outboxReaders;
            _outboxPublisher = outboxPublisher;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Starting outbox publishing job.");

            foreach (var reader in _outboxReaders)
            {
                try
                {
                    var messages = await reader.GetUnprocessedMessagesAsync(20, context.CancellationToken);
                    
                    if (messages.Count == 0)
                    {
                        continue;
                    }

                    _logger.LogInformation("Found {Count} unprocessed messages.", messages.Count);

                    foreach (var message in messages)
                    {
                        try
                        {
                            await _outboxPublisher.PublishAsync(message, context.CancellationToken);
                            await reader.MarkAsProcessedAsync(message, context.CancellationToken);
                            _logger.LogInformation("Published and marked processed message {MessageId}.", message.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to publish message {MessageId}.", message.Id);
                            // We do not abort the job, try next message
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing outbox reader {ReaderType}.", reader.GetType().Name);
                }
            }

            _logger.LogInformation("Completed outbox publishing job.");
        }
    }
}
