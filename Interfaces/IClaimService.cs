using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Http;

namespace CMS_ASSIGNMENT.Interfaces
{
    public interface IClaimService
    {
     Task<Claim> SubmitClaimAsync(Claim claim, IFormFile? document);
        Task<Claim?> GetClaimByIdAsync(int id);
        Task<IEnumerable<Claim>> GetClaimsByLecturerAsync(string lecturerId);
        Task<IEnumerable<Claim>> GetClaimsForCoordinatorAsync(string coordinatorId);
  Task<IEnumerable<Claim>> GetPendingClaimsForCoordinatorAsync();
        Task<IEnumerable<Claim>> GetPendingClaimsForManagerAsync();
        Task<bool> ApproveClaimByCoordinatorAsync(int claimId, string coordinatorId);
        Task<bool> RejectClaimByCoordinatorAsync(int claimId, string coordinatorId);
        Task<bool> ApproveClaimByManagerAsync(int claimId);
     Task<bool> RejectClaimByManagerAsync(int claimId);
    Task<string?> UploadDocumentAsync(IFormFile file, string userId);
        Task<bool> DeleteDocumentAsync(string filePath);
    }
}