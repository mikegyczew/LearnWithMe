using Microsoft.Extensions.Logging;
using LearnWithMe.Application.Features.Profiles;
using LearnWithMe.Infrastructure;
using LearnWithMe.Presentation.ViewModels;

namespace LearnWithMe.Presentation;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddInfrastructure(
			Path.Combine(FileSystem.AppDataDirectory, "learnwithme.db3"));
		builder.Services.AddTransient<IChildProfileService, ChildProfileService>();
		builder.Services.AddTransient<ProfilesViewModel>();
		builder.Services.AddTransient<AlphabetLearningViewModel>();
		builder.Services.AddTransient<MathLearningViewModel>();
		builder.Services.AddTransient<HomeViewModel>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
