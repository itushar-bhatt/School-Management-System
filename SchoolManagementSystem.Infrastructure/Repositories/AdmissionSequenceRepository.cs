using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Interfaces;
using SchoolManagementSystem.Infrastructure.Identity;

namespace SchoolManagementSystem.Infrastructure.Repositories
{
    public class AdmissionSequenceRepository : IAdmissionSequenceRepository
    {
        private readonly ApplicationDbContext _context;

        public AdmissionSequenceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdmissionSequence> GetBySessionAsync(string academicSession)
        {
            return await _context.AdmissionSequences
                .FirstOrDefaultAsync(s => s.AcademicSession == academicSession);
        }

        public async Task AddAsync(AdmissionSequence sequence)
        {
            await _context.AdmissionSequences.AddAsync(sequence);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AdmissionSequence sequence)
        {
            _context.AdmissionSequences.Update(sequence);
            await _context.SaveChangesAsync();
        }
    }
}