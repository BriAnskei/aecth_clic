using aesth_clic.Tenant.Dto.UserManagement;
using aesth_clic.Tenant.Model;
using aesth_clic.Tenant.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aesth_clic.Tenant.Controller
{
    public class UserController
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        public async Task AddUserAsync(NewUserDto newUserDto)
        {
            if (newUserDto == null)
                throw new ArgumentNullException(nameof(newUserDto));
            newUserDto.ValidateRequiredFields();
            var user = new User
            {
                FullName = newUserDto.FullName,
                Email = newUserDto.Email,
                PhoneNumber = newUserDto.PhoneNumber,
                Username = newUserDto.UserName,
                Password = newUserDto.Password,
                Role = newUserDto.Role
            };
            await _userService.AddUserAsync(user);
        }

     
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userService.GetAllUsersAsync();
        }


        public async Task<User?> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user id.");
            return await _userService.GetUserByIdAsync(id);
        }


        public async Task UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            if (updateUserDto == null)
                throw new ArgumentNullException(nameof(updateUserDto));
            updateUserDto.ValidateRequiredFields();

            var user = new User
            {
                Id = updateUserDto.Id,
                FullName = updateUserDto.FullName,
                Email = updateUserDto.Email,
                PhoneNumber = updateUserDto.PhoneNumber,
                Username = updateUserDto.UserName,
                Password = updateUserDto.Password ?? "",
                Role = updateUserDto.Role
            };
            await _userService.UpdateUserAsync(user);
        }


        public async Task UpdateAccountStatusAsync(int userId, string newStatus)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user id.");
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status is required.");
            await _userService.UpdateAccountStatusAsync(userId, newStatus);
        }


        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user id.");
            await _userService.DeleteUserAsync(id);
        }
    }
}