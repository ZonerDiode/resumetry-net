using System.Collections.ObjectModel;
using System.Windows.Input;
using Resumetry.Domain.Entities;
using Resumetry.Domain.Enums;

namespace Resumetry.ViewModels
{
    public class StatusItemViewModel : ViewModelBase
    {
        private DateTime _occurred = DateTime.Now;
        private StatusEnum _status = StatusEnum.APPLIED;

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
        private DateTime _occurred = DateTime.Now;
        private string _description = string.Empty;

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

        public ApplicationFormViewModel()
        {
            StatusItems = new ObservableCollection<StatusItemViewModel>();
            ApplicationEvents = new ObservableCollection<ApplicationEventViewModel>();

            AddStatusCommand = new RelayCommand(_ => AddStatus());
            RemoveStatusCommand = new RelayCommand(item => RemoveStatus(item as StatusItemViewModel));
            AddNoteCommand = new RelayCommand(_ => AddNote());
            RemoveNoteCommand = new RelayCommand(item => RemoveNote(item as ApplicationEventViewModel));

            // Add initial status item
            AddStatus();
        }

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
            StatusItems.Add(new StatusItemViewModel { Occurred = CreatedAt });
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

        public bool Validate()
        {
            return !string.IsNullOrWhiteSpace(Company) &&
                   !string.IsNullOrWhiteSpace(Role);
        }

        public JobApplication CreateJobApplication()
        {
            // TODO: This needs to be updated once we handle creating entities properly
            // For now, this is a placeholder showing the structure
            throw new NotImplementedException("Creating job applications will be implemented with repository pattern");
        }
    }
}
