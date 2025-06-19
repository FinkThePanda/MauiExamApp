using SQLite;

namespace MauiExamApp.Models
{
    public class Student
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int ExamId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int Order { get; set; }
        public int? QuestionNumber { get; set; }
        public int? ActualExamTimeMinutes { get; set; }
        public string? Notes { get; set; }
        public string? Grade { get; set; }
    }
}