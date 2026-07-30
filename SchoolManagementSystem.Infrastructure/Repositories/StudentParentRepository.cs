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
    public class StudentParentRepository : IStudentParentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentParentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentParent> GetByIdAsync(string id)
        {
            return await _context.StudentParents.FindAsync(id);
        }

        public async Task<IEnumerable<StudentParent>> GetByStudentIdAsync(string studentId)
        {
            return await _context.StudentParents
                .Where(sp => sp.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentParent>> GetByParentIdAsync(string parentId)
        {
            return await _context.StudentParents
                .Where(sp => sp.ParentId == parentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<StudentParent>> GetAllAsync()
        {
            return await _context.StudentParents.ToListAsync();
        }

        public async Task AddAsync(StudentParent studentParent)
        {
            await _context.StudentParents.AddAsync(studentParent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var studentParent = await GetByIdAsync(id);
            if (studentParent != null)
            {
                _context.StudentParents.Remove(studentParent);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string studentId, string parentId)
        {
            return await _context.StudentParents
                .AnyAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);
        }

        public async Task DeleteByStudentIdAsync(string studentId)
        {
            var links = await GetByStudentIdAsync(studentId);
            _context.StudentParents.RemoveRange(links);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByParentIdAsync(string parentId)
        {
            var links = await GetByParentIdAsync(parentId);
            _context.StudentParents.RemoveRange(links);
            await _context.SaveChangesAsync();
        }
    }
}