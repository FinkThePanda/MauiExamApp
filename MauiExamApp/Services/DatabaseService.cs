using SQLite;
using MauiExamApp.Models;

namespace MauiExamApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;

        private async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Exams.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<Exam>();
            await _database.CreateTableAsync<Student>();
            _isInitialized = true;
        }

        // Exam Methods (Get/Save er uændrede)
        public async Task<List<Exam>> GetExamsAsync(bool completed) { /* ... */ }
        public async Task<Exam> GetExamAsync(int id) { /* ... */ }
        public async Task<int> SaveExamAsync(Exam exam) { /* ... */ }

        // NY METODE: Slet eksamen og alle tilhørende studerende
        public async Task<int> DeleteExamAsync(Exam exam)
        {
            await InitializeAsync();
            // Slet først alle studerende tilknyttet denne eksamen
            var students = await GetStudentsForExamAsync(exam.Id);
            foreach (var student in students)
            {
                await _database.DeleteAsync(student);
            }
            // Slet derefter selve eksamen
            return await _database.DeleteAsync(exam);
        }

        // Student Methods (Get/Save er uændrede)
        public async Task<List<Student>> GetStudentsForExamAsync(int examId) { /* ... */ }
        public async Task<int> SaveStudentAsync(Student student) { /* ... */ }

        // NY METODE: Slet en specifik studerende
        public async Task<int> DeleteStudentAsync(Student student)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(student);
        }
    }
}