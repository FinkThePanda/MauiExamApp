using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExamApp.Models;
using MauiExamApp.Services;
using System;
using System.Threading.Tasks;

namespace MauiExamApp.ViewModels
{
    public partial class ExamFormViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

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

        public event Func<Task>? ExamSaved;

        public ExamFormViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void PrepareForNewExam()
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

        public void PrepareForEdit(Exam exam)
        {
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
            
            PrepareForNewExam();

            if (ExamSaved != null)
            {
                await ExamSaved.Invoke();
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            PrepareForNewExam();
        }
    }
}
