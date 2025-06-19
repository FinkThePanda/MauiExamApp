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
        private List<Exam> _allCompletedExams; // Gemmer den ufiltrerede liste

        [ObservableProperty]
        private ObservableCollection<Exam> _completedExams;
        
        [ObservableProperty]
        private ObservableCollection<Student> _studentsForSelectedExam;

        [ObservableProperty]
        private bool _isDetailsPopupVisible = false;

        [ObservableProperty]
        private Exam _selectedExam;

        [ObservableProperty]
        private string _averageGrade;
        
        // --- Nye Properties for Filtrering ---
        [ObservableProperty]
        private ObservableCollection<string> _courseFilterOptions;

        [ObservableProperty]
        private ObservableCollection<string> _termFilterOptions;

        // RETTELSE: Ændret fra nameof(FilteredExams) til nameof(CompletedExams)
        [ObservableProperty]
        private string _selectedCourseFilter;

        // RETTELSE: Ændret fra nameof(FilteredExams) til nameof(CompletedExams)
        [ObservableProperty]
        private string _selectedTermFilter;

        // RETTELSE: Ændret fra nameof(FilteredExams) til nameof(CompletedExams)
        [ObservableProperty]
        private string _selectedSortOrder;

        public ObservableCollection<string> SortOrderOptions { get; }

        public HistoryViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            
            // Initialiserer samlinger
            _allCompletedExams = new List<Exam>();
            CompletedExams = new ObservableCollection<Exam>();
            StudentsForSelectedExam = new ObservableCollection<Student>();
            CourseFilterOptions = new ObservableCollection<string>();
            TermFilterOptions = new ObservableCollection<string>();
            
            // Opsæt sorteringsmuligheder
            SortOrderOptions = new ObservableCollection<string> { "Nyeste først", "Ældste først" };
            _selectedSortOrder = SortOrderOptions.First();
        }

        // Denne metode kaldes, når en filter-property ændres
        partial void OnSelectedCourseFilterChanged(string value) => ApplyFilters();
        partial void OnSelectedTermFilterChanged(string value) => ApplyFilters();
        partial void OnSelectedSortOrderChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            if (_allCompletedExams == null) return;

            // Start med den fulde liste
            IEnumerable<Exam> filtered = _allCompletedExams;

            // Anvend kursusfilter
            if (!string.IsNullOrEmpty(SelectedCourseFilter) && SelectedCourseFilter != "Alle Kurser")
            {
                filtered = filtered.Where(e => e.CourseName == SelectedCourseFilter);
            }

            // Anvend terminfilter
            if (!string.IsNullOrEmpty(SelectedTermFilter) && SelectedTermFilter != "Alle Terminer")
            {
                filtered = filtered.Where(e => e.Term == SelectedTermFilter);
            }

            // Anvend sortering
            if (SelectedSortOrder == "Nyeste først")
            {
                filtered = filtered.OrderByDescending(e => e.Date);
            }
            else
            {
                filtered = filtered.OrderBy(e => e.Date);
            }

            // Opdater den synlige samling
            CompletedExams.Clear();
            foreach (var exam in filtered)
            {
                CompletedExams.Add(exam);
            }
        }
        
        private async Task LoadAllDataAsync()
        {
            _allCompletedExams = await _databaseService.GetExamsAsync(true); // Hent kun afsluttede

            // Udfyld filtermuligheder fra data
            var courses = _allCompletedExams.Select(e => e.CourseName).Distinct().ToList();
            CourseFilterOptions.Clear();
            CourseFilterOptions.Add("Alle Kurser");
            courses.ForEach(c => CourseFilterOptions.Add(c));
            _selectedCourseFilter = CourseFilterOptions.First();

            var terms = _allCompletedExams.Select(e => e.Term).Distinct().ToList();
            TermFilterOptions.Clear();
            TermFilterOptions.Add("Alle Terminer");
            terms.ForEach(t => TermFilterOptions.Add(t));
            _selectedTermFilter = TermFilterOptions.First();
            
            // Anvend de initiale filtre
            ApplyFilters();
        }
        
        [RelayCommand]
        private async Task ShowDetailsPopupAsync(Exam exam)
        {
            if (exam == null) return;
            
            SelectedExam = exam;
            var students = await _databaseService.GetStudentsForExamAsync(exam.Id);
            
            StudentsForSelectedExam.Clear();
            foreach (var s in students)
            {
                StudentsForSelectedExam.Add(s);
            }
            CalculateAverageGrade(students);
            IsDetailsPopupVisible = true;
        }

        [RelayCommand]
        private void HideDetailsPopup()
        {
            IsDetailsPopupVisible = false;
        }

        private void CalculateAverageGrade(List<Student> students)
        {
            var validGrades = students
                .Where(s => int.TryParse(s.Grade, out _))
                .Select(s => int.Parse(s.Grade))
                .ToList();
            
            if (validGrades.Any())
            {
                var average = validGrades.Average();
                AverageGrade = $"Gennemsnit: {average:F2}";
            }
            else
            {
                AverageGrade = "Gennemsnit: N/A";
            }
        }
        
        public async Task OnAppearing()
        {
            await LoadAllDataAsync();
        }
    }
}