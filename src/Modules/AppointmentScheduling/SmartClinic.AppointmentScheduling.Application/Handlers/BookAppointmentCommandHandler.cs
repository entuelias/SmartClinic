using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SmartClinic.AppointmentScheduling.Application.Commands;
using SmartClinic.AppointmentScheduling.Domain.Entities;
using SmartClinic.AppointmentScheduling.Domain.Repositories;
using SmartClinic.AppointmentScheduling.Domain.ValueObjects;

namespace SmartClinic.AppointmentScheduling.Application.Handlers
{
    public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Guid>
    {
        private readonly IAppointmentRepository _repository;

        public BookAppointmentCommandHandler(IAppointmentRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Guid> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
        {
            var patientId = request.PatientId != Guid.Empty
                ? request.PatientId
                : request.PatientRegistered?.PatientId ?? throw new ArgumentException("PatientId is required.", nameof(request));

            if (request.AppointmentDate == default)
                throw new ArgumentException("AppointmentDate is required.", nameof(request));

            var appointmentDate = new AppointmentDate(request.AppointmentDate);
            var appointmentId = request.AppointmentId ?? Guid.NewGuid();

            var appointment = Appointment.Book(appointmentId, patientId, appointmentDate);

            await _repository.AddAsync(appointment);

            return appointment.Id;
        }
    }
}
