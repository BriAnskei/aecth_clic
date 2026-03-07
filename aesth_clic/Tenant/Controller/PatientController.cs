using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class PatientController(PatientService patientService)
    {
        private readonly PatientService _patientService = patientService;

        // -------------------------
        // CREATE
        // -------------------------
        public async Task<Patient> CreatePatientAsync(
            string fullName,
            string gender,
            int age,
            string email,
            string address,
            string phoneNumber)
        {
            var patient = new Patient
            {
                FullName = fullName,
                Gender = gender,
                Age = age,
                Email = email,
                Address = address,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            patient.Validate();

            return await _patientService.CreateAsync(patient);
        }

        // -------------------------
        // READ ALL
        // -------------------------
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _patientService.GetAllAsync();
        }

        // -------------------------
        // READ BY ID
        // -------------------------
        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid patient id.");

            return await _patientService.GetByIdAsync(id);
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdatePatientAsync(
            int id,
            string fullName,
            string gender,
            int age,
            string email,
            string address,
            string phoneNumber)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid patient id.");

            var patient = new Patient
            {
                Id = id,
                FullName = fullName,
                Gender = gender,
                Age = age,
                Email = email,
                Address = address,
                PhoneNumber = phoneNumber
            };

            patient.Validate();

            return await _patientService.UpdateAsync(patient);
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeletePatientAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid patient id.");

            return await _patientService.DeleteAsync(id);
        }
    }
}