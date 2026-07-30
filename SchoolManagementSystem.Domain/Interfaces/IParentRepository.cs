using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Domain.Interfaces
{
    public interface IParentRepository
    {
        Task<Parent> GetByIdAsync(string id);
        Task<Parent> GetByUserIdAsync(string userId);
        Task<Parent> GetByPhoneAsync(string phone);
        Task<IEnumerable<Parent>> GetAllAsync();
        Task AddAsync(Parent parent);
        Task UpdateAsync(Parent parent);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
        Task<bool> ExistsByPhoneAsync(string phone);
    }
}