using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExamApp.Models;
using MauiExamApp.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace MauiExamApp.ViewModels
{
    public partial class HistoryViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private List<ExamDisplay> _allCompletedExams; // Gemmer den ufiltrerede liste

        [ObservableProperty]
        private ObservableCollection<ExamDisplay> _completedExams;
        
        [ObservableProperty]
        private ObservableCollection<Student> _studentsForSelectedExam;

        [ObservableProperty]
        private bool _isDetailsPopupVisible = false;

        [ObservableProperty]
        private ExamDisplay? _selectedExamDisplay; // Changed from Exam to ExamDisplay

        [ObservableProperty]
        private string _averageGrade = string.Empty;
        
        [ObservableProperty]
        private ObservableCollection<string> _courseFilterOptions = new();

        [ObservableProperty]
        private ObservableCollection<string> _termFilterOptions = new();

        [ObservableProperty]
        private string _selectedCourseFilter = string.Empty;

        [ObservableProperty]
        private string _selectedTermFilter = string.Empty;

        [ObservableProperty]
        private string _selectedSortOrder = string.Empty;

        public ObservableCollection<string> SortOrderOptions { get; }

        public HistoryViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            
            _allCompletedExams = new List<ExamDisplay>();
            CompletedExams = new ObservableCollection<ExamDisplay>();
            StudentsForSelectedExam = new ObservableCollection<Student>();
            
            SortOrderOptions = new ObservableCollection<string> { "Nyeste først", "Ældste først" };
            SelectedSortOrder = SortOrderOptions.First();
        }

        [RelayCommand]
        private async Task ShowDetails(ExamDisplay examDisplay)
        {
            if (examDisplay == null) return;

            SelectedExamDisplay = examDisplay;
            var students = await _databaseService.GetStudentsForExamAsync(examDisplay.Exam.Id);
            StudentsForSelectedExam.Clear();
            foreach (var student in students)
            {
                StudentsForSelectedExam.Add(student);
            }

            CalculateAverageGrade();
            IsDetailsPopupVisible = true;
        }

        [RelayCommand]
        private void CloseDetailsPopup()
        {
            IsDetailsPopupVisible = false;
            SelectedExamDisplay = null;
        }

        private void CalculateAverageGrade()
        {
            if (StudentsForSelectedExam == null || !StudentsForSelectedExam.Any())
            {
                AverageGrade = "Ingen karakterer givet";
                return;
            }

            var grades = new List<int>();
            foreach (var student in StudentsForSelectedExam)
            {
                if (!string.IsNullOrEmpty(student.Grade) && int.TryParse(student.Grade, out int gradeValue))
                {
                    grades.Add(gradeValue);
                }
            }

            if (!grades.Any())
            {
                AverageGrade = "Ingen karakterer givet";
                return;
            }

            var average = grades.Average();
            AverageGrade = $"Gennemsnitskarakter: {average:F2}";
        }
        
        partial void OnSelectedCourseFilterChanged(string value) => ApplyFilters();
        partial void OnSelectedTermFilterChanged(string value) => ApplyFilters();
        partial void OnSelectedSortOrderChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            if (_allCompletedExams == null) return;

            IEnumerable<ExamDisplay> filtered = _allCompletedExams;

            if (!string.IsNullOrEmpty(SelectedCourseFilter) && SelectedCourseFilter != "Alle Kurser")
            {
                filtered = filtered.Where(e => e.Exam.CourseName == SelectedCourseFilter);
            }

            if (!string.IsNullOrEmpty(SelectedTermFilter) && SelectedTermFilter != "Alle Terminer")
            {
                filtered = filtered.Where(e => e.Exam.Term == SelectedTermFilter);
            }

            if (SelectedSortOrder == "Nyeste først")
            {
                filtered = filtered.OrderByDescending(e => e.Exam.Date);
            }
            else
            {
                filtered = filtered.OrderBy(e => e.Exam.Date);
            }

            CompletedExams.Clear();
            foreach (var exam in filtered)
            {
                CompletedExams.Add(exam);
            }
        }
        
        private async Task LoadAllDataAsync()
        {
            var examsFromDb = await _databaseService.GetExamsAsync(true);
            
            _allCompletedExams.Clear();
            foreach (var exam in examsFromDb)
            {
                var examDisplay = new ExamDisplay(exam);
                var students = await _databaseService.GetStudentsForExamAsync(exam.Id);
                examDisplay.StudentCount = students.Count;
                _allCompletedExams.Add(examDisplay);
            }

            var courses = _allCompletedExams.Select(e => e.Exam.CourseName).Distinct().ToList();
            CourseFilterOptions.Clear();
            CourseFilterOptions.Add("Alle Kurser");
            courses.ForEach(c => CourseFilterOptions.Add(c));
            SelectedCourseFilter = CourseFilterOptions.First();

            var terms = _allCompletedExams.Select(e => e.Exam.Term).Distinct().ToList();
            TermFilterOptions.Clear();
            TermFilterOptions.Add("Alle Terminer");
            terms.ForEach(t => TermFilterOptions.Add(t));
            SelectedTermFilter = TermFilterOptions.First();
            
            ApplyFilters();
        }
        
        public async Task OnAppearing()
        {
            await LoadAllDataAsync();
        }
    }
}