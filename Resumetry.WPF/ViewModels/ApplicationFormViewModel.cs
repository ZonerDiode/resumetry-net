using System.Collections.ObjectModel;
using System.Windows.Input;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public class StatusItemViewModel : ViewModelBase
    {
        private Guid? _id;
        private DateTime _occurred = DateTime.Now;
        private StatusEnum _status = StatusEnum.APPLIED;

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
        private readonly IUnitOfWork _unitOfWork;
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

        public ApplicationFormViewModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            StatusItems = [];
            ApplicationEvents = [];

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

        public ObservableCollection<StatusItemViewModel> StatusItems { get; }
        public ObservableCollection<ApplicationEventViewModel> ApplicationEvents { get; }

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

        public void LoadExistingJobApplication(JobApplication jobApplication)
        {
            _existingId = jobApplication.Id;
            Company = jobApplication.Company;
            Role = jobApplication.Position;
            Salary = jobApplication.Salary ?? string.Empty;
            Description = jobApplication.Description ?? string.Empty;
            MarkAsTopJob = jobApplication.TopJob;
            SourcePageUrl = jobApplication.SourcePage ?? string.Empty;
            ReviewPageUrl = jobApplication.ReviewPage ?? string.Empty;
            LoginHints = jobApplication.LoginNotes ?? string.Empty;
            CreatedAt = jobApplication.CreatedAt;

            // Load recruiter
            if (jobApplication.Recruiter != null)
            {
                RecruiterName = jobApplication.Recruiter.Name;
                RecruiterCompany = jobApplication.Recruiter.Company ?? string.Empty;
            }

            // Load status items with their IDs
            StatusItems.Clear();
            foreach (var statusItem in jobApplication.StatusItems.OrderBy(s => s.Occurred))
            {
                StatusItems.Add(new StatusItemViewModel
                {
                    Id = statusItem.Id,
                    Occurred = statusItem.Occurred,
                    Status = statusItem.Status
                });
            }

            // Add one if empty
            if (StatusItems.Count == 0)
            {
                AddStatus();
            }

            // Load application events with their IDs
            ApplicationEvents.Clear();
            foreach (var appEvent in jobApplication.ApplicationEvents.OrderBy(e => e.Occurred))
            {
                ApplicationEvents.Add(new ApplicationEventViewModel
                {
                    Id = appEvent.Id,
                    Occurred = appEvent.Occurred,
                    Description = appEvent.Description
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
                if (IsEditMode && _existingId.HasValue)
                {
                    // Load the existing entity
                    var existingJobApp = await _unitOfWork.JobApplications.GetByIdAsync(_existingId.Value);
                    if (existingJobApp == null)
                    {
                        System.Windows.MessageBox.Show("Job application not found.",
                            "Error",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                        return false;
                    }

                    // Update scalar properties
                    existingJobApp.Company = Company;
                    existingJobApp.Position = Role;
                    existingJobApp.Description = string.IsNullOrWhiteSpace(Description) ? null : Description;
                    existingJobApp.Salary = string.IsNullOrWhiteSpace(Salary) ? null : Salary;
                    existingJobApp.TopJob = MarkAsTopJob;
                    existingJobApp.SourcePage = string.IsNullOrWhiteSpace(SourcePageUrl) ? null : SourcePageUrl;
                    existingJobApp.ReviewPage = string.IsNullOrWhiteSpace(ReviewPageUrl) ? null : ReviewPageUrl;
                    existingJobApp.LoginNotes = string.IsNullOrWhiteSpace(LoginHints) ? null : LoginHints;

                    // Update or create recruiter
                    if (!string.IsNullOrWhiteSpace(RecruiterName))
                    {
                        if (existingJobApp.Recruiter != null)
                        {
                            existingJobApp.Recruiter.Name = RecruiterName;
                            existingJobApp.Recruiter.Company = string.IsNullOrWhiteSpace(RecruiterCompany) ? null : RecruiterCompany;
                        }
                        else
                        {
                            existingJobApp.Recruiter = new Recruiter
                            {
                                Name = RecruiterName,
                                Company = string.IsNullOrWhiteSpace(RecruiterCompany) ? null : RecruiterCompany,
                                Email = null,
                                Phone = null
                            };
                        }
                    }
                    else
                    {
                        existingJobApp.Recruiter = null;
                    }

                    // Update status items
                    var existingStatusIds = existingJobApp.StatusItems.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();
                    var currentStatusIds = StatusItems.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToHashSet();

                    // Remove deleted status items
                    var statusItemsToRemove = existingJobApp.StatusItems
                        .Where(s => s.Id.HasValue && !currentStatusIds.Contains(s.Id.Value))
                        .ToList();
                    foreach (var item in statusItemsToRemove)
                    {
                        existingJobApp.StatusItems.Remove(item);
                    }

                    // Update existing and add new status items
                    foreach (var statusVm in StatusItems)
                    {
                        if (statusVm.Id.HasValue)
                        {
                            // Update existing
                            var existing = existingJobApp.StatusItems.FirstOrDefault(s => s.Id.HasValue && s.Id.Value == statusVm.Id.Value);
                            if (existing != null)
                            {
                                existing.Occurred = statusVm.Occurred;
                                existing.Status = statusVm.Status;
                            }
                        }
                        else
                        {
                            // Add new
                            existingJobApp.StatusItems.Add(new StatusItem
                            {
                                Occurred = statusVm.Occurred,
                                Status = statusVm.Status
                            });
                        }
                    }

                    // Update application events
                    var existingEventIds = existingJobApp.ApplicationEvents.Where(e => e.Id.HasValue).Select(e => e.Id!.Value).ToHashSet();
                    var currentEventIds = ApplicationEvents.Where(e => e.Id.HasValue).Select(e => e.Id!.Value).ToHashSet();

                    // Remove deleted events
                    var eventsToRemove = existingJobApp.ApplicationEvents
                        .Where(e => e.Id.HasValue && !currentEventIds.Contains(e.Id.Value))
                        .ToList();
                    foreach (var item in eventsToRemove)
                    {
                        existingJobApp.ApplicationEvents.Remove(item);
                    }

                    // Update existing and add new events
                    foreach (var eventVm in ApplicationEvents)
                    {
                        if (!string.IsNullOrWhiteSpace(eventVm.Description))
                        {
                            if (eventVm.Id.HasValue)
                            {
                                // Update existing
                                var existing = existingJobApp.ApplicationEvents.FirstOrDefault(e => e.Id.HasValue && e.Id.Value == eventVm.Id.Value);
                                if (existing != null)
                                {
                                    existing.Occurred = eventVm.Occurred;
                                    existing.Description = eventVm.Description;
                                }
                            }
                            else
                            {
                                // Add new
                                existingJobApp.ApplicationEvents.Add(new ApplicationEvent
                                {
                                    Occurred = eventVm.Occurred,
                                    Description = eventVm.Description
                                });
                            }
                        }
                    }

                    _unitOfWork.JobApplications.Update(existingJobApp);
                    await _unitOfWork.SaveChangesAsync();

                    return true;
                }
                else
                {
                    // Create new job application
                    // Create recruiter if provided
                    Recruiter? recruiter = null;
                    if (!string.IsNullOrWhiteSpace(RecruiterName))
                    {
                        recruiter = new Recruiter
                        {
                            Name = RecruiterName,
                            Company = string.IsNullOrWhiteSpace(RecruiterCompany) ? null : RecruiterCompany,
                            Email = null,
                            Phone = null
                        };
                    }

                    // Create status items
                    var statusItems = new List<StatusItem>();
                    foreach (var statusVm in StatusItems)
                    {
                        statusItems.Add(new StatusItem
                        {
                            Occurred = statusVm.Occurred,
                            Status = statusVm.Status
                        });
                    }

                    // Create application events
                    var applicationEvents = new List<ApplicationEvent>();
                    foreach (var eventVm in ApplicationEvents)
                    {
                        if (!string.IsNullOrWhiteSpace(eventVm.Description))
                        {
                            applicationEvents.Add(new ApplicationEvent
                            {
                                Occurred = eventVm.Occurred,
                                Description = eventVm.Description
                            });
                        }
                    }

                    // Create job application
                    var jobApplication = new JobApplication
                    {
                        Company = Company,
                        Position = Role,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                        Salary = string.IsNullOrWhiteSpace(Salary) ? null : Salary,
                        TopJob = MarkAsTopJob,
                        SourcePage = string.IsNullOrWhiteSpace(SourcePageUrl) ? null : SourcePageUrl,
                        ReviewPage = string.IsNullOrWhiteSpace(ReviewPageUrl) ? null : ReviewPageUrl,
                        LoginNotes = string.IsNullOrWhiteSpace(LoginHints) ? null : LoginHints,
                        Recruiter = recruiter,
                        ApplicationEvents = applicationEvents,
                        StatusItems = statusItems
                    };

                    await _unitOfWork.JobApplications.AddAsync(jobApplication);
                    await _unitOfWork.SaveChangesAsync();

                    return true;
                }
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
