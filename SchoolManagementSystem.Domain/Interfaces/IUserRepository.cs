using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchoolManagementSystem.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<Entities.User> GetByIdAsync(string id);
        Task<Entities.User> GetByUsernameAsync(string username);
        Task<IEnumerable<Entities.User>> GetAllAsync();
        Task AddAsync(Entities.User user);
        Task DeleteAsync(string id);
    }
}