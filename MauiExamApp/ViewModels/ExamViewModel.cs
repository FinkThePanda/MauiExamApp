using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiExamApp.Models;
using MauiExamApp.Services;
using System.Collections.ObjectModel;
using Plugin.Maui.Audio; // Tilføj denne using

namespace MauiExamApp.ViewModels
{
    // Attributten her lader os modtage data fra navigationen.
    [QueryProperty(nameof(ExamId), "ExamId")]
    public partial class ExamViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private IDispatcherTimer _timer;
        private readonly IAudioManager _audioManager; // Tilføj denne

        // Properties til at holde styr på tilstanden
        [ObservableProperty]
        private int _examId;

        [ObservableProperty]
        private Exam _currentExam;

        [ObservableProperty]
        private Student _currentStudent;

        private List<Student> _studentList;

        [ObservableProperty]
        private bool _isExamFinished = false;

        // Properties for den aktuelle eksaminand
        [ObservableProperty]
        private int? _drawnQuestion;

        [ObservableProperty]
        private string _notes;

        [ObservableProperty]
        private string _selectedGrade;

        [ObservableProperty]
        public ObservableCollection<string> _grades;

        // Timer properties
        [ObservableProperty]
        private string _timerText = "00:00";

        [ObservableProperty]
        private double _timeProgress = 0.0;

        [ObservableProperty]
        private bool _isTimerRunning = false;

        private int _timeRemainingSeconds;

        public ExamViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _audioManager = audioManager; // Gem instansen af Lyd
            // Initialiserer listen over mulige karakterer
            Grades = new ObservableCollection<string> { "-3", "00", "02", "4", "7", "10", "12" };
        }

        // Denne metode kaldes automatisk, når ExamId bliver sat via navigation
        async partial void OnExamIdChanged(int value)
        {
            await InitializeExamAsync();
        }

        private async Task InitializeExamAsync()
        {
            CurrentExam = await _databaseService.GetExamAsync(ExamId);
            _studentList = await _databaseService.GetStudentsForExamAsync(ExamId);

            LoadCurrentStudent();
            SetupTimer();
        }

        private void LoadCurrentStudent()
        {
            if (CurrentExam.CurrentStudentIndex < _studentList.Count)
            {
                CurrentStudent = _studentList[CurrentExam.CurrentStudentIndex];
                // Nulstil felter for den nye studerende
                DrawnQuestion = CurrentStudent.QuestionNumber;
                Notes = CurrentStudent.Notes;
                SelectedGrade = CurrentStudent.Grade;
                TimerText = $"{CurrentExam.ExamDurationMinutes:00}:00";
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
            _timer.Start();
            IsTimerRunning = true;
        }

        [RelayCommand]
        private void StopExamination()
        {
            if (!IsTimerRunning) return;
            _timer.Stop();
            IsTimerRunning = false;
            // Registrer den faktiske tid brugt
            int totalSeconds = CurrentExam.ExamDurationMinutes * 60;
            CurrentStudent.ActualExamTimeMinutes = (totalSeconds - _timeRemainingSeconds) / 60;
        }

        [RelayCommand]
        private async Task SaveAndNextStudentAsync()
        {
            if (CurrentStudent == null) return;

            // Gem data for den nuværende studerende
            CurrentStudent.QuestionNumber = DrawnQuestion;
            CurrentStudent.Notes = Notes;
            CurrentStudent.Grade = SelectedGrade;
            await _databaseService.SaveStudentAsync(CurrentStudent);

            // Opdater eksamen til at pege på næste studerende
            CurrentExam.CurrentStudentIndex++;
            await _databaseService.SaveExamAsync(CurrentExam);

            // Gå videre til næste
            LoadCurrentStudent();
        }

        [RelayCommand]
        private async Task FinishExamAndGoBack()
        {
            // Marker eksamen som afsluttet i databasen
            CurrentExam.IsCompleted = true;
            await _databaseService.SaveExamAsync(CurrentExam);

            // Naviger tilbage til startsiden
            await Shell.Current.GoToAsync("..");
        }

        private void SetupTimer()
        {
            if (_timer == null)
            {
                _timer = Application.Current.Dispatcher.CreateTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += async (s, e) => // Gør Tick-eventen async
                {
                    _timeRemainingSeconds--;
                    TimeSpan time = TimeSpan.FromSeconds(_timeRemainingSeconds);
                    TimerText = $"{time.Minutes:00}:{time.Seconds:00}";

                    double totalDuration = CurrentExam.ExamDurationMinutes * 60;
                    TimeProgress = (_timeRemainingSeconds) / totalDuration;

                    if (_timeRemainingSeconds <= 0)
                    {
                        _timer.Stop();
                        IsTimerRunning = false;
                        TimeProgress = 0;

                        // AFSPIL LYD
                        await PlayAlarmSoundAsync();

                        await Shell.Current.DisplayAlert("Tiden er gået!", "Eksaminationstiden er udløbet.", "OK");
                    }
                };
            }
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
                // Håndter fejl, f.eks. hvis lydfilen ikke kan findes.
                System.Diagnostics.Debug.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}