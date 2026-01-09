using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartClinic.AppointmentScheduling.Domain.Entities;
using SmartClinic.BuildingBlocks.Domain;
using SmartClinic.BuildingBlocks.Outbox;

namespace SmartClinic.AppointmentScheduling.Infrastructure.Persistence
{
    public class AppointmentDbContext : DbContext
    {
        public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>(builder =>
            {
                builder.ToTable("Appointments");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Id).IsRequired();
                builder.Property(x => x.PatientId).IsRequired();
                
                builder.OwnsOne(x => x.AppointmentDate, d =>
                {
                    d.Property(p => p.Value).HasColumnName("AppointmentDate");
                });
                
                builder.Ignore(x => x.DomainEvents);
            });

            modelBuilder.Entity<OutboxMessage>(builder =>
            {
                builder.ToTable("OutboxMessages");
                builder.HasKey(x => x.Id);
                builder.Property(x => x.Type).IsRequired().HasMaxLength(500);
                builder.Property(x => x.Payload).IsRequired();
                builder.Property(x => x.OccurredOn).IsRequired();
                builder.Property(x => x.ProcessedOn);
                builder.Property(x => x.Error).HasMaxLength(2000);
                builder.HasIndex(x => new { x.ProcessedOn, x.OccurredOn })
                    .HasDatabaseName("IX_OutboxMessages_ProcessedOn_OccurredOn");
            });
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            var domainEvents = ExtractDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                var message = OutboxMessage.FromDomainEvent(domainEvent);
                OutboxMessages.Add(message);
            }

            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private IReadOnlyCollection<IDomainEvent> ExtractDomainEvents()
        {
            var domainEvents = new List<IDomainEvent>();

            foreach (var entry in ChangeTracker.Entries())
            {
                var entity = entry.Entity;
                if (entity == null) continue;

                var entityType = entity.GetType();
                var domainEventsProperty = entityType.GetProperty("DomainEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (domainEventsProperty == null) continue;

                var eventsValue = domainEventsProperty.GetValue(entity);
                if (eventsValue == null) continue;

                IEnumerable<object> eventsEnumerable = eventsValue switch
                {
                    System.Collections.IEnumerable enumerable => enumerable.Cast<object>(),
                    _ => Enumerable.Empty<object>()
                };

                foreach (var evt in eventsEnumerable)
                {
                    if (evt is IDomainEvent domainEvent)
                    {
                        domainEvents.Add(domainEvent);
                    }
                }

                var clearMethod = entityType.GetMethod("ClearDomainEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                clearMethod?.Invoke(entity, null);
            }

            return domainEvents;
        }
    }
}
