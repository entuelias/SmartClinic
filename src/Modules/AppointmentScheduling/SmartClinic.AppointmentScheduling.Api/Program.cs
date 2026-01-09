using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartClinic.AppointmentScheduling.Api.Requests;
using SmartClinic.AppointmentScheduling.Application.Commands;
using SmartClinic.AppointmentScheduling.Domain.Repositories;
using SmartClinic.AppointmentScheduling.Infrastructure.Persistence;
using SmartClinic.AppointmentScheduling.Api.Authentication;

var builder = WebApplication.CreateBuilder(args: args ?? Array.Empty<string>());

// Application wiring
builder.Services.AddMediatR(typeof(BookAppointmentCommand).Assembly);

// Infrastructure
builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseInMemoryDatabase("AppointmentDb"));

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authentication
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
})
.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>("Dev", _ => { });

builder.Services.AddAuthorization();

var app = builder.Build();

// Enable Swagger in all environments for testing purposes
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
}

app.UseAuthentication();
app.UseAuthorization();

// Minimal secured endpoints
app.MapPost("/appointments", [Authorize] async (BookAppointmentRequest req, IMediator mediator) =>
{
    var cmd = new BookAppointmentCommand
    {
        PatientId = req.PatientId,
        AppointmentDate = req.AppointmentDate
    };

    var id = await mediator.Send(cmd);
    return Results.Created($"/appointments/{id}", new { id });
})
.RequireAuthorization();

app.Run();