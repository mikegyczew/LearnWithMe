using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Alphabet;
using LearnWithMe.Domain.Entities;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace LearnWithMe.Presentation.ViewModels;

public sealed class AlphabetLearningViewModel : INotifyPropertyChanged
{
    public sealed record ExerciseOption(string Name, ExerciseType Type);

    private readonly IAlphabetLearningService _learningService;
    private Guid? _profileId;
    private string _profileName = string.Empty;
    private AlphabetLetter? _selectedLetter;
    private ExerciseQuestion? _question;
    private ExerciseType _exerciseType = ExerciseType.FindLetter;
    private string _feedback = string.Empty;
    private string _progressSummary = "Wybierz profil, aby zobaczyć postępy.";
    private string _badgeSummary = string.Empty;
    private string _role = "Dziecko";
    private bool _isQuestionAnswered;
    private bool _isBusy;

    public AlphabetLearningViewModel(IAlphabetLearningService learningService)
    {
        _learningService = learningService;
        Letters = new ObservableCollection<AlphabetLetter>(AlphabetLetter.All);
        ExerciseTypes =
        [
            new("Znajdź literę", ExerciseType.FindLetter),
            new("Dopasuj obrazek do słowa", ExerciseType.MatchWord),
            new("Znajdź literę w słowie", ExerciseType.FindFirstLetter),
            new("Połącz wielką i małą literę", ExerciseType.MatchCase)
        ];
        SelectedExercise = ExerciseTypes[0];
        History = [];
        SelectLetterCommand = new Command<AlphabetLetter>(SelectLetter);
        MarkLearnedCommand = new Command(async () => await MarkLearnedAsync());
        SpeakCommand = new Command(async () => await SpeakAsync());
        NewQuestionCommand = new Command(CreateQuestion);
        AnswerCommand = new Command<string>(async answer => await AnswerAsync(answer));
        SwitchRoleCommand = new Command(SwitchRole);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<AlphabetLetter> Letters { get; }

    public ObservableCollection<string> History { get; }

    public IReadOnlyList<ExerciseOption> ExerciseTypes { get; }

    public Command<AlphabetLetter> SelectLetterCommand { get; }

    public Command MarkLearnedCommand { get; }

    public Command SpeakCommand { get; }

    public Command NewQuestionCommand { get; }

    public Command<string> AnswerCommand { get; }

    public Command SwitchRoleCommand { get; }

    public AlphabetLetter? SelectedLetter
    {
        get => _selectedLetter;
        private set
        {
            if (SetProperty(ref _selectedLetter, value))
            {
                OnPropertyChanged(nameof(HasSelectedLetter));
            }
        }
    }

    public bool HasSelectedLetter => SelectedLetter is not null;

    public ExerciseQuestion? Question
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

    public ExerciseOption SelectedExercise
    {
        get => ExerciseTypes.First(option => option.Type == _exerciseType);
        set
        {
            if (SetProperty(ref _exerciseType, value.Type))
            {
                OnPropertyChanged(nameof(SelectedExercise));
            }
        }
    }

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

    public string BadgeSummary
    {
        get => _badgeSummary;
        private set => SetProperty(ref _badgeSummary, value);
    }

    public string Role
    {
        get => _role;
        private set
        {
            if (SetProperty(ref _role, value))
            {
                OnPropertyChanged(nameof(RolePrompt));
            }
        }
    }

    public string RolePrompt => Role == "Dziecko"
        ? $"{_profileName}, teraz Twoja kolej. Rodzic może Ci pomóc!"
        : "Teraz Ty uczysz rodzica. Wybierz odpowiedź razem!";

    public bool IsQuestionAnswered
    {
        get => _isQuestionAnswered;
        private set => SetProperty(ref _isQuestionAnswered, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public async Task SetProfileAsync(Guid profileId, string profileName)
    {
        _profileId = profileId;
        _profileName = profileName;
        Role = "Dziecko";
        OnPropertyChanged(nameof(RolePrompt));
        await RefreshProgressAsync();
    }

    public void ClearProfile()
    {
        _profileId = null;
        _profileName = string.Empty;
        SelectedLetter = null;
        Question = null;
        Feedback = string.Empty;
        ProgressSummary = "Wybierz profil, aby zobaczyć postępy.";
        BadgeSummary = string.Empty;
        Role = "Dziecko";
    }

    public async Task RecordTraceAsync(bool wasComplete)
    {
        if (_profileId is not { } profileId || SelectedLetter is null)
        {
            return;
        }

        try
        {
            await _learningService.RecordAnswerAsync(
                profileId,
                new ExerciseQuestion(
                    ExerciseType.FindLetter,
                    "Rysowanie litery",
                    string.Empty,
                    [],
                    SelectedLetter.Uppercase),
                wasCorrect: wasComplete);
            Feedback = wasComplete
                ? "Świetnie ćwiczysz kształt litery! Możesz narysować ją jeszcze raz."
                : "Spróbuj dorysować brakujące części litery, zanim zakończysz.";
            await RefreshProgressAsync();
        }
        catch (ProfileStorageException)
        {
            Feedback = "Nie udało się zapisać ćwiczenia. Spróbuj ponownie.";
        }
    }

    private void SelectLetter(AlphabetLetter? letter)
    {
        if (letter is not null)
        {
            SelectedLetter = letter;
            Feedback = string.Empty;
        }
    }

    private async Task MarkLearnedAsync()
    {
        if (_profileId is not { } profileId || SelectedLetter is null || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            await _learningService.MarkLearnedAsync(profileId, SelectedLetter.Uppercase);
            Feedback = $"Brawo! Poznaliście literę {SelectedLetter.Uppercase}.";
            await RefreshProgressAsync();
        }
        catch (ProfileStorageException)
        {
            Feedback = "Nie udało się zapisać postępu. Spróbuj ponownie.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SpeakAsync()
    {
        if (SelectedLetter is null)
        {
            return;
        }

        try
        {
            var polishLocale = (await TextToSpeech.Default.GetLocalesAsync())
                .FirstOrDefault(locale =>
                    locale.Id.StartsWith("pl", StringComparison.OrdinalIgnoreCase) ||
                    locale.Language.Equals("pl", StringComparison.OrdinalIgnoreCase));

            if (polishLocale is null)
            {
                Feedback = "Na tym urządzeniu nie ma dostępnej polskiej wymowy.";
                return;
            }

            await TextToSpeech.Default.SpeakAsync(
                SelectedLetter.Pronunciation,
                new SpeechOptions { Locale = polishLocale });
        }
        catch (FeatureNotSupportedException)
        {
            Feedback = "Na tym urządzeniu nie ma dostępnego syntezatora mowy.";
        }
    }

    private void CreateQuestion()
    {
        Question = _learningService.CreateQuestion(_exerciseType);
        IsQuestionAnswered = false;
        Feedback = string.Empty;
    }

    private async Task AnswerAsync(string? answer)
    {
        if (answer is null || Question is null || _profileId is not { } profileId || IsQuestionAnswered || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var wasCorrect = string.Equals(answer, Question.CorrectAnswer, StringComparison.Ordinal);
            await _learningService.RecordAnswerAsync(profileId, Question, wasCorrect);
            IsQuestionAnswered = true;
            Feedback = wasCorrect
                ? "Świetnie! Każda próba pomaga się uczyć."
                : "Nic się nie stało — spróbuj ponownie przy kolejnym pytaniu.";
            await RefreshProgressAsync();
        }
        catch (ProfileStorageException)
        {
            Feedback = "Nie udało się zapisać odpowiedzi. Spróbuj ponownie.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void SwitchRole()
    {
        Role = Role == "Dziecko" ? "Rodzic" : "Dziecko";
        CreateQuestion();
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
            var learnedCount = progress.Count(item => item.IsLearned);
            var practiceCount = progress.Sum(item => item.PracticeCount);
            ProgressSummary = $"Poznane litery: {learnedCount} z {AlphabetLetter.All.Count}  •  Wspólne próby: {practiceCount}";
            BadgeSummary = learnedCount switch
            {
                >= 32 => "🏅 Mistrz alfabetu — poznaliście wszystkie litery!",
                >= 20 => "🌟 Wielki odkrywca — już ponad połowa alfabetu!",
                >= 5 => "🎈 Ciekawski odkrywca — pierwsze litery poznane!",
                >= 1 => "⭐ Pierwszy krok — każda litera to nowe odkrycie!",
                _ => "Każda próba się liczy — zacznijcie od wybranej litery."
            };

            var history = await _learningService.GetHistoryAsync(profileId);
            History.Clear();
            foreach (var record in history.Take(5))
            {
                History.Add(
                    $"{record.CreatedAtUtc.ToLocalTime():dd.MM HH:mm}  ·  {record.Letter}  ·  " +
                    (record.WasCorrect ? "udało się" : "próba"));
            }
        }
        catch (ProfileStorageException)
        {
            Feedback = "Nie udało się odczytać postępów. Spróbuj ponownie.";
        }
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
