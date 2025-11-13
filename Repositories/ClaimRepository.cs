using Microsoft.EntityFrameworkCore;
using CMS_ASSIGNMENT.Data;
using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.Models;

namespace CMS_ASSIGNMENT.Repositories
{
    public class ClaimRepository : Repository<Claim>, IClaimRepository
    {
        public ClaimRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Claim>> GetClaimsByLecturerAsync(string lecturerId)
        {
            return await _context.Claims
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetClaimsByCoordinatorAsync(string coordinatorId)
        {
            return await _context.Claims
                .Where(c => c.CoordinatorId == coordinatorId)
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsForCoordinatorAsync()
        {
            return await _context.Claims
                .Where(c => c.Status == ClaimStatus.Pending)
                .Include(c => c.Lecturer)
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsForManagerAsync()
        {
            return await _context.Claims
                .Where(c => c.Status == ClaimStatus.ApprovedByCoordinator)
                .Include(c => c.Lecturer)
                .Include(c => c.Coordinator)
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();
        }

        public async Task<Claim?> GetClaimWithDetailsAsync(int id)
        {
            return await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Coordinator)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}