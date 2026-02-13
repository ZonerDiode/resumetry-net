using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public partial class StatusItemViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid? _id;

        [ObservableProperty]
        private DateTime _occurred = DateTime.Now;

        [ObservableProperty]
        private StatusEnum _status = StatusEnum.Applied;

        public StatusEnum[] AvailableStatuses => Enum.GetValues<StatusEnum>();
    }

    public partial class ApplicationEventViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Guid? _id;

        [ObservableProperty]
        private DateTime _occurred = DateTime.Now;

        [ObservableProperty]
        private string _description = string.Empty;
    }

    public partial class ApplicationFormViewModel(IJobApplicationService jobApplicationService) : ViewModelBase
    {
        public bool IsEditMode => ExistingId.HasValue;
        public string WindowTitle => IsEditMode ? "Edit Application" : "New Application";
        public string SaveButtonText => IsEditMode ? "Save" : "Create";

        public ObservableCollection<StatusItemViewModel> StatusItems { get; } = [];
        public ObservableCollection<ApplicationEventViewModel> ApplicationEvents { get; } = [];

        public int StatusCount => StatusItems.Count;
        public int NotesCount => ApplicationEvents.Count;

        [ObservableProperty]
        private Guid? _existingId;

        [ObservableProperty]
        private string _company = string.Empty;

        [ObservableProperty]
        private string _role = string.Empty;

        [ObservableProperty]
        private string _recruiterName = string.Empty;

        [ObservableProperty]
        private string _recruiterCompany = string.Empty;

        [ObservableProperty]
        private DateTime _createdAt = DateTime.Now;

        [ObservableProperty]
        private string _salary = string.Empty;

        [ObservableProperty]
        private bool _markAsTopJob;

        [ObservableProperty]
        private string _sourcePageUrl = string.Empty;

        [ObservableProperty]
        private string _reviewPageUrl = string.Empty;

        [ObservableProperty]
        private string _loginHints = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [RelayCommand]
        private void AddStatus()
        {
            StatusItems.Add(new StatusItemViewModel { Occurred = DateTime.Now });
            OnPropertyChanged(nameof(StatusCount));
        }

        [RelayCommand]
        private void RemoveStatus(StatusItemViewModel? item)
        {
            if (item != null)
            {
                StatusItems.Remove(item);
                OnPropertyChanged(nameof(StatusCount));
            }
        }

        [RelayCommand]
        private void AddNote()
        {
            ApplicationEvents.Add(new ApplicationEventViewModel { Occurred = DateTime.Now });
            OnPropertyChanged(nameof(NotesCount));
        }

        [RelayCommand]
        private void RemoveNote(ApplicationEventViewModel? item)
        {
            if (item != null)
            {
                ApplicationEvents.Remove(item);
                OnPropertyChanged(nameof(NotesCount));
            }
        }

        public void LoadExistingJobApplication(JobApplicationDetailDto detailDto)
        {
            ExistingId = detailDto.Id;
            Company = detailDto.Company;
            Role = detailDto.Position;
            Salary = detailDto.Salary ?? string.Empty;
            Description = detailDto.Description ?? string.Empty;
            MarkAsTopJob = detailDto.TopJob;
            SourcePageUrl = detailDto.SourcePage ?? string.Empty;
            ReviewPageUrl = detailDto.ReviewPage ?? string.Empty;
            LoginHints = detailDto.LoginNotes ?? string.Empty;
            CreatedAt = detailDto.CreatedAt;

            // Load recruiter
            if (detailDto.Recruiter != null)
            {
                RecruiterName = detailDto.Recruiter.Name;
                RecruiterCompany = detailDto.Recruiter.Company ?? string.Empty;
            }

            // Load status items with their IDs
            StatusItems.Clear();
            foreach (var statusItemDto in detailDto.StatusItems.OrderBy(s => s.Occurred))
            {
                StatusItems.Add(new StatusItemViewModel
                {
                    Id = statusItemDto.Id,
                    Occurred = statusItemDto.Occurred,
                    Status = statusItemDto.Status
                });
            }

            // Add one if empty
            if (StatusItems.Count == 0)
            {
                AddStatus();
            }

            // Load application events with their IDs
            ApplicationEvents.Clear();
            foreach (var eventDto in detailDto.ApplicationEvents.OrderBy(e => e.Occurred))
            {
                ApplicationEvents.Add(new ApplicationEventViewModel
                {
                    Id = eventDto.Id,
                    Occurred = eventDto.Occurred,
                    Description = eventDto.Description
                });
            }

            OnPropertyChanged(nameof(StatusCount));
            OnPropertyChanged(nameof(NotesCount));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(WindowTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Company) &&
                   !string.IsNullOrWhiteSpace(Role);
        }

        public async Task<bool> SaveAsync()
        {
            try
            {
                // Build recruiter DTO if provided
                RecruiterDto? recruiterDto = null;
                if (!string.IsNullOrWhiteSpace(RecruiterName))
                {
                    recruiterDto = new RecruiterDto(
                        Name: RecruiterName,
                        Company: string.IsNullOrWhiteSpace(RecruiterCompany) ? null : RecruiterCompany);
                }

                // Build status items DTOs
                var statusItemDtos = StatusItems
                    .Select(s => new StatusItemDto(s.Occurred, s.Status, s.Id))
                    .ToList();

                // Build application events DTOs (exclude empty descriptions)
                var eventDtos = ApplicationEvents
                    .Where(e => !string.IsNullOrWhiteSpace(e.Description))
                    .Select(e => new ApplicationEventDto(e.Occurred, e.Description, e.Id))
                    .ToList();

                if (IsEditMode && ExistingId.HasValue)
                {
                    // Update existing application
                    var updateDto = new JobApplicationUpdateDto(
                        Id: ExistingId.Value,
                        Company: Company,
                        Position: Role,
                        Description: string.IsNullOrWhiteSpace(Description) ? null : Description,
                        Salary: string.IsNullOrWhiteSpace(Salary) ? null : Salary,
                        TopJob: MarkAsTopJob,
                        SourcePage: string.IsNullOrWhiteSpace(SourcePageUrl) ? null : SourcePageUrl,
                        ReviewPage: string.IsNullOrWhiteSpace(ReviewPageUrl) ? null : ReviewPageUrl,
                        LoginNotes: string.IsNullOrWhiteSpace(LoginHints) ? null : LoginHints,
                        Recruiter: recruiterDto,
                        StatusItems: statusItemDtos,
                        ApplicationEvents: eventDtos);

                    await jobApplicationService.UpdateAsync(updateDto);
                }
                else
                {
                    // Create new application
                    var createDto = new JobApplicationCreateDto(
                        Company: Company,
                        Position: Role,
                        Description: string.IsNullOrWhiteSpace(Description) ? null : Description,
                        Salary: string.IsNullOrWhiteSpace(Salary) ? null : Salary,
                        TopJob: MarkAsTopJob,
                        SourcePage: string.IsNullOrWhiteSpace(SourcePageUrl) ? null : SourcePageUrl,
                        ReviewPage: string.IsNullOrWhiteSpace(ReviewPageUrl) ? null : ReviewPageUrl,
                        LoginNotes: string.IsNullOrWhiteSpace(LoginHints) ? null : LoginHints,
                        Recruiter: recruiterDto,
                        StatusItems: statusItemDtos,
                        ApplicationEvents: eventDtos);

                    await jobApplicationService.CreateAsync(createDto);
                }

                return true;
            }
            catch (KeyNotFoundException)
            {
                System.Windows.MessageBox.Show("Job application not found.",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
            catch (ArgumentException ex)
            {
                System.Windows.MessageBox.Show($"Validation error: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving job application: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return false;
            }
        }
    }
}
