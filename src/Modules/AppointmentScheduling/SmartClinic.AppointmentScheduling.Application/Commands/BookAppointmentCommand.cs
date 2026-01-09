using System;
using MediatR;
using SmartClinic.AppointmentScheduling.Application.DTOs;

namespace SmartClinic.AppointmentScheduling.Application.Commands
{
    public sealed class BookAppointmentCommand : IRequest<Guid>
    {
        public Guid PatientId { get; init; }
        public DateTime AppointmentDate { get; init; }
        public Guid? AppointmentId { get; init; }
        public PatientRegisteredDto? PatientRegistered { get; init; }

        public static BookAppointmentCommand FromPatientRegistered(PatientRegisteredDto patient, DateTime appointmentDate, Guid? appointmentId = null)
        {
            if (patient == null) throw new ArgumentNullException(nameof(patient));

            return new BookAppointmentCommand
            {
                AppointmentId = appointmentId,
                PatientId = patient.PatientId,
                AppointmentDate = appointmentDate,
                PatientRegistered = patient
            };
        }
    }
}
