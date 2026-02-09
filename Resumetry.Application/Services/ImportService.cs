using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Resumetry.Application.Services
{
    public class ImportService : IImportService
    {
        public async Task<IEnumerable<JobApplication>> ImportFromJsonAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            var dtos = JsonSerializer.Deserialize<List<JobApplicationDto>>(jsonContent);

            if (dtos == null)
            {
                throw new InvalidOperationException("Failed to deserialize JSON file");
            }

            var jobApplications = new List<JobApplication>();

            foreach (var dto in dtos)
            {
                var jobApplication = MapDtoToEntity(dto);
                jobApplications.Add(jobApplication);
            }

            return jobApplications;
        }

        public Task<IEnumerable<JobApplication>> ImportFromCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException("CSV import is not yet implemented");
        }

        private JobApplication MapDtoToEntity(JobApplicationDto dto)
        {
            var jobApplication = new JobApplication
            {
                Id = dto.Id,
                Company = dto.Company ?? string.Empty,
                Position = dto.Role ?? string.Empty,
                Description = dto.Description ?? string.Empty,
                Salary = dto.Salary ?? string.Empty,
                TopJob = dto.TopJob,
                SourcePage = string.IsNullOrWhiteSpace(dto.SourcePage) ? null : dto.SourcePage,
                ReviewPage = string.IsNullOrWhiteSpace(dto.ReviewPage) ? null : dto.ReviewPage,
                LoginNotes = string.IsNullOrWhiteSpace(dto.LoginHints) ? null : dto.LoginHints,
                CreatedAt = dto.AppliedDate,
                UpdatedAt = dto.AppliedDate
            };

            // Map Recruiter if name is provided
            if (!string.IsNullOrWhiteSpace(dto.RecruiterName))
            {
                jobApplication.Recruiter = new Recruiter
                {
                    Name = dto.RecruiterName,
                    Company = string.IsNullOrWhiteSpace(dto.RecruiterCompany) ? null : dto.RecruiterCompany
                };
            }

            // Map Status Items
            if (dto.Status != null)
            {
                foreach (var statusDto in dto.Status)
                {
                    if (Enum.TryParse<StatusEnum>(statusDto.Status, true, out var statusEnum))
                    {
                        jobApplication.StatusItems.Add(new StatusItem
                        {
                            Occurred = statusDto.OccurDate,
                            Status = statusEnum
                        });
                    }
                }
            }

            // Map Application Events (Notes)
            if (dto.Notes != null)
            {
                foreach (var noteDto in dto.Notes)
                {
                    if (!string.IsNullOrWhiteSpace(noteDto.Description))
                    {
                        jobApplication.ApplicationEvents.Add(new ApplicationEvent
                        {
                            Occurred = noteDto.OccurDate,
                            Description = noteDto.Description
                        });
                    }
                }
            }

            return jobApplication;
        }

        // DTO classes for JSON deserialization
        private class JobApplicationDto
        {
            [JsonPropertyName("id")]
            public Guid Id { get; set; }

            [JsonPropertyName("company")]
            public string? Company { get; set; }

            [JsonPropertyName("role")]
            public string? Role { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("salary")]
            public string? Salary { get; set; }

            [JsonPropertyName("topJob")]
            public bool TopJob { get; set; }

            [JsonPropertyName("sourcePage")]
            public string? SourcePage { get; set; }

            [JsonPropertyName("reviewPage")]
            public string? ReviewPage { get; set; }

            [JsonPropertyName("loginHints")]
            public string? LoginHints { get; set; }

            [JsonPropertyName("recruiterName")]
            public string? RecruiterName { get; set; }

            [JsonPropertyName("recruiterCompany")]
            public string? RecruiterCompany { get; set; }

            [JsonPropertyName("appliedDate")]
            public DateTime AppliedDate { get; set; }

            [JsonPropertyName("status")]
            public List<StatusDto>? Status { get; set; }

            [JsonPropertyName("notes")]
            public List<NoteDto>? Notes { get; set; }
        }

        private class StatusDto
        {
            [JsonPropertyName("occurDate")]
            public DateTime OccurDate { get; set; }

            [JsonPropertyName("status")]
            public string Status { get; set; } = string.Empty;
        }

        private class NoteDto
        {
            [JsonPropertyName("occurDate")]
            public DateTime OccurDate { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;
        }
    }
}
