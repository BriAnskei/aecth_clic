using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class MedicineController(MedicineService medicineService)
    {
        private readonly MedicineService _medicineService = medicineService;

        // -------------------------
        // CREATE
        // -------------------------
        public async Task<Medicine> CreateMedicineAsync(
            string name,
            int stock,
            string unit,
            DateTime expiryDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Medicine name is required.");

            if (stock < 0)
                throw new ArgumentException("Stock cannot be negative.");

            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit is required.");

            if (expiryDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future.");

            return await _medicineService.CreateAsync(name, stock, unit, expiryDate);
        }

        // -------------------------
        // READ ALL
        // -------------------------
        public async Task<List<Medicine>> GetAllMedicinesAsync()
        {
            return await _medicineService.GetAllAsync();
        }

        // -------------------------
        // READ BY ID
        // -------------------------
        public async Task<Medicine?> GetMedicineByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid medicine id.");

            return await _medicineService.GetByIdAsync(id);
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateMedicineAsync(
            int id,
            string name,
            int stock,
            string unit,
            DateTime expiryDate)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid medicine id.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Medicine name is required.");

            if (stock < 0)
                throw new ArgumentException("Stock cannot be negative.");

            if (string.IsNullOrWhiteSpace(unit))
                throw new ArgumentException("Unit is required.");

            if (expiryDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future.");

            return await _medicineService.UpdateAsync(id, name, stock, unit, expiryDate);
        }



        // -------------------------
        // RESTOCK
        // -------------------------
        public async Task<bool> RestockMedicineAsync(int id, int amount)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid medicine id.");

            if (amount <= 0)
                throw new ArgumentException("Restock amount must be greater than zero.");

            return await _medicineService.RestockAsync(id, amount);
        }



        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeleteMedicineAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid medicine id.");

            return await _medicineService.DeleteAsync(id);
        }
    }
}