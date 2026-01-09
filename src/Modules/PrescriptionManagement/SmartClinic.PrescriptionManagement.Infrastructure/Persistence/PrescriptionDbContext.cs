using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartClinic.BuildingBlocks.Domain;
using SmartClinic.BuildingBlocks.Outbox;
using SmartClinic.PrescriptionManagement.Domain.Aggregates;
using SmartClinic.PrescriptionManagement.Domain.ValueObjects;

namespace SmartClinic.PrescriptionManagement.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core DbContext for the PrescriptionManagement module.
    /// Responsible for persisting aggregates and transactional outbox messages.
    /// </summary>
    public class PrescriptionDbContext : DbContext
    {
        public PrescriptionDbContext(DbContextOptions<PrescriptionDbContext> options)
            : base(options)
        {
        }

        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Prescription aggregate
            modelBuilder.Entity<Prescription>(builder =>
            {
                builder.ToTable("Prescriptions");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Id)
                    .IsRequired();

                builder.Property(x => x.AppointmentId)
                    .IsRequired();

                builder.Property(x => x.Notes)
                    .HasMaxLength(1000);

                // Configure PrescriptionItem as owned entity (value object)
                builder.OwnsMany(x => x.Medications, medicationBuilder =>
                {
                    medicationBuilder.ToTable("PrescriptionItems");
                    medicationBuilder.WithOwner().HasForeignKey("PrescriptionId");

                    medicationBuilder.Property(m => m.MedicationName)
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnName("MedicationName");

                    medicationBuilder.Property(m => m.Dosage)
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnName("Dosage");

                    medicationBuilder.Property(m => m.Quantity)
                        .IsRequired()
                        .HasColumnName("Quantity");
                });

                // Ignore domain events - they are persisted via OutboxMessages
                builder.Ignore(x => x.DomainEvents);
            });

            // Configure OutboxMessage
            modelBuilder.Entity<OutboxMessage>(builder =>
            {
                builder.ToTable("OutboxMessages");
                builder.HasKey(x => x.Id);

                builder.Property(x => x.Type)
                    .IsRequired()
                    .HasMaxLength(500);

                builder.Property(x => x.Payload)
                    .IsRequired();

                builder.Property(x => x.OccurredOn)
                    .IsRequired();

                builder.Property(x => x.ProcessedOn);
                builder.Property(x => x.Error)
                    .HasMaxLength(2000);

                // Index for efficient querying of unprocessed messages
                builder.HasIndex(x => new { x.ProcessedOn, x.OccurredOn })
                    .HasDatabaseName("IX_OutboxMessages_ProcessedOn_OccurredOn");
            });
        }

        /// <summary>
        /// Overrides SaveChangesAsync to capture domain events from tracked aggregates,
        /// convert them to OutboxMessages, and persist everything in a single transaction.
        /// </summary>
        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            // 1. Extract domain events from tracked entities before saving.
            var domainEvents = ExtractDomainEvents();

            // 2. Convert domain events to OutboxMessages and attach to the context.
            foreach (var domainEvent in domainEvents)
            {
                var message = OutboxMessage.FromDomainEvent(domainEvent);
                OutboxMessages.Add(message);
            }

            // 3. Persist aggregates + outbox messages atomically in a single transaction.
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken)
                .ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Extracts domain events from all tracked aggregates that have pending events.
        /// </summary>
        private IReadOnlyCollection<IDomainEvent> ExtractDomainEvents()
        {
            var domainEvents = new List<IDomainEvent>();

            foreach (var entry in ChangeTracker.Entries())
            {
                var entity = entry.Entity;
                if (entity == null)
                {
                    continue;
                }

                var entityType = entity.GetType();

                // Look for a property named "DomainEvents" that returns IReadOnlyCollection<IDomainEvent> or similar
                var domainEventsProperty = entityType.GetProperty(
                    "DomainEvents",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (domainEventsProperty == null)
                {
                    continue;
                }

                var eventsValue = domainEventsProperty.GetValue(entity);
                if (eventsValue == null)
                {
                    continue;
                }

                // Handle different collection types
                IEnumerable<object> eventsEnumerable = eventsValue switch
                {
                    System.Collections.IEnumerable enumerable => enumerable.Cast<object>(),
                    _ => Enumerable.Empty<object>()
                };

                // Filter to only IDomainEvent instances
                foreach (var evt in eventsEnumerable)
                {
                    if (evt is IDomainEvent domainEvent)
                    {
                        domainEvents.Add(domainEvent);
                    }
                }

                // Clear domain events on the aggregate if it exposes a ClearDomainEvents method.
                var clearMethod = entityType.GetMethod(
                    "ClearDomainEvents",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                clearMethod?.Invoke(entity, null);
            }

            return domainEvents;
        }
    }
}

