using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using SmartClinic.PatientManagement.Application.Commands;

var builder = WebApplication.CreateBuilder(args: args ?? new string[0]);

// Register MediatR scanning the Application assembly
builder.Services.AddMediatR(typeof(RegisterPatientCommand).Assembly);
// Register minimal in-memory repository implementation from Infrastructure
builder.Services.AddSingleton<SmartClinic.PatientManagement.Domain.Repositories.IPatientRepository, SmartClinic.PatientManagement.Infrastructure.Persistence.PatientRepository>();

// Swagger/OpenAPI for minimal API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/patients", async (RegisterPatientDto dto, IMediator mediator) =>
{
    var cmd = new RegisterPatientCommand
    {
        FullName = dto.FullName,
        Email = dto.Email,
        DateOfBirth = dto.DateOfBirth
    };

    var id = await mediator.Send(cmd);
    return Results.Created($"/patients/{id}", new { id });
});

app.Run();

public record RegisterPatientDto(string FullName, string Email, DateTime DateOfBirth);
