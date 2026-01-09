using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartClinic.PatientManagement.Domain.Repositories;
using SmartClinic.PatientManagement.Domain.ValueObjects;
using SmartClinic.PatientManagement.Domain.Entities;
using SmartClinic.PatientManagement.Application.Commands;

namespace SmartClinic.PatientManagement.Application.Handlers
{
    public class RegisterPatientCommandHandler : IRequestHandler<RegisterPatientCommand, Guid>
    {
        private readonly IPatientRepository _repository;

        public RegisterPatientCommandHandler(IPatientRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Guid> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
        {
            var email = new Email(request.Email);
            var id = Guid.NewGuid();

            var patient = Patient.Register(id, request.FullName, email, request.DateOfBirth);

            await _repository.AddAsync(patient);

            return patient.Id;
        }
    }
}
