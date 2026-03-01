using System;
using System.ComponentModel.DataAnnotations;

namespace aesth_clic.Master.Model
{
    public class Client
    {
        public int Id { get; set; }

        public string ClinicName { get; set; } = string.Empty;

        public string DbName { get; set; } = string.Empty;

        public string ClinicCode { get; set; } = string.Empty;

        public string Status { get; set; } = "active"; // default active

        public string Tier { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Validate for insertion: only ClinicName and Tier
        public void ValidateForInsert()
        {
            if (string.IsNullOrWhiteSpace(ClinicName))
                throw new ValidationException("Clinic Name is required.");

            if (string.IsNullOrWhiteSpace(Tier))
                throw new ValidationException("Tier is required.");
        }

        // Validate for reading/fetching: all fields
        public void ValidateForRead()
        {
            if (string.IsNullOrWhiteSpace(ClinicName))
                throw new ValidationException("Clinic Name is required.");

            if (string.IsNullOrWhiteSpace(DbName))
                throw new ValidationException("Database Name is required.");

            if (string.IsNullOrWhiteSpace(ClinicCode))
                throw new ValidationException("Clinic Code is required.");

            if (string.IsNullOrWhiteSpace(Status))
                throw new ValidationException("Status is required.");

            if (string.IsNullOrWhiteSpace(Tier))
                throw new ValidationException("Tier is required.");
        }


        public void GenerateDbName()
        {

            string dbName = $"Aesth_{ClinicName.Replace(" ", "_")}";
            this.DbName = dbName;
        }



    }
}