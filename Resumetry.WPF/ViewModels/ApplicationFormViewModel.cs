using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Resumetry.Application.DTOs;
using Resumetry.Application.Interfaces;
using Resumetry.Domain.Enums;
using Resumetry.WPF.Messages;
using Resumetry.WPF.Services;

namespace Resumetry.ViewModels
{
    public partial class ApplicationStatusViewModel : ViewModelBase
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

    public partial class ApplicationFormViewModel(IJobApplicationService jobApplicationService, INavigationService navigationService, IDialogService dialogService) : ViewModelBase
    {
        public bool IsEditMode => ExistingId.HasValue;
        public string WindowTitle => IsEditMode ? "Edit Application" : "New Application";
        public string SaveButtonText => IsEditMode ? "Save" : "Create";

        public ObservableCollection<ApplicationStatusViewModel> ApplicationStatuses { get; } = [];
        public ObservableCollection<ApplicationEventViewModel> ApplicationEvents { get; } = [];

        public int StatusCount => ApplicationStatuses.Count;
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
        private string _recruiterPhone = string.Empty;

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
            ApplicationStatuses.Add(new ApplicationStatusViewModel { Occurred = DateTime.Now });
            OnPropertyChanged(nameof(StatusCount));
        }

        [RelayCommand]
        private void RemoveStatus(ApplicationStatusViewModel? item)
        {
            if (item != null)
            {
                ApplicationStatuses.Remove(item);
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
                RecruiterPhone = detailDto.Recruiter.Phone ?? string.Empty;
            }

            // Load status items with their IDs
            ApplicationStatuses.Clear();
            foreach (var statusItemDto in detailDto.ApplicationStatuses.OrderBy(s => s.Occurred))
            {
                ApplicationStatuses.Add(new ApplicationStatusViewModel
                {
                    Id = statusItemDto.Id,
                    Occurred = statusItemDto.Occurred,
                    Status = statusItemDto.Status
                });
            }

            // Add one if empty
            if (ApplicationStatuses.Count == 0)
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

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Validate
            if (!Validate())
            {
                dialogService.ShowError("Please fill in Company and Role fields.", "Validation Error");
                return;
            }

            try
            {
                // Build recruiter DTO if provided
                RecruiterDto? recruiterDto = null;
                if (!string.IsNullOrWhiteSpace(RecruiterName))
                {
                    recruiterDto = new RecruiterDto(
                        Name: RecruiterName,
                        Company: string.IsNullOrWhiteSpace(RecruiterCompany) ? null : RecruiterCompany,
                        Phone: string.IsNullOrWhiteSpace(RecruiterPhone) ? null : RecruiterPhone);
                }

                // Build status items DTOs
                var statusItemDtos = ApplicationStatuses
                    .Select(s => new ApplicationStatusDto(s.Occurred, s.Status, s.Id))
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
                        ApplicationStatuses: statusItemDtos,
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
                        ApplicationStatuses: statusItemDtos,
                        ApplicationEvents: eventDtos);

                    await jobApplicationService.CreateAsync(createDto);
                }

                // Send message to notify that a job application was saved
                WeakReferenceMessenger.Default.Send(new JobApplicationSavedMessage());

                // Navigate back to home
                navigationService.NavigateToHome();
            }
            catch (KeyNotFoundException)
            {
                dialogService.ShowError("Job application not found.", "Error");
            }
            catch (ArgumentException ex)
            {
                dialogService.ShowError($"Validation error: {ex.Message}", "Error");
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"Error saving job application: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            navigationService.NavigateToHome();
        }

        [RelayCommand]
        private void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                dialogService.ShowError($"Could not open URL: {ex.Message}", "Error");
            }
        }
    }
}
