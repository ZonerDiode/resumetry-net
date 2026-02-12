using System.Collections.ObjectModel;
using System.Windows.Input;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public class StatusItemViewModel : ViewModelBase
    {
        private Guid? _id;
        private DateTime _occurred = DateTime.Now;
        private StatusEnum _status = StatusEnum.Applied;

        public Guid? Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public DateTime Occurred
        {
            get => _occurred;
            set => SetProperty(ref _occurred, value);
        }

        public StatusEnum Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public StatusEnum[] AvailableStatuses => Enum.GetValues<StatusEnum>();
    }

    public class ApplicationEventViewModel : ViewModelBase
    {
        private Guid? _id;
        private DateTime _occurred = DateTime.Now;
        private string _description = string.Empty;

        public Guid? Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public DateTime Occurred
        {
            get => _occurred;
            set => SetProperty(ref _occurred, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
    }

    public class ApplicationFormViewModel : ViewModelBase
    {
        private readonly IJobApplicationService _jobApplicationService;
        private Guid? _existingId;
        private string _company = string.Empty;
        private string _role = string.Empty;
        private string _recruiterName = string.Empty;
        private string _recruiterCompany = string.Empty;
        private DateTime _createdAt = DateTime.Now;
        private string _salary = string.Empty;
        private bool _markAsTopJob;
        private string _sourcePageUrl = string.Empty;
        private string _reviewPageUrl = string.Empty;
        private string _loginHints = string.Empty;
        private string _description = string.Empty;

        public ApplicationFormViewModel(IJobApplicationService jobApplicationService)
        {
            _jobApplicationService = jobApplicationService;

            AddStatusCommand = new RelayCommand(_ => AddStatus());
            RemoveStatusCommand = new RelayCommand(item => RemoveStatus(item as StatusItemViewModel));
            AddNoteCommand = new RelayCommand(_ => AddNote());
            RemoveNoteCommand = new RelayCommand(item => RemoveNote(item as ApplicationEventViewModel));

            // Add initial status item for new applications
            AddStatus();
        }

        public bool IsEditMode => _existingId.HasValue;
        public string WindowTitle => IsEditMode ? "Edit Application" : "New Application";
        public string SaveButtonText => IsEditMode ? "Save" : "Create";

        public string Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        public string RecruiterName
        {
            get => _recruiterName;
            set => SetProperty(ref _recruiterName, value);
        }

        public string RecruiterCompany
        {
            get => _recruiterCompany;
            set => SetProperty(ref _recruiterCompany, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public string Salary
        {
            get => _salary;
            set => SetProperty(ref _salary, value);
        }

        public bool MarkAsTopJob
        {
            get => _markAsTopJob;
            set => SetProperty(ref _markAsTopJob, value);
        }

        public string SourcePageUrl
        {
            get => _sourcePageUrl;
            set => SetProperty(ref _sourcePageUrl, value);
        }

        public string ReviewPageUrl
        {
            get => _reviewPageUrl;
            set => SetProperty(ref _reviewPageUrl, value);
        }

        public string LoginHints
        {
            get => _loginHints;
            set => SetProperty(ref _loginHints, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ObservableCollection<StatusItemViewModel> StatusItems { get; } = [];
        public ObservableCollection<ApplicationEventViewModel> ApplicationEvents { get; } = [];

        public ICommand AddStatusCommand { get; }
        public ICommand RemoveStatusCommand { get; } 
        public ICommand AddNoteCommand { get; }
        public ICommand RemoveNoteCommand { get; }

        public int StatusCount => StatusItems.Count;
        public int NotesCount => ApplicationEvents.Count;

        private void AddStatus()
        {
            StatusItems.Add(new StatusItemViewModel { Occurred = DateTime.Now });
            OnPropertyChanged(nameof(StatusCount));
        }

        private void RemoveStatus(StatusItemViewModel? item)
        {
            if (item != null)
            {
                StatusItems.Remove(item);
                OnPropertyChanged(nameof(StatusCount));
            }
        }

        private void AddNote()
        {
            ApplicationEvents.Add(new ApplicationEventViewModel { Occurred = DateTime.Now });
            OnPropertyChanged(nameof(NotesCount));
        }

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
            _existingId = detailDto.Id;
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

                if (IsEditMode && _existingId.HasValue)
                {
                    // Update existing application
                    var updateDto = new JobApplicationUpdateDto(
                        Id: _existingId.Value,
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

                    await _jobApplicationService.UpdateAsync(updateDto);
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

                    await _jobApplicationService.CreateAsync(createDto);
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
