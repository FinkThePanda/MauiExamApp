using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExamApp.Models;
using MauiExamApp.Services;
using Plugin.Maui.Audio;
using System.Collections.ObjectModel;

namespace MauiExamApp.ViewModels
{
    [QueryProperty(nameof(ExamId), "ExamId")]
    public partial class ExamViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly IAudioManager _audioManager;
        private IDispatcherTimer? _timer;

        [ObservableProperty]
        private int _examId;

        [ObservableProperty]
        private Exam? _currentExam;

        [ObservableProperty]
        private Student? _currentStudent;

        private List<Student> _studentList = new();

        [ObservableProperty]
        private bool _isExamFinished = false;

        [ObservableProperty]
        private int? _drawnQuestion;

        [ObservableProperty]
        private string? _notes;
        
        [ObservableProperty]
        private string? _selectedGrade;
        
        public ObservableCollection<string> Grades { get; set; }

        [ObservableProperty]
        private string _timerText = "00:00";

        [ObservableProperty]
        private double _timeProgress = 0.0;
        
        [ObservableProperty]
        private bool _isTimerRunning = false;
        
        private int _timeRemainingSeconds;
        
        public ExamViewModel(DatabaseService databaseService, IAudioManager audioManager)
        {
            _databaseService = databaseService;
            _audioManager = audioManager;
            Grades = new ObservableCollection<string> { "-3", "00", "02", "4", "7", "10", "12" };
        }

        async partial void OnExamIdChanged(int value)
        {
            await InitializeExamAsync();
        }

        private async Task InitializeExamAsync()
        {
            CurrentExam = await _databaseService.GetExamAsync(ExamId);
            if (CurrentExam == null) return;

            _studentList = await _databaseService.GetStudentsForExamAsync(ExamId);
            
            LoadCurrentStudent();
            SetupTimer();
        }

        private void LoadCurrentStudent()
        {
            if (CurrentExam == null) return;
            if (CurrentExam.CurrentStudentIndex < _studentList.Count)
            {
                CurrentStudent = _studentList[CurrentExam.CurrentStudentIndex];
                DrawnQuestion = CurrentStudent.QuestionNumber;
                Notes = CurrentStudent.Notes;
                SelectedGrade = CurrentStudent.Grade;
                TimerText = $"{CurrentExam.ExamDurationMinutes:00}:00";
                IsTimerRunning = false;
                TimeProgress = 1.0;
            }
            else
            {
                IsExamFinished = true;
            }
        }

        [RelayCommand]
        private void DrawQuestion()
        {
            if (CurrentExam == null) return;
            var random = new Random();
            DrawnQuestion = random.Next(1, CurrentExam.NumberOfQuestions + 1);
        }

        [RelayCommand]
        private void StartExamination()
        {
            if (IsTimerRunning || CurrentExam == null) return;
            
            _timeRemainingSeconds = CurrentExam.ExamDurationMinutes * 60;
            _timer?.Start();
            IsTimerRunning = true;
        }
        
        [RelayCommand]
        private void StopExamination()
        {
            if (!IsTimerRunning || CurrentStudent == null || CurrentExam == null) return;
            _timer?.Stop();
            IsTimerRunning = false;
            
            int totalSeconds = CurrentExam.ExamDurationMinutes * 60;
            CurrentStudent.ActualExamTimeMinutes = (totalSeconds - _timeRemainingSeconds) / 60;
        }

        [RelayCommand]
        private async Task SaveAndNextStudentAsync()
        {
            if (CurrentStudent == null || CurrentExam == null) return;

            StopExamination();
            
            CurrentStudent.QuestionNumber = DrawnQuestion;
            CurrentStudent.Notes = Notes;
            CurrentStudent.Grade = SelectedGrade;
            await _databaseService.SaveStudentAsync(CurrentStudent);
            
            CurrentExam.CurrentStudentIndex++;
            await _databaseService.SaveExamAsync(CurrentExam);

            LoadCurrentStudent();
        }

        [RelayCommand]
        private async Task FinishExamAndGoBack()
        {
            if (CurrentExam == null) return;
            
            CurrentExam.IsCompleted = true;
            await _databaseService.SaveExamAsync(CurrentExam);
            
            await Shell.Current.GoToAsync("..");
        }

        private void SetupTimer()
        {
            if (_timer != null) return;
            
            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += async (s, e) =>
            {
                _timeRemainingSeconds--;
                TimeSpan time = TimeSpan.FromSeconds(_timeRemainingSeconds);
                TimerText = $"{time.Minutes:00}:{time.Seconds:00}";

                if(CurrentExam != null && CurrentExam.ExamDurationMinutes > 0)
                {
                    double totalDuration = CurrentExam.ExamDurationMinutes * 60;
                    TimeProgress = (_timeRemainingSeconds) / totalDuration;
                }

                if (_timeRemainingSeconds <= 0)
                {
                    _timer.Stop();
                    IsTimerRunning = false;
                    TimeProgress = 0;
                    
                    await PlayAlarmSoundAsync(); 

                    await Shell.Current.DisplayAlert("Tiden er gået!", "Eksaminationstiden er udløbet.", "OK");
                }
            };
        }

        private async Task PlayAlarmSoundAsync()
        {
            try
            {
                var player = _audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("timer_alarm.mp3"));
                player.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}