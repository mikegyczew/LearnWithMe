using Microsoft.Extensions.DependencyInjection;

namespace LearnWithMe.Presentation;

public partial class App : Microsoft.Maui.Controls.Application
{
	private readonly IServiceProvider _serviceProvider;

	public App(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell(_serviceProvider));
	}
}
