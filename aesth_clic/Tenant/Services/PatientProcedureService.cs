using aesth_clic.Session;
using aesth_clic.Tenant.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Services
{
    public sealed class PatientProcedureService(TenantDbContextFactory tenantFactory) : TenantServiceBase
    {
        private readonly TenantDbContextFactory _tenantFactory = tenantFactory;

        /*
        ============================================================
        1. CREATE PATIENT PROCEDURE (STATUS = PENDING)
        ============================================================
        */

        public async Task AddPatientProcedureAsync(PatientProcedure newProcedure)
        {
            using var tenantDb = CreateTenantDb();

            if (newProcedure == null)
                throw new ArgumentNullException(nameof(newProcedure));

            newProcedure.Status = "pending";
            newProcedure.CreatedAt = DateTime.UtcNow;

            newProcedure.ValidateForInsert();

            tenantDb.PatientProcedures.Add(newProcedure);

            await tenantDb.SaveChangesAsync();
        }


        /*
        ============================================================
        2. SCHEDULE PROCEDURE
        sets AssignedDoctorId + AppointmentDate
        status -> scheduled
        ============================================================
        */

        public async Task SchedulePatientProcedureAsync(
            int patientProcedureId,
            int assignedDoctorId,
            DateTime appointmentDate)
        {
            using var tenantDb = CreateTenantDb();

            var procedure = await tenantDb.PatientProcedures
                .FirstOrDefaultAsync(p => p.Id == patientProcedureId);

            if (procedure == null)
                throw new Exception("Patient procedure not found.");

            if (!procedure.Status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only pending procedures can be scheduled.");

            if (assignedDoctorId <= 0)
                throw new Exception("AssignedDoctorId is required.");

            if (appointmentDate == default)
                throw new Exception("AppointmentDate is required.");

            procedure.AssignedDoctorId = assignedDoctorId;
            procedure.AppointmentDate = appointmentDate;
            procedure.Status = "scheduled";

            await tenantDb.SaveChangesAsync();
        }


        /*
        ============================================================
        3. ADD PROCEDURE DATE
        status remains scheduled
        ============================================================
        */

        public async Task AddProcedureDateAsync(int patientProcedureId, DateTime procedureDate)
        {
            using var tenantDb = CreateTenantDb();

            var procedure = await tenantDb.PatientProcedures
                .FirstOrDefaultAsync(p => p.Id == patientProcedureId);

            if (procedure == null)
                throw new Exception("Patient procedure not found.");

            if (!procedure.Status.Equals("scheduled", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Procedure must be scheduled first.");

            if (procedure.AppointmentDate == null)
                throw new Exception("AppointmentDate must exist before setting ProcedureDate.");

            if (procedureDate == default)
                throw new Exception("ProcedureDate is required.");

            if (procedureDate < procedure.AppointmentDate)
                throw new Exception("ProcedureDate cannot be earlier than AppointmentDate.");

            procedure.ProcedureDate = procedureDate;

            await tenantDb.SaveChangesAsync();
        }


        /*
        ============================================================
        4. COMPLETE PROCEDURE
        status -> completed
        ============================================================
        */

        public async Task CompletePatientProcedureAsync(int patientProcedureId)
        {
            using var tenantDb = CreateTenantDb();

            var procedure = await tenantDb.PatientProcedures
                .FirstOrDefaultAsync(p => p.Id == patientProcedureId);

            if (procedure == null)
                throw new Exception("Patient procedure not found.");

            if (!procedure.Status.Equals("scheduled", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only scheduled procedures can be completed.");

            if (procedure.ProcedureDate == null)
                throw new Exception("ProcedureDate must be set before completing.");

            procedure.Status = "completed";

            await tenantDb.SaveChangesAsync();
        }


        /*
        ============================================================
        GET ALL PROCEDURES
        ============================================================
        */

        public async Task<List<PatientProcedure>> GetAllPatientProceduresAsync()
        {
            using var tenantDb = CreateTenantDb();

            var res =  await tenantDb.PatientProcedures
                .Include(p => p.Patient)
                .Include(p => p.User)
                .Include(p => p.ServiceMenu)
                .ToListAsync();



            return res;
        }


        /*
        ============================================================
        GET PROCEDURE BY ID
        ============================================================
        */

        public async Task<PatientProcedure?> GetPatientProcedureByIdAsync(int id)
        {
            using var tenantDb = CreateTenantDb();

            var res =  await tenantDb.PatientProcedures
                .Include(p => p.Patient)
                .Include(p => p.User)
                .Include(p => p.ServiceMenu)
                .FirstOrDefaultAsync(p => p.Id == id);


            return res;
        }


        /*
        ============================================================
        GET PROCEDURES APPOINTED TO CURRENT DOCTOR
        (ProcedureDate must be null → still an appointment)
        ============================================================
        */

        public async Task<List<PatientProcedure>> GetDoctorAppointmentsAsync(int doctorId)
        {
            using var tenantDb = CreateTenantDb();

            if (doctorId <= 0)
                throw new Exception("DoctorId is required.");

            var appointments = await tenantDb.PatientProcedures
                .Include(p => p.Patient)
                .Include(p => p.User)
                .Include(p => p.ServiceMenu)
                .Where(p =>
                    p.AssignedDoctorId == doctorId &&
                    p.AppointmentDate != null &&
                    p.ProcedureDate == null &&
                    p.Status.Equals("scheduled"))
                .OrderBy(p => p.AppointmentDate)
                .ToListAsync();

            return appointments;
        }



        /*
        ============================================================
        GET ALL CURRENT APPOINTMENTS
        (AssignedDoctorId + AppointmentDate exist, ProcedureDate null)
        ============================================================
        */

        public async Task<List<PatientProcedure>> GetCurrentAppointmentsAsync()
        {
            using var tenantDb = CreateTenantDb();

            var appointments = await tenantDb.PatientProcedures
                .Include(p => p.Patient)
                .Include(p => p.User)
                .Include(p => p.ServiceMenu)
                .Where(p =>
                    p.AssignedDoctorId != null &&
                    p.AppointmentDate != null &&
                    p.ProcedureDate == null &&
                    p.Status.Equals("scheduled"))
                .OrderBy(p => p.AppointmentDate)
                .ToListAsync();

            return appointments;
        }


        /*
        ============================================================
        GET COMPLETED PROCEDURES BY DOCTOR
        (ProcedureDate is NOT null → already performed)
        ============================================================
        */

        public async Task<List<PatientProcedure>> getProceduresByDoctorsId(int doctorId)
        {
            using var tenantDb = CreateTenantDb();

            if (doctorId <= 0)
                throw new Exception("DoctorId is required.");

            var procedures = await tenantDb.PatientProcedures
                .Include(p => p.Patient)
                .Include(p => p.User)
                .Include(p => p.ServiceMenu)
                .Where(p =>
                    p.AssignedDoctorId == doctorId &&
                    p.ProcedureDate != null)
                .OrderByDescending(p => p.ProcedureDate)
                .ToListAsync();

            return procedures;
        }


        /*
        ============================================================
        DELETE PROCEDURE
        ============================================================
        */

        public async Task DeletePatientProcedureAsync(int id)
        {
            using var tenantDb = CreateTenantDb();

            var procedure = await tenantDb.PatientProcedures.FindAsync(id);

            if (procedure == null)
                throw new Exception("Patient procedure not found.");

            tenantDb.PatientProcedures.Remove(procedure);

            await tenantDb.SaveChangesAsync();
        }
    }
}