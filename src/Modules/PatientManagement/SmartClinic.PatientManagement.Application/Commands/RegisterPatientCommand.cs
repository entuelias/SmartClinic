using System;
using MediatR;

namespace SmartClinic.PatientManagement.Application.Commands
{
    public sealed class RegisterPatientCommand : IRequest<Guid>
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime DateOfBirth { get; init; }
    }
}
