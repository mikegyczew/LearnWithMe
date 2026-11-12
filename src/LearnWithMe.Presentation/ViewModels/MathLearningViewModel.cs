using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Math;
using LearnWithMe.Domain.Entities;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace LearnWithMe.Presentation.ViewModels;

public sealed class MathLearningViewModel : INotifyPropertyChanged
{
    private readonly IMathLearningService _learningService;
    private IReadOnlyList<MathTopicProgress> _progress = [];
    private Guid? _profileId;
    private MathExerciseOption _selectedExercise;
    private MathQuestion? _question;
    private string _feedback = string.Empty;
    private string _progressSummary = "Wybierz profil, aby zobaczyć postępy.";
    private string _topicProgress = string.Empty;
    private bool _isCorrectAnswerSelected;
    private bool _isAnswerInProgress;

    public MathLearningViewModel(IMathLearningService learningService)
    {
        _learningService = learningService;
        ExerciseTypes = learningService.ExerciseTypes;
        _selectedExercise = ExerciseTypes[0];
        Question = learningService.CreateQuestion(_selectedExercise.Topic);
        Options = [];
        NewQuestionCommand = new Command(CreateQuestion);
        AnswerCommand = new Command<int>(async answer => await AnswerAsync(answer));
        SpeakTextCommand = new Command<string>(async text => await SpeakTextAsync(text));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<MathExerciseOption> ExerciseTypes { get; }

    public ObservableCollection<int> Options { get; }

    public Command NewQuestionCommand { get; }

    public Command<int> AnswerCommand { get; }

    public Command<string> SpeakTextCommand { get; }

    public MathExerciseOption SelectedExercise
    {
        get => _selectedExercise;
        set
        {
            if (SetProperty(ref _selectedExercise, value))
            {
                OnPropertyChanged(nameof(IsNumberJourneyVisible));
                CreateQuestion();
                UpdateTopicProgress();
            }
        }
    }

    public MathQuestion? Question
    {
        get => _question;
        private set
        {
            if (SetProperty(ref _question, value))
            {
                OnPropertyChanged(nameof(HasQuestion));
            }
        }
    }

    public bool HasQuestion => Question is not null;

    public bool IsNumberJourneyVisible => SelectedExercise.Topic == MathTopic.Numbers;

    public string Feedback
    {
        get => _feedback;
        private set => SetProperty(ref _feedback, value);
    }

    public string ProgressSummary
    {
        get => _progressSummary;
        private set => SetProperty(ref _progressSummary, value);
    }

    public string TopicProgress
    {
        get => _topicProgress;
        private set => SetProperty(ref _topicProgress, value);
    }

    public bool IsCorrectAnswerSelected
    {
        get => _isCorrectAnswerSelected;
        private set
        {
            if (SetProperty(ref _isCorrectAnswerSelected, value))
            {
                OnPropertyChanged(nameof(CanAnswerQuestion));
            }
        }
    }

    public bool IsAnswerInProgress
    {
        get => _isAnswerInProgress;
        private set
        {
            if (SetProperty(ref _isAnswerInProgress, value))
            {
                OnPropertyChanged(nameof(CanAnswerQuestion));
            }
        }
    }

    public bool CanAnswerQuestion => !IsCorrectAnswerSelected && !IsAnswerInProgress;

    public async Task SetProfileAsync(Guid profileId)
    {
        _profileId = profileId;
        await RefreshProgressAsync();
        CreateQuestion();
    }

    public async Task RecordNumberTraceAsync(bool wasComplete)
    {
        if (_profileId is not { } profileId ||
            Question?.Topic != MathTopic.Numbers ||
            IsAnswerInProgress)
        {
            return;
        }

        IsAnswerInProgress = true;
        try
        {
            await _learningService.RecordAnswerAsync(profileId, MathTopic.Numbers, wasComplete);
            Feedback = wasComplete
                ? "Dobra robota! Pięknie narysowana liczba."
                : "Spróbuj ponownie — prowadź palec po jasnym śladzie.";
            await RefreshProgressAsync();
            if (wasComplete)
            {
                await Task.Delay(1400);
                CreateQuestion();
            }
        }
        catch (ProfileStorageException)
        {
            Feedback = "Nie udało się zapisać postępu. Spróbuj ponownie.";
        }
        finally
        {
            IsAnswerInProgress = false;
        }
    }

    public void ClearProfile()
    {
        _profileId = null;
        _progress = [];
        ProgressSummary = "Wybierz profil, aby zobaczyć postępy.";
        TopicProgress = string.Empty;
        Feedback = string.Empty;
        IsCorrectAnswerSelected = false;
    }

    private void CreateQuestion()
    {
        Question = _learningService.CreateQuestion(SelectedExercise.Topic);
        Options.Clear();
        foreach (var option in Question.Options)
        {
            Options.Add(option);
        }

        Feedback = string.Empty;
        IsCorrectAnswerSelected = false;
    }

    private async Task AnswerAsync(int answer)
    {
        if (_profileId is not { } profileId ||
            Question is not { } question ||
            IsCorrectAnswerSelected ||
            IsAnswerInProgress)
        {
            return;
        }

        var wasCorrect = answer == question.CorrectAnswer;
        IsAnswerInProgress = true;
        try
        {
            await _learningService.RecordAnswerAsync(profileId, question.Topic, wasCorrect);
            Feedback = wasCorrect
                ? question.Topic == MathTopic.Numbers
                    ? "Dobra robota! Teraz przeczytaj i narysuj tę liczbę."
                    : "Dobra robota! Za chwilę nowe zadanie."
                : "Spróbuj ponownie. Każda próba pomaga się uczyć.";
            IsCorrectAnswerSelected = wasCorrect;
            await RefreshProgressAsync();
            await Task.Delay(1300);
            if (wasCorrect && question.Topic != MathTopic.Numbers)
            {
                CreateQuestion();
            }
        }
        catch (ProfileStorageException)
        {
            Feedback = "Nie udało się zapisać postępu. Spróbuj ponownie.";
        }
        finally
        {
            IsAnswerInProgress = false;
        }
    }

    private static async Task SpeakTextAsync(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
        {
            Locale = new System.Globalization.CultureInfo("pl-PL")
        });
    }

    private async Task RefreshProgressAsync()
    {
        if (_profileId is not { } profileId)
        {
            return;
        }

        try
        {
            var progress = await _learningService.GetProgressAsync(profileId);
            _progress = progress;
            var attempts = progress.Sum(item => item.Attempts);
            var correct = progress.Sum(item => item.Correct);
            ProgressSummary = $"Wspólne próby: {attempts} · udane: {correct}";
            UpdateTopicProgress();
        }
        catch (ProfileStorageException)
        {
            ProgressSummary = "Nie udało się odczytać postępów matematycznych.";
            TopicProgress = string.Empty;
        }
    }

    private void UpdateTopicProgress()
    {
        var topic = _progress.FirstOrDefault(item => item.Topic == SelectedExercise.Topic);
        TopicProgress = $"W tej zabawie: {topic?.Correct ?? 0} udanych prób";
    }

    private bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
