using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Interfaces;

namespace Resumetry.Application.Services;

/// <summary>
/// Service for managing job application business logic.
/// </summary>
public class JobApplicationService(IUnitOfWork unitOfWork) : IJobApplicationService
{
    /// <inheritdoc />
    public async Task<Guid> CreateAsync(JobApplicationCreateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate
        ValidateDto(dto.Company, dto.Position);

        // Map DTO to entity
        var entity = new JobApplication
        {
            Id = Guid.NewGuid(),
            Company = dto.Company,
            Position = dto.Position,
            Description = dto.Description,
            Salary = dto.Salary,
            TopJob = dto.TopJob,
            SourcePage = dto.SourcePage,
            ReviewPage = dto.ReviewPage,
            LoginNotes = dto.LoginNotes
        };

        // Map recruiter if present
        if (dto.Recruiter is not null)
        {
            entity.Recruiter = new Recruiter
            {
                Name = dto.Recruiter.Name,
                Company = dto.Recruiter.Company,
                Email = dto.Recruiter.Email,
                Phone = dto.Recruiter.Phone
            };
        }

        // Map status items if present
        if (dto.ApplicationStatuses is not null)
        {
            foreach (var statusDto in dto.ApplicationStatuses)
            {
                entity.ApplicationStatuses.Add(new ApplicationStatus
                {
                    Occurred = statusDto.Occurred,
                    Status = statusDto.Status
                });
            }
        }

        // Map application events if present
        if (dto.ApplicationEvents is not null)
        {
            foreach (var eventDto in dto.ApplicationEvents)
            {
                entity.ApplicationEvents.Add(new ApplicationEvent
                {
                    Occurred = eventDto.Occurred,
                    Description = eventDto.Description
                });
            }
        }

        // Persist
        await unitOfWork.JobApplications.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(JobApplicationUpdateDto dto, CancellationToken cancellationToken = default)
    {
        // Validate
        ValidateDto(dto.Company, dto.Position);

        // Load existing entity
        var entity = await unitOfWork.JobApplications.GetByIdAsync(dto.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job application with ID {dto.Id} not found.");

        // Update scalar properties
        entity.Company = dto.Company;
        entity.Position = dto.Position;
        entity.Description = dto.Description;
        entity.Salary = dto.Salary;
        entity.TopJob = dto.TopJob;
        entity.SourcePage = dto.SourcePage;
        entity.ReviewPage = dto.ReviewPage;
        entity.LoginNotes = dto.LoginNotes;

        // Sync recruiter
        SyncRecruiter(entity, dto.Recruiter);

        // Sync status items
        SyncApplicationStatuses(entity, dto.ApplicationStatuses);

        // Sync application events
        SyncApplicationEvents(entity, dto.ApplicationEvents);

        // Persist
        unitOfWork.JobApplications.Update(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<JobApplicationSummaryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await unitOfWork.JobApplications.GetAllAsync(cancellationToken);

        return entities.Select(entity =>
        {
            // Compute current status from latest ApplicationStatus
            var latestApplicationStatus = entity.ApplicationStatuses
                .OrderByDescending(s => s.Occurred)
                .FirstOrDefault();
            var currentStatus = latestApplicationStatus?.Status;
            var currentStatusText = currentStatus?.ToString() ?? "UNKNOWN";

            // Compute applied date from first APPLIED ApplicationStatus, fallback to CreatedAt
            var appliedApplicationStatus = entity.ApplicationStatuses
                .Where(s => s.Status == Domain.Enums.StatusEnum.Applied)
                .OrderBy(s => s.Occurred)
                .FirstOrDefault();
            var appliedDate = appliedApplicationStatus?.Occurred ?? entity.CreatedAt;

            return new JobApplicationSummaryDto(
                Id: entity.Id,
                Company: entity.Company,
                Position: entity.Position,
                Salary: entity.Salary,
                TopJob: entity.TopJob,
                CreatedAt: entity.CreatedAt,
                CurrentStatus: currentStatus,
                CurrentStatusText: currentStatusText,
                AppliedDate: appliedDate
            );
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<JobApplicationDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.JobApplications.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        return new JobApplicationDetailDto(
            Id: entity.Id,
            Company: entity.Company,
            Position: entity.Position,
            Description: entity.Description,
            Salary: entity.Salary,
            TopJob: entity.TopJob,
            SourcePage: entity.SourcePage,
            ReviewPage: entity.ReviewPage,
            LoginNotes: entity.LoginNotes,
            CreatedAt: entity.CreatedAt,
            Recruiter: entity.Recruiter is null ? null : new RecruiterDto(
                Name: entity.Recruiter.Name,
                Company: entity.Recruiter.Company,
                Email: entity.Recruiter.Email,
                Phone: entity.Recruiter.Phone
            ),
            ApplicationStatuses: entity.ApplicationStatuses.Select(s => new ApplicationStatusDto(
                Occurred: s.Occurred,
                Status: s.Status,
                Id: s.Id
            )).ToList(),
            ApplicationEvents: entity.ApplicationEvents.Select(e => new ApplicationEventDto(
                Occurred: e.Occurred,
                Description: e.Description,
                Id: e.Id
            )).ToList()
        );
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.JobApplications.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Job application with ID {id} not found.");

        unitOfWork.JobApplications.Delete(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateDto(string company, string position)
    {
        if (string.IsNullOrWhiteSpace(company))
        {
            throw new ArgumentException("Company is required.", nameof(company));
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            throw new ArgumentException("Position is required.", nameof(position));
        }
    }


    private static void SyncRecruiter(JobApplication entity, RecruiterDto? recruiterDto)
    {
        if (recruiterDto is null)
        {
            // Remove recruiter
            entity.Recruiter = null;
        }
        else if (entity.Recruiter is null)
        {
            // Create new recruiter
            entity.Recruiter = new Recruiter
            {
                Name = recruiterDto.Name,
                Company = recruiterDto.Company,
                Email = recruiterDto.Email,
                Phone = recruiterDto.Phone
            };
        }
        else
        {
            // Update existing recruiter
            entity.Recruiter.Name = recruiterDto.Name;
            entity.Recruiter.Company = recruiterDto.Company;
            entity.Recruiter.Email = recruiterDto.Email;
            entity.Recruiter.Phone = recruiterDto.Phone;
        }
    }

    private static void SyncApplicationStatuses(JobApplication entity, List<ApplicationStatusDto>? statusItemsDto)
    {
        statusItemsDto ??= [];

        // Remove items not in the DTO list
        var itemsToRemove = entity.ApplicationStatuses
            .Where(existing => !statusItemsDto.Any(dto => dto.Id == existing.Id))
            .ToList();
        foreach (var item in itemsToRemove)
        {
            entity.ApplicationStatuses.Remove(item);
        }

        // Update existing or add new items
        foreach (var dto in statusItemsDto)
        {
            if (dto.Id.HasValue)
            {
                // Update existing
                var existing = entity.ApplicationStatuses.FirstOrDefault(x => x.Id == dto.Id);
                if (existing is not null)
                {
                    existing.Occurred = dto.Occurred;
                    existing.Status = dto.Status;
                }
            }
            else
            {
                // Add new
                entity.ApplicationStatuses.Add(new ApplicationStatus
                {
                    Occurred = dto.Occurred,
                    Status = dto.Status
                });
            }
        }
    }

    private static void SyncApplicationEvents(JobApplication entity, List<ApplicationEventDto>? eventsDto)
    {
        eventsDto ??= [];

        // Remove events not in the DTO list
        var eventsToRemove = entity.ApplicationEvents
            .Where(existing => !eventsDto.Any(dto => dto.Id == existing.Id))
            .ToList();
        foreach (var evt in eventsToRemove)
        {
            entity.ApplicationEvents.Remove(evt);
        }

        // Update existing or add new events
        foreach (var dto in eventsDto)
        {
            if (dto.Id.HasValue)
            {
                // Update existing
                var existing = entity.ApplicationEvents.FirstOrDefault(x => x.Id == dto.Id);
                if (existing is not null)
                {
                    existing.Occurred = dto.Occurred;
                    existing.Description = dto.Description;
                }
            }
            else
            {
                // Add new
                entity.ApplicationEvents.Add(new ApplicationEvent
                {
                    Occurred = dto.Occurred,
                    Description = dto.Description
                });
            }
        }
    }
}
