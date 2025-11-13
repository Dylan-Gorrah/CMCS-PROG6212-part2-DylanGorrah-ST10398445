using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CMS_ASSIGNMENT.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string[] _allowedExtensions;
        private readonly long _maxFileSize;

        public ClaimService(IClaimRepository claimRepository, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _claimRepository = claimRepository;
            _environment = environment;
            _configuration = configuration;

            _allowedExtensions = _configuration["FileUpload:AllowedExtensions"]?.Split(',')
                ?? new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx", ".xlsx" };
            _maxFileSize = long.Parse(_configuration["FileUpload:MaxFileSize"] ?? "5242880"); // 5MB default
        }

        public async Task<Claim> SubmitClaimAsync(Claim claim, IFormFile? document)
        {
            // Calculate TotalAmount before saving
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;
            claim.SubmittedDate = DateTime.UtcNow;

            if (document != null)
            {
                var filePath = await UploadDocumentAsync(document, claim.LecturerId);
                claim.DocumentFileName = document.FileName;
                claim.DocumentFilePath = filePath;
                claim.DocumentContentType = document.ContentType;
                claim.DocumentSize = document.Length;
            }

            await _claimRepository.AddAsync(claim);
            return claim;
        }

        public async Task<Claim?> GetClaimByIdAsync(int id)
        {
            return await _claimRepository.GetClaimWithDetailsAsync(id);
        }

        public async Task<IEnumerable<Claim>> GetClaimsByLecturerAsync(string lecturerId)
        {
            return await _claimRepository.GetClaimsByLecturerAsync(lecturerId);
        }

        public async Task<IEnumerable<Claim>> GetClaimsForCoordinatorAsync(string coordinatorId)
        {
            return await _claimRepository.GetClaimsByCoordinatorAsync(coordinatorId);
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsForCoordinatorAsync()
        {
            return await _claimRepository.GetPendingClaimsForCoordinatorAsync();
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsForManagerAsync()
        {
            return await _claimRepository.GetPendingClaimsForManagerAsync();
        }

        public async Task<bool> ApproveClaimByCoordinatorAsync(int claimId, string coordinatorId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.CoordinatorId != coordinatorId || claim.Status != ClaimStatus.Pending)
                return false;

            claim.Status = ClaimStatus.ApprovedByCoordinator;
            claim.ApprovedDate = DateTime.UtcNow;
            claim.ApprovedById = coordinatorId;
            await _claimRepository.UpdateAsync(claim);
            return true;
        }

        public async Task<bool> RejectClaimByCoordinatorAsync(int claimId, string coordinatorId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.CoordinatorId != coordinatorId || claim.Status != ClaimStatus.Pending)
                return false;

            claim.Status = ClaimStatus.RejectedByCoordinator;
            claim.RejectedDate = DateTime.UtcNow;
            claim.RejectedById = coordinatorId;
            await _claimRepository.UpdateAsync(claim);
            return true;
        }

        public async Task<bool> ApproveClaimByManagerAsync(int claimId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.Status != ClaimStatus.ApprovedByCoordinator)
                return false;

            claim.Status = ClaimStatus.ApprovedByManager;
            claim.ApprovedDate = DateTime.UtcNow;
            await _claimRepository.UpdateAsync(claim);
            return true;
        }

        public async Task<bool> RejectClaimByManagerAsync(int claimId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.Status != ClaimStatus.ApprovedByCoordinator)
                return false;

            claim.Status = ClaimStatus.RejectedByManager;
            claim.RejectedDate = DateTime.UtcNow;
            await _claimRepository.UpdateAsync(claim);
            return true;
        }

        public async Task<string?> UploadDocumentAsync(IFormFile file, string userId)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file size
            if (file.Length > _maxFileSize)
                throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_maxFileSize / 1024 / 1024}MB");

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
                throw new InvalidOperationException($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

            // Create user-specific folder
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", userId);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public Task<bool> DeleteDocumentAsync(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}