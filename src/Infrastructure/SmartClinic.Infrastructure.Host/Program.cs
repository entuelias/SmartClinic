using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using RabbitMQ.Client;
using SmartClinic.AppointmentScheduling.Infrastructure.Persistence;
using SmartClinic.BuildingBlocks.BackgroundJobs;
using SmartClinic.BuildingBlocks.Messaging;
using SmartClinic.BuildingBlocks.Outbox;
using SmartClinic.PatientManagement.Infrastructure.Persistence;
using SmartClinic.PrescriptionManagement.Infrastructure.Persistence;

var builder = Host.CreateApplicationBuilder(args);

// 1. Register DbContexts
// Using InMemory for demonstration/verification purposes as per instructions.
// In production, these would use SQL Server connection strings.
builder.Services.AddDbContext<PatientDbContext>(options =>
    options.UseInMemoryDatabase("PatientDb"));

builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseInMemoryDatabase("AppointmentDb"));

builder.Services.AddDbContext<PrescriptionDbContext>(options =>
    options.UseInMemoryDatabase("PrescriptionDb"));

// 2. Register Outbox Readers (one per module)
builder.Services.AddTransient<IOutboxReader, EntityFrameworkOutboxReader<PatientDbContext>>();
builder.Services.AddTransient<IOutboxReader, EntityFrameworkOutboxReader<AppointmentDbContext>>();
builder.Services.AddTransient<IOutboxReader, EntityFrameworkOutboxReader<PrescriptionDbContext>>();

// 3. Register Outbox Publisher (RabbitMQ)
builder.Services.AddSingleton<IConnectionFactory>(sp => new ConnectionFactory
{
    HostName = "localhost",
    DispatchConsumersAsync = true
});
builder.Services.AddTransient<IOutboxPublisher, RabbitMQOutboxPublisher>();

// 4. Configure Quartz
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("OutboxPublishingJob");
    q.AddJob<OutboxPublishingJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("OutboxPublishingTrigger")
        .WithSimpleSchedule(x => x
            .WithIntervalInSeconds(5)
            .RepeatForever()));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var host = builder.Build();
host.Run();
