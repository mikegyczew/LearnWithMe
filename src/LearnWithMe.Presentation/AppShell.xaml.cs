using Microsoft.Extensions.DependencyInjection;

namespace LearnWithMe.Presentation;

public partial class AppShell : Shell
{
	public AppShell(IServiceProvider serviceProvider)
	{
		InitializeComponent();
		Items.Add(new ShellContent
		{
			Title = "Nauka",
			Content = serviceProvider.GetRequiredService<MainPage>(),
			Route = "Profiles"
		});
	}
}
