using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LearnWithMe.Application.Abstractions;
using LearnWithMe.Application.Features.Profiles;
using LearnWithMe.Domain.Entities;
using Microsoft.Maui.Controls;

namespace LearnWithMe.Presentation.ViewModels;

public sealed class ProfilesViewModel : INotifyPropertyChanged
{
    private readonly IChildProfileService _profileService;
    private string _newProfileName = string.Empty;
    private string? _selectedAvatar;
    private ChildProfile? _selectedProfile;
    private string _errorMessage = string.Empty;
    private bool _isBusy;
    private bool _isEmpty = true;

    public ProfilesViewModel(IChildProfileService profileService)
    {
        _profileService = profileService;
        AvatarOptions = ["🐻", "🐱", "🦊", "🐰", "🐼", "🦁"];
        LoadProfilesCommand = new Command(async () => await LoadProfilesAsync());
        AddProfileCommand = new Command(async () => await AddProfileAsync());
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ChildProfile> Profiles { get; } = [];

    public IReadOnlyList<string> AvatarOptions { get; }

    public Command LoadProfilesCommand { get; }

    public Command AddProfileCommand { get; }

    public string NewProfileName
    {
        get => _newProfileName;
        set => SetProperty(ref _newProfileName, value);
    }

    public string? SelectedAvatar
    {
        get => _selectedAvatar;
        set => SetProperty(ref _selectedAvatar, value);
    }

    public ChildProfile? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (SetProperty(ref _selectedProfile, value))
            {
                OnPropertyChanged(nameof(SelectedProfileDescription));
            }
        }
    }

    public string SelectedProfileDescription => SelectedProfile is null
        ? string.Empty
        : $"Dziś uczy się: {SelectedProfile.Name}";

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetProperty(ref _isEmpty, value);
    }

    public async Task LoadProfilesAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var profiles = await _profileService.GetProfilesAsync(cancellationToken);
            Profiles.Clear();
            foreach (var profile in profiles)
            {
                Profiles.Add(profile);
            }

            var selectedProfileId = SelectedProfile?.Id;
            SelectedProfile = Profiles.FirstOrDefault(profile => profile.Id == selectedProfileId)
                ?? Profiles.FirstOrDefault();

            IsEmpty = Profiles.Count == 0;
        }
        catch (ProfileStorageException)
        {
            ErrorMessage = "Nie udało się odczytać profili. Spróbuj ponownie.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task AddProfileAsync(CancellationToken cancellationToken = default)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            var profile = await _profileService.CreateProfileAsync(
                NewProfileName,
                SelectedAvatar,
                cancellationToken);

            Profiles.Add(profile);
            SelectedProfile = profile;
            NewProfileName = string.Empty;
            SelectedAvatar = null;
            IsEmpty = false;
        }
        catch (ArgumentException)
        {
            ErrorMessage = "Podaj imię o długości od 1 do 30 znaków.";
        }
        catch (ProfileStorageException)
        {
            ErrorMessage = "Nie udało się zapisać profilu. Spróbuj ponownie.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task DeleteProfileAsync(
        ChildProfile profile,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await _profileService.DeleteProfileAsync(profile.Id, cancellationToken);
            Profiles.Remove(profile);
            if (SelectedProfile?.Id == profile.Id)
            {
                SelectedProfile = Profiles.FirstOrDefault();
            }

            IsEmpty = Profiles.Count == 0;
        }
        catch (ProfileStorageException)
        {
            ErrorMessage = "Nie udało się usunąć profilu i jego postępów. Spróbuj ponownie.";
        }
        finally
        {
            IsBusy = false;
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
