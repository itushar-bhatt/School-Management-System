using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Domain.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student> GetByIdAsync(string id);
        Task<Student> GetByUserIdAsync(string userId);
        Task<Student> GetByAdmissionNoAsync(string admissionNo);
        Task<IEnumerable<Student>> GetAllAsync();
        Task<IEnumerable<Student>> GetByClassAndSectionAsync(string className, string section);
        Task AddAsync(Student student);
        Task UpdateAsync(Student student);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}