using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Interfaces;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.Infrastructure.Repositories
{
    public class TeacherClassRepository : ITeacherClassRepository
    {
        private readonly ApplicationDbContext _context;

        public TeacherClassRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TeacherClass> GetByIdAsync(string id)
        {
            return await _context.TeacherClasses.FindAsync(id);
        }

        public async Task<IEnumerable<TeacherClass>> GetByTeacherIdAsync(string teacherId)
        {
            return await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId && tc.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<TeacherClass>> GetByClassAsync(string className)
        {
            return await _context.TeacherClasses
                .Where(tc => tc.Class == className && tc.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(TeacherClass teacherClass)
        {
            await _context.TeacherClasses.AddAsync(teacherClass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TeacherClass teacherClass)
        {
            _context.TeacherClasses.Update(teacherClass);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var teacherClass = await GetByIdAsync(id);
            if (teacherClass != null)
            {
                _context.TeacherClasses.Remove(teacherClass);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByTeacherIdAsync(string teacherId)
        {
            var assignments = await _context.TeacherClasses
                .Where(tc => tc.TeacherId == teacherId)
                .ToListAsync();
            
            _context.TeacherClasses.RemoveRange(assignments);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string teacherId, string className, string? section)
        {
            return await _context.TeacherClasses
                .AnyAsync(tc => tc.TeacherId == teacherId && 
                               tc.Class == className && 
                               tc.Section == section && 
                               tc.IsActive);
        }
    }
}