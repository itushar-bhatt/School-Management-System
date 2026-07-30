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
    public class ParentRepository : IParentRepository
    {
        private readonly ApplicationDbContext _context;

        public ParentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Parent> GetByIdAsync(string id)
        {
            return await _context.Parents.FindAsync(id);
        }

        public async Task<Parent> GetByUserIdAsync(string userId)
        {
            return await _context.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Parent> GetByPhoneAsync(string phone)
        {
            return await _context.Parents.FirstOrDefaultAsync(p => p.Phone == phone);
        }

        public async Task<IEnumerable<Parent>> GetAllAsync()
        {
            return await _context.Parents.ToListAsync();
        }

        public async Task AddAsync(Parent parent)
        {
            await _context.Parents.AddAsync(parent);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Parent parent)
        {
            _context.Parents.Update(parent);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var parent = await GetByIdAsync(id);
            if (parent != null)
            {
                _context.Parents.Remove(parent);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.Parents.AnyAsync(p => p.Id == id);
        }

        public async Task<bool> ExistsByPhoneAsync(string phone)
        {
            return await _context.Parents.AnyAsync(p => p.Phone == phone);
        }
    }
}