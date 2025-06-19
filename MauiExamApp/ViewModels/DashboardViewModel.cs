using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExamApp.Models;
using MauiExamApp.Services;
using MauiExamApp.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MauiExamApp.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<Exam> _upcomingExams;

        // Properties for "Create Exam" form
        [ObservableProperty] private string _term;
        [ObservableProperty] private string _courseName;
        [ObservableProperty] private DateTime _date = DateTime.Today;
        [ObservableProperty] private int _numberOfQuestions;
        [ObservableProperty] private int _examDurationMinutes;
        [ObservableProperty] private TimeSpan _startTime = DateTime.Now.TimeOfDay;

        // --- Properties for redigering af eksamen ---
        [ObservableProperty]
        private bool _isEditingExam = false;

        [ObservableProperty]
        private int _editingExamId;

        [ObservableProperty] private string _formTitle = "Opret Ny Eksamen";
        [ObservableProperty] private string _formButtonText = "Opret Eksamen";

        // Properties for "Manage Students" popup
        [ObservableProperty]
        private bool _isStudentPopupVisible = false;

        [ObservableProperty]
        private Exam _selectedExam;

        [ObservableProperty]
        private ObservableCollection<Student> _studentsForSelectedExam;

        [ObservableProperty]
        private string _newStudentNumber;

        [ObservableProperty]
        private string _newStudentName;

        public DashboardViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            UpcomingExams = new ObservableCollection<Exam>();
            StudentsForSelectedExam = new ObservableCollection<Student>();
        }

        [RelayCommand]
        private async Task LoadUpcomingExamsAsync()
        {
            var exams = await _databaseService.GetExamsAsync(false); // Hent ikke-afsluttede eksamener
            UpcomingExams.Clear();
            foreach (var exam in exams)
            {
                UpcomingExams.Add(exam);
            }
        }

        [RelayCommand]
        private async Task SaveExamAsync()
        {
            if (string.IsNullOrWhiteSpace(Term) || string.IsNullOrWhiteSpace(CourseName))
            {
                await Shell.Current.DisplayAlert("Fejl", "Udfyld venligst alle felter.", "OK");
                return;
            }

            Exam exam;
            if (IsEditingExam)
            {
                // Opdater eksisterende eksamen
                exam = await _databaseService.GetExamAsync(EditingExamId);
            }
            else
            {
                // Opret ny eksamen
                exam = new Exam();
            }

            exam.Term = Term;
            exam.CourseName = CourseName;
            exam.Date = Date;
            exam.NumberOfQuestions = NumberOfQuestions;
            exam.ExamDurationMinutes = ExamDurationMinutes;
            exam.StartTime = StartTime;

            await _databaseService.SaveExamAsync(exam);
            await CancelEditExamAsync(); // Nulstil formularen
            await LoadUpcomingExamsAsync();
        }

        [RelayCommand]
        private async Task ShowStudentPopupAsync(Exam exam)
        {
            if (exam == null) return;
            SelectedExam = exam;
            var students = await _databaseService.GetStudentsForExamAsync(exam.Id);
            StudentsForSelectedExam.Clear();
            foreach (var s in students)
            {
                StudentsForSelectedExam.Add(s);
            }
            IsStudentPopupVisible = true;
        }

        [RelayCommand]
        private void HideStudentPopup()
        {
            IsStudentPopupVisible = false;
            NewStudentName = string.Empty;
            NewStudentNumber = string.Empty;
        }

        [RelayCommand]
        private async Task AddStudentAsync()
        {
            if (SelectedExam == null || string.IsNullOrWhiteSpace(NewStudentNumber) || string.IsNullOrWhiteSpace(NewStudentName))
            {
                await Shell.Current.DisplayAlert("Fejl", "Udfyld studienummer og navn.", "OK");
                return;
            }

            var newStudent = new Student
            {
                ExamId = SelectedExam.Id,
                StudentNumber = NewStudentNumber,
                FullName = NewStudentName
            };

            await _databaseService.SaveStudentAsync(newStudent);

            // Opdater listen og nulstil felter
            var students = await _databaseService.GetStudentsForExamAsync(SelectedExam.Id);
            StudentsForSelectedExam.Clear();
            foreach (var s in students)
            {
                StudentsForSelectedExam.Add(s);
            }
            NewStudentName = string.Empty;
            NewStudentNumber = string.Empty;
        }

        [RelayCommand]
        private async Task GoToExamAsync(Exam exam)
        {
            if (exam == null) return;
            await Shell.Current.GoToAsync($"{nameof(ExamView)}?ExamId={exam.Id}");
        }

        public async Task OnAppearing()
        {
            await LoadUpcomingExamsAsync();
        }
        
        
        [RelayCommand]
        private async Task EditExamAsync(Exam exam)
        {
            if (exam == null) return;

            IsEditingExam = true;
            EditingExamId = exam.Id;
            FormTitle = "Rediger Eksamen";
            FormButtonText = "Gem Ændringer";

            // Udfyld formular
            Term = exam.Term;
            CourseName = exam.CourseName;
            Date = exam.Date;
            StartTime = exam.StartTime;
            NumberOfQuestions = exam.NumberOfQuestions;
            ExamDurationMinutes = exam.ExamDurationMinutes;
        }

        [RelayCommand]
        private async Task CancelEditExamAsync()
        {
            IsEditingExam = false;
            EditingExamId = 0;
            FormTitle = "Opret Ny Eksamen";
            FormButtonText = "Opret Eksamen";

            // Nulstil felter
            Term = string.Empty;
            CourseName = string.Empty;
            Date = DateTime.Today;
        }

        [RelayCommand]
        private async Task DeleteExamAsync(Exam exam)
        {
            if (exam == null) return;
            bool confirmed = await Shell.Current.DisplayAlert("Bekræft Sletning", $"Er du sikker på, du vil slette eksamen '{exam.CourseName}' og alle tilmeldte studerende?", "Ja, Slet", "Annuller");
            if (confirmed)
            {
                await _databaseService.DeleteExamAsync(exam);
                await LoadUpcomingExamsAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteStudentAsync(Student student)
        {
            if (student == null) return;
            bool confirmed = await Shell.Current.DisplayAlert("Bekræft Sletning", $"Er du sikker på, du vil slette '{student.FullName}'?", "Ja, Slet", "Annuller");
            if (confirmed)
            {
                await _databaseService.DeleteStudentAsync(student);
                // Genindlæs listen i pop-up'en
                StudentsForSelectedExam.Remove(student);
            }
        }
    }
}
