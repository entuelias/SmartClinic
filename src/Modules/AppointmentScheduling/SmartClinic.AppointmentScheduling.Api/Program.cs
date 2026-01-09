using System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
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
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Dev")
        .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>("Dev", _ => { });

    builder.Services.AddAuthorization(options =>
    {
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .AddAuthenticationSchemes("Dev")
            .RequireAuthenticatedUser()
            .Build();
    });
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
