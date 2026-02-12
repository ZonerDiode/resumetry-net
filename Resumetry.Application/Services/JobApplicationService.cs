using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;

namespace Resumetry.Application.Services;

/// <summary>
/// Service for managing job application business logic.
/// </summary>
public class JobApplicationService(IUnitOfWork unitOfWork) : IJobApplicationService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    /// <inheritdoc />
    public async Task<Guid> CreateAsync(CreateJobApplicationDto dto, CancellationToken cancellationToken = default)
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
        if (dto.StatusItems is not null)
        {
            foreach (var statusDto in dto.StatusItems)
            {
                entity.StatusItems.Add(new StatusItem
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
        await _unitOfWork.JobApplications.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UpdateJobApplicationDto dto, CancellationToken cancellationToken = default)
    {
        // Validate
        ValidateDto(dto.Company, dto.Position);

        // Load existing entity
        var entity = await _unitOfWork.JobApplications.GetByIdAsync(dto.Id, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Job application with ID {dto.Id} not found.");
        }

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
        SyncStatusItems(entity, dto.StatusItems);

        // Sync application events
        SyncApplicationEvents(entity, dto.ApplicationEvents);

        // Persist
        _unitOfWork.JobApplications.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
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

    private static void SyncStatusItems(JobApplication entity, List<StatusItemDto>? statusItemsDto)
    {
        if (statusItemsDto is null)
        {
            statusItemsDto = [];
        }

        // Remove items not in the DTO list
        var itemsToRemove = entity.StatusItems
            .Where(existing => !statusItemsDto.Any(dto => dto.Id == existing.Id))
            .ToList();
        foreach (var item in itemsToRemove)
        {
            entity.StatusItems.Remove(item);
        }

        // Update existing or add new items
        foreach (var dto in statusItemsDto)
        {
            if (dto.Id.HasValue)
            {
                // Update existing
                var existing = entity.StatusItems.FirstOrDefault(x => x.Id == dto.Id);
                if (existing is not null)
                {
                    existing.Occurred = dto.Occurred;
                    existing.Status = dto.Status;
                }
            }
            else
            {
                // Add new
                entity.StatusItems.Add(new StatusItem
                {
                    Occurred = dto.Occurred,
                    Status = dto.Status
                });
            }
        }
    }

    private static void SyncApplicationEvents(JobApplication entity, List<ApplicationEventDto>? eventsDto)
    {
        if (eventsDto is null)
        {
            eventsDto = [];
        }

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
}
