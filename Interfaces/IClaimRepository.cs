using CMS_ASSIGNMENT.Models;

namespace CMS_ASSIGNMENT.Interfaces
{
    public interface IClaimRepository : IRepository<Claim>
    {
        Task<IEnumerable<Claim>> GetClaimsByLecturerAsync(string lecturerId);
        Task<IEnumerable<Claim>> GetClaimsByCoordinatorAsync(string coordinatorId);
        Task<IEnumerable<Claim>> GetPendingClaimsForCoordinatorAsync();
        Task<IEnumerable<Claim>> GetPendingClaimsForManagerAsync();
        Task<Claim?> GetClaimWithDetailsAsync(int id);
    }
}