using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchoolManagementSystem.Domain.Entities;

namespace SchoolManagementSystem.Domain.Interfaces
{
    public interface ITeacherClassRepository
    {
        Task<TeacherClass> GetByIdAsync(string id);
        Task<IEnumerable<TeacherClass>> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<TeacherClass>> GetByClassAsync(string className);
        Task AddAsync(TeacherClass teacherClass);
        Task UpdateAsync(TeacherClass teacherClass);
        Task DeleteAsync(string id);
        Task DeleteByTeacherIdAsync(string teacherId);
        Task<bool> ExistsAsync(string teacherId, string className, string? section);
    }
}