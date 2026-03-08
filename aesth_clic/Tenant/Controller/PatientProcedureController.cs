using aesth_clic.Tenant.Dto.PatientProcedure;
using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class PatientProcedureController
    {
        private readonly PatientProcedureService _procedureService;

        public PatientProcedureController(PatientProcedureService procedureService)
        {
            _procedureService = procedureService;
        }

        /*
        ============================================================
        CREATE PATIENT PROCEDURE (pending)
        ============================================================
        */

        public async Task AddPatientProcedureAsync(NewPatientProcedureDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            dto.ValidateRequiredFields();

            var procedure = new PatientProcedure
            {
                PatientId = dto.PatientId,
                ProcedureId = dto.ProcedureId
            };

            await _procedureService.AddPatientProcedureAsync(procedure);
        }

        /*
        ============================================================
        SCHEDULE PROCEDURE
        ============================================================
        */

        public async Task ScheduleProcedureAsync(SchedulePatientProcedureDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            dto.ValidateRequiredFields();

            await _procedureService.SchedulePatientProcedureAsync(
                dto.PatientProcedureId,
                dto.AssignedDoctorId,
                dto.AppointmentDate
            );
        }

        /*
        ============================================================
        ADD PROCEDURE DATE
        ============================================================
        */

        public async Task AddProcedureDateAsync(AddProcedureDateDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            dto.ValidateRequiredFields();

            await _procedureService.AddProcedureDateAsync(
                dto.PatientProcedureId,
                dto.ProcedureDate
            );
        }

        /*
        ============================================================
        COMPLETE PROCEDURE
        ============================================================
        */

        public async Task CompleteProcedureAsync(int procedureId)
        {
            if (procedureId <= 0)
                throw new ArgumentException("Invalid procedure id.");

            await _procedureService.CompletePatientProcedureAsync(procedureId);
        }

        /*
        ============================================================
        GET ALL
        ============================================================
        */

        public async Task<List<PatientProcedure>> GetAllPatientProceduresAsync()
        {
            return await _procedureService.GetAllPatientProceduresAsync();
        }

        /*
        ============================================================
        GET BY ID
        ============================================================
        */

        public async Task<PatientProcedure?> GetPatientProcedureByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid procedure id.");

            return await _procedureService.GetPatientProcedureByIdAsync(id);
        }

        public async Task<List<PatientProcedure>> GetDoctorAppointmentsAsync(int doctorId)
        {
            return await _procedureService.GetDoctorAppointmentsAsync(doctorId);
        }

        public async Task<List<PatientProcedure>> GetCurrentAppointmentsAsync()
        {
            return await _procedureService.GetCurrentAppointmentsAsync();
        }

        public async Task<List<PatientProcedure>> getProceduresByDoctorsId(int doctorId)
        {
            return await _procedureService.getProceduresByDoctorsId(doctorId);
        }



        /*
        ============================================================
        DELETE
        ============================================================
        */

        public async Task DeletePatientProcedureAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid procedure id.");

            await _procedureService.DeletePatientProcedureAsync(id);
        }
    }
}