namespace JournalApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// Use Console instead of Debug
		System.Console.WriteLine($"\n\n***** DATABASE PATH: {JournalApp.Data.Constants.DatabasePath} *****\n\n");
  
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new MainPage()) { Title = "JournalApp" };
	}
}

