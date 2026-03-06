using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class MenuController(MenuService menuService)
    {
        private readonly MenuService _menuService = menuService;

        // -------------------------
        // CREATE
        // -------------------------
        public async Task<ServiceMenu> CreateServiceAsync(string name, double price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Service name is required.");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative.");

            return await _menuService.CreateAsync(name, price);
        }

        // -------------------------
        // READ ALL
        // -------------------------
        public async Task<List<ServiceMenu>> GetAllServicesAsync()
        {
            return await _menuService.GetAllAsync();
        }

        // -------------------------
        // READ BY ID
        // -------------------------
        public async Task<ServiceMenu?> GetServiceByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid service id.");

            return await _menuService.GetByIdAsync(id);
        }

        // -------------------------
        // UPDATE
        // -------------------------
        public async Task<bool> UpdateServiceAsync(int id, string name, double price)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid service id.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Service name is required.");

            if (price < 0)
                throw new ArgumentException("Price cannot be negative.");

            return await _menuService.UpdateAsync(id, name, price);
        }

        // -------------------------
        // DELETE
        // -------------------------
        public async Task<bool> DeleteServiceAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid service id.");

            return await _menuService.DeleteAsync(id);
        }
    }
}