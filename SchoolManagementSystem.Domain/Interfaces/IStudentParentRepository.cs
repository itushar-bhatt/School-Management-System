using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Domain.Interfaces
{
    public interface IStudentParentRepository
    {
        Task<StudentParent> GetByIdAsync(string id);
        Task<IEnumerable<StudentParent>> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<StudentParent>> GetByParentIdAsync(string parentId);
        Task<IEnumerable<StudentParent>> GetAllAsync();
        Task AddAsync(StudentParent studentParent);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string studentId, string parentId);
        Task DeleteByStudentIdAsync(string studentId);
        Task DeleteByParentIdAsync(string parentId);
    }
}