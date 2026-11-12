using System.ComponentModel;
using System.Runtime.CompilerServices;
using LearnWithMe.Domain.Entities;
using Microsoft.Maui.Controls;

namespace LearnWithMe.Presentation.ViewModels;

public sealed class HomeViewModel : INotifyPropertyChanged
{
    private bool _isLearningVisible;
    private bool _isMathVisible;

    public HomeViewModel(
        ProfilesViewModel profiles,
        AlphabetLearningViewModel learning,
        MathLearningViewModel math)
    {
        Profiles = profiles;
        Learning = learning;
        Math = math;
        StartLearningCommand = new Command(async () => await ActivateSelectedProfileAsync());
        ShowMathZoneCommand = new Command(async () => await ShowMathZoneAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ProfilesViewModel Profiles { get; }

    public AlphabetLearningViewModel Learning { get; }

    public MathLearningViewModel Math { get; }

    public Command StartLearningCommand { get; }

    public Command ShowMathZoneCommand { get; }

    public bool IsLearningVisible
    {
        get => _isLearningVisible;
        private set
        {
            if (_isLearningVisible != value)
            {
                _isLearningVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsMathVisible
    {
        get => _isMathVisible;
        private set
        {
            if (_isMathVisible != value)
            {
                _isMathVisible = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task LoadAsync()
    {
        await Profiles.LoadProfilesAsync();
        if (Profiles.SelectedProfile is not null)
        {
            await ActivateSelectedProfileAsync();
        }
    }

    public async Task ActivateSelectedProfileAsync()
    {
        if (Profiles.SelectedProfile is not ChildProfile profile)
        {
            Learning.ClearProfile();
            Math.ClearProfile();
            IsLearningVisible = false;
            IsMathVisible = false;
            return;
        }

        await Learning.SetProfileAsync(profile.Id, profile.Name);
        await Math.SetProfileAsync(profile.Id);
        IsMathVisible = false;
        IsLearningVisible = true;
    }

    private async Task ShowMathZoneAsync()
    {
        await ActivateSelectedProfileAsync();
        IsMathVisible = IsLearningVisible;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
