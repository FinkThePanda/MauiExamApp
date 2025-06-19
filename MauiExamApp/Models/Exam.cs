using SQLite;

namespace MauiExamApp.Models
{
    public class Exam
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Term { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int NumberOfQuestions { get; set; }
        public int ExamDurationMinutes { get; set; }
        public TimeSpan StartTime { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int CurrentStudentIndex { get; set; } = 0;
    }
}