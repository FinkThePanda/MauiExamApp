using SQLite;
using MauiExamApp.Models;

namespace MauiExamApp.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;
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

        // Exam Methods
        public async Task<List<Exam>> GetExamsAsync(bool completed)
        {
            await InitializeAsync();
            return await _database!.Table<Exam>().Where(e => e.IsCompleted == completed).ToListAsync();
        }

        public async Task<Exam> GetExamAsync(int id)
        {
            await InitializeAsync();
            return await _database!.Table<Exam>().Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveExamAsync(Exam exam)
        {
            await InitializeAsync();
            if (exam.Id != 0)
            {
                return await _database!.UpdateAsync(exam);
            }
            else
            {
                return await _database!.InsertAsync(exam);
            }
        }
        
        public async Task<int> DeleteExamAsync(Exam exam)
        {
            await InitializeAsync();
            var students = await GetStudentsForExamAsync(exam.Id);
            foreach (var student in students)
            {
                await _database!.DeleteAsync(student);
            }
            return await _database!.DeleteAsync(exam);
        }
        
        // Student Methods
        public async Task<List<Student>> GetStudentsForExamAsync(int examId)
        {
            await InitializeAsync();
            return await _database!.Table<Student>().Where(s => s.ExamId == examId).OrderBy(s => s.Order).ToListAsync();
        }

        public async Task<int> SaveStudentAsync(Student student)
        {
            await InitializeAsync();
             if (student.Id != 0)
            {
                return await _database!.UpdateAsync(student);
            }
            else
            {
                var existingStudents = await GetStudentsForExamAsync(student.ExamId);
                student.Order = existingStudents.Count > 0 ? existingStudents.Max(s => s.Order) + 1 : 1;
                return await _database!.InsertAsync(student);
            }
        }
        
        public async Task<int> DeleteStudentAsync(Student student)
        {
            await InitializeAsync();
            return await _database!.DeleteAsync(student);
        }
    }
}