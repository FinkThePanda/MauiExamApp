using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExamApp.Models;
using MauiExamApp.Services;
using MauiExamApp.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MauiExamApp.ViewModels
{
    public partial class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<ExamDisplay> _upcomingExams = new();
        [ObservableProperty]
        private ObservableCollection<Student> _studentsForSelectedExam = new();
        [ObservableProperty]
        private bool _isStudentPopupVisible = false;
        [ObservableProperty]
        private Exam? _selectedExam;
        [ObservableProperty]
        private string _newStudentNumber = string.Empty;
        [ObservableProperty]
        private string _newStudentName = string.Empty;
        [ObservableProperty]
        private bool _isEditingExam = false;
        [ObservableProperty]
        private int _editingExamId;
        [ObservableProperty]
        private string _term = string.Empty;
        [ObservableProperty]
        private string _courseName = string.Empty;
        [ObservableProperty]
        private DateTime _date = DateTime.Today;
        [ObservableProperty]
        private int _numberOfQuestions;
        [ObservableProperty]
        private int _examDurationMinutes;
        [ObservableProperty]
        private TimeSpan _startTime = DateTime.Now.TimeOfDay;
        [ObservableProperty]
        private string _formTitle = "Opret Ny Eksamen";
        [ObservableProperty]
        private string _formButtonText = "Opret Eksamen";

        public DashboardViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task OnAppearing()
        {
            await LoadUpcomingExamsAsync();
        }

        [RelayCommand]
        private async Task LoadUpcomingExamsAsync()
        {
            var exams = await _databaseService.GetExamsAsync(false);
            UpcomingExams.Clear();
            foreach (var exam in exams.OrderBy(e => e.Date))
            {
                var examDisplay = new ExamDisplay(exam);
                var students = await _databaseService.GetStudentsForExamAsync(exam.Id);
                
                examDisplay.StudentCount = students.Count;
                examDisplay.CompletedStudentCount = students.Count(s => !string.IsNullOrEmpty(s.Grade));
                
                UpcomingExams.Add(examDisplay);
            }
        }
        
        [RelayCommand]
        private void EditExam(ExamDisplay examDisplay)
        {
            if (examDisplay == null) return;
            var exam = examDisplay.Exam;
            
            IsEditingExam = true;
            EditingExamId = exam.Id;
            FormTitle = "Rediger Eksamen";
            FormButtonText = "Gem Ændringer";
            Term = exam.Term;
            CourseName = exam.CourseName;
            Date = exam.Date;
            StartTime = exam.StartTime;
            NumberOfQuestions = exam.NumberOfQuestions;
            ExamDurationMinutes = exam.ExamDurationMinutes;
        }
        
        [RelayCommand]
        private async Task DeleteExamAsync(ExamDisplay examDisplay)
        {
            if (examDisplay == null) return;
            var exam = examDisplay.Exam;
            
            bool confirmed = await Shell.Current.DisplayAlert("Bekræft Sletning", $"Er du sikker på, du vil slette eksamen '{exam.CourseName}' og alle tilmeldte studerende?", "Ja, Slet", "Annuller");
            if (confirmed)
            {
                await _databaseService.DeleteExamAsync(exam);
                await LoadUpcomingExamsAsync();
            }
        }
        
        [RelayCommand]
        private async Task ShowStudentPopupAsync(ExamDisplay examDisplay)
        {
            if (examDisplay == null) return;
            var exam = examDisplay.Exam;

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
        private async Task GoToExamAsync(ExamDisplay examDisplay)
        {
            if (examDisplay == null) return;
            var exam = examDisplay.Exam;
            await Shell.Current.GoToAsync($"{nameof(ExamView)}?ExamId={exam.Id}");
        }
        
        [RelayCommand]
        private async Task SaveExamAsync()
        {
            if (string.IsNullOrWhiteSpace(Term) || string.IsNullOrWhiteSpace(CourseName))
            {
                await Shell.Current.DisplayAlert("Fejl", "Udfyld venligst alle felter.", "OK");
                return;
            }

            Exam? exam;
            if (IsEditingExam)
            {
                exam = await _databaseService.GetExamAsync(EditingExamId);
            }
            else
            {
                exam = new Exam();
            }

            if (exam == null) return;

            exam.Term = Term;
            exam.CourseName = CourseName;
            exam.Date = Date;
            exam.NumberOfQuestions = NumberOfQuestions;
            exam.ExamDurationMinutes = ExamDurationMinutes;
            exam.StartTime = StartTime;

            await _databaseService.SaveExamAsync(exam);
            CancelEditExam();
            await LoadUpcomingExamsAsync();
        }

        [RelayCommand]
        private void CancelEditExam()
        {
            IsEditingExam = false;
            EditingExamId = 0;
            FormTitle = "Opret Ny Eksamen";
            FormButtonText = "Opret Eksamen";
            Term = string.Empty;
            CourseName = string.Empty;
            Date = DateTime.Today;
            NumberOfQuestions = 0;
            ExamDurationMinutes = 0;
            StartTime = DateTime.Now.TimeOfDay;
        }
        
        [RelayCommand]
        private void HideStudentPopup()
        {
            IsStudentPopupVisible = false;
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
        private async Task DeleteStudentAsync(Student student)
        {
            if (student == null) return;
            bool confirmed = await Shell.Current.DisplayAlert("Bekræft Sletning", $"Er du sikker på, du vil slette '{student.FullName}'?", "Ja, Slet", "Annuller");
            if (confirmed)
            {
                await _databaseService.DeleteStudentAsync(student);
                StudentsForSelectedExam.Remove(student);
            }
        }
    }
}