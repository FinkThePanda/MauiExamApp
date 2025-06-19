using CommunityToolkit.Mvvm.ComponentModel;
using MauiExamApp.Models;

namespace MauiExamApp.Models
{
    // Denne klasse "pakker" vores Exam-model ind for at tilføje UI-specifikke properties.
    public partial class ExamDisplay : ObservableObject
    {
        public Exam Exam { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullStatusText))]
        private int _studentCount;
        
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FullStatusText))]
        private int _completedStudentCount;

        public string FullStatusText => $"Gennemførte: {CompletedStudentCount} / {StudentCount}";

        public ExamDisplay(Exam exam)
        {
            Exam = exam;
        }
    }
}