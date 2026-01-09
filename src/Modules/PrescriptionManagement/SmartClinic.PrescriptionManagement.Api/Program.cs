using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MediatR;
using SmartClinic.PrescriptionManagement.Api.Dtos;
using SmartClinic.PrescriptionManagement.Application.Commands;
using SmartClinic.PrescriptionManagement.Domain.Repositories;
using SmartClinic.PrescriptionManagement.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args: args ?? new string[0]);

// Register MediatR scanning the Application assembly
builder.Services.AddMediatR(typeof(CreatePrescriptionCommand).Assembly);

// Register DbContext with InMemory database for testing
builder.Services.AddDbContext<PrescriptionDbContext>(options =>
    options.UseInMemoryDatabase("PrescriptionManagementDb"));

// Register repository
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();

// Add authentication and authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/clinic-realm";
    options.RequireHttpsMetadata = false;
    options.Audience = "clinic-client";
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Keycloak:Authority"] ?? "http://localhost:8080/realms/clinic-realm",
        ValidateLifetime = true
    };
});
builder.Services.AddAuthorization();

// Swagger/OpenAPI for minimal API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// POST /prescriptions - Create a new prescription
app.MapPost("/prescriptions", [Authorize] async (CreatePrescriptionRequest request, IMediator mediator) =>
{
    // Map API DTO to Application Command
    var command = new CreatePrescriptionCommand
    {
        AppointmentId = request.AppointmentId,
        Medications = request.Medications.Select(m => new PrescriptionItemDto
        {
            MedicationName = m.MedicationName,
            Dosage = m.Dosage,
            Quantity = m.Quantity
        }).ToList(),
        Notes = request.Notes
    };

    var prescriptionId = await mediator.Send(command);
    return Results.Created($"/prescriptions/{prescriptionId}", new { id = prescriptionId });
})
.RequireAuthorization()
.WithName("CreatePrescription")
.WithTags("Prescriptions")
.Produces<Guid>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized);

app.Run();