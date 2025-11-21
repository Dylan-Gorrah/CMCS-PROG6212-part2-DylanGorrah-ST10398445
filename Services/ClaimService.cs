using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

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

            ValidateClaimInput(claim);

            if (document != null)
            {
                var filePath = await UploadDocumentAsync(document, claim.LecturerId);
                claim.DocumentFileName = document.FileName;
                claim.DocumentFilePath = filePath;
                claim.DocumentContentType = document.ContentType;
                claim.DocumentSize = document.Length;
            }

            ApplyClaimBusinessRules(claim);

            await _claimRepository.AddAsync(claim);
            return claim;
        }

        private static void ValidateClaimInput(Claim claim)
        {
            if (claim.HoursWorked <= 0)
            {
                throw new InvalidOperationException("Hours worked must be greater than zero.");
            }

            if (claim.HoursWorked > 200)
            {
                throw new InvalidOperationException("Hours worked cannot exceed 200 hours per claim.");
            }

            if (claim.HourlyRate <= 0)
            {
                throw new InvalidOperationException("Hourly rate must be greater than zero.");
            }

            if (claim.HourlyRate > 10000)
            {
                throw new InvalidOperationException("Hourly rate cannot exceed 10 000 ZAR.");
            }

            if (claim.TotalAmount <= 0)
            {
                throw new InvalidOperationException("Total amount must be greater than zero.");
            }

            if (claim.TotalAmount > 200 * 10000)
            {
                throw new InvalidOperationException("Calculated total amount exceeds allowable limits for a single claim.");
            }
        }

        private static void ApplyClaimBusinessRules(Claim claim)
        {
            var evaluation = EvaluateClaimRules(claim);
            claim.IsFlaggedForReview = evaluation.IsFlaggedForReview;
            claim.HasBlockingViolations = evaluation.HasBlockingViolations;
            claim.FlaggedReasons = evaluation.IsFlaggedForReview ? evaluation.Reasons : null;
        }

        private static ClaimRuleEvaluation EvaluateClaimRules(Claim claim)
        {
            var flaggedReasons = new List<string>();
            var blockingReasons = new List<string>();

            if (claim.HoursWorked > 160)
            {
                flaggedReasons.Add("Hours worked exceeds 160 hours in a single claim.");
            }

            if (claim.HourlyRate > 1500)
            {
                flaggedReasons.Add("Hourly rate is above the preferred ceiling of R1 500.");
            }

            if (claim.TotalAmount > 80000)
            {
                flaggedReasons.Add("Total amount is unusually high (exceeds R80 000).");
            }

            if (claim.HoursWorked > 80 && string.IsNullOrWhiteSpace(claim.DocumentFileName))
            {
                blockingReasons.Add("Claims over 80 hours require a supporting document.");
            }

            if (claim.HourlyRate <= 0 || claim.HoursWorked <= 0)
            {
                blockingReasons.Add("Invalid numeric values detected for hours or rate.");
            }

            var allReasons = blockingReasons.Concat(flaggedReasons).ToList();

            return new ClaimRuleEvaluation
            {
                IsFlaggedForReview = allReasons.Any(),
                HasBlockingViolations = blockingReasons.Any(),
                Reasons = allReasons.Any() ? string.Join(" | ", allReasons) : null
            };
        }

        public async Task<Claim?> GetClaimByIdAsync(int id)
        {
            var claim = await _claimRepository.GetClaimWithDetailsAsync(id);
            if (claim != null)
            {
                ApplyClaimBusinessRules(claim);
            }

            return claim;
        }

        public async Task<IEnumerable<Claim>> GetClaimsByLecturerAsync(string lecturerId)
        {
            var claims = await _claimRepository.GetClaimsByLecturerAsync(lecturerId);
            foreach (var claim in claims)
            {
                ApplyClaimBusinessRules(claim);
            }

            return claims;
        }

        public async Task<IEnumerable<Claim>> GetClaimsForCoordinatorAsync(string coordinatorId)
        {
            var claims = await _claimRepository.GetClaimsByCoordinatorAsync(coordinatorId);
            foreach (var claim in claims)
            {
                ApplyClaimBusinessRules(claim);
            }

            return claims;
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsForCoordinatorAsync()
        {
            var claims = await _claimRepository.GetPendingClaimsForCoordinatorAsync();
            foreach (var claim in claims)
            {
                ApplyClaimBusinessRules(claim);
            }

            return claims;
        }

        public async Task<IEnumerable<Claim>> GetPendingClaimsForManagerAsync()
        {
            var claims = await _claimRepository.GetPendingClaimsForManagerAsync();
            foreach (var claim in claims)
            {
                ApplyClaimBusinessRules(claim);
            }

            return claims;
        }

        public async Task<bool> ApproveClaimByCoordinatorAsync(int claimId, string coordinatorId)
        {
            var claim = await _claimRepository.GetByIdAsync(claimId);
            if (claim == null || claim.CoordinatorId != coordinatorId || claim.Status != ClaimStatus.Pending)
                return false;

            ApplyClaimBusinessRules(claim);
            if (claim.HasBlockingViolations)
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

            ApplyClaimBusinessRules(claim);
            if (claim.HasBlockingViolations)
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

            await SaveFileAsync(file, filePath);

            return filePath;
        }

        private sealed class ClaimRuleEvaluation
        {
            public bool IsFlaggedForReview { get; init; }
            public bool HasBlockingViolations { get; init; }
            public string? Reasons { get; init; }
        }

        private async Task<string> SaveFileAsync(IFormFile file, string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            {
                await file.CopyToAsync(stream);
            }

            return filePath;
        }

        public Task<bool> DeleteDocumentAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return Task.FromResult(false);
            File.Delete(filePath);
            return Task.FromResult(true);
        }
    }
}