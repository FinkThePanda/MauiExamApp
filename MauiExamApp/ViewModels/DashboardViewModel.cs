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

        public ExamFormViewModel ExamFormViewModel { get; }

        public DashboardViewModel(DatabaseService databaseService, ExamFormViewModel examFormViewModel)
        {
            _databaseService = databaseService;
            ExamFormViewModel = examFormViewModel;
            ExamFormViewModel.ExamSaved += async () => await LoadUpcomingExamsAsync();
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
            if (examDisplay?.Exam == null) return;
            ExamFormViewModel.PrepareForEdit(examDisplay.Exam);
        }
        
        [RelayCommand]
        private async Task DeleteExamAsync(ExamDisplay examDisplay)
        {
            if (examDisplay?.Exam == null) return;
            
            bool confirmed = await Shell.Current.DisplayAlert("Bekræft Sletning", $"Er du sikker på, du vil slette eksamen '{examDisplay.Exam.CourseName}' og alle tilmeldte studerende?", "Ja, Slet", "Annuller");
            if (confirmed)
            {
                await _databaseService.DeleteExamAsync(examDisplay.Exam);
                await LoadUpcomingExamsAsync();
            }
        }
        
        [RelayCommand]
        private async Task ShowStudentPopupAsync(ExamDisplay examDisplay)
        {
            if (examDisplay?.Exam == null) return;

            SelectedExam = examDisplay.Exam;
            var students = await _databaseService.GetStudentsForExamAsync(examDisplay.Exam.Id);
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
            if (examDisplay?.Exam == null) return;
            await Shell.Current.GoToAsync($"{nameof(ExamView)}?ExamId={examDisplay.Exam.Id}");
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
            StudentsForSelectedExam.Add(newStudent);

            NewStudentNumber = string.Empty;
            NewStudentName = string.Empty;

            var examDisplay = UpcomingExams.FirstOrDefault(e => e.Exam.Id == SelectedExam.Id);
            if (examDisplay != null)
            {
                examDisplay.StudentCount = StudentsForSelectedExam.Count;
            }
        }

        [RelayCommand]
        private async Task DeleteStudentAsync(Student student)
        {
            if (student == null) return;

            await _databaseService.DeleteStudentAsync(student);
            StudentsForSelectedExam.Remove(student);
            
            if (SelectedExam != null)
            {
                var examDisplay = UpcomingExams.FirstOrDefault(e => e.Exam.Id == SelectedExam.Id);
                if (examDisplay != null)
                {
                    examDisplay.StudentCount = StudentsForSelectedExam.Count;
                }
            }
        }
    }
}