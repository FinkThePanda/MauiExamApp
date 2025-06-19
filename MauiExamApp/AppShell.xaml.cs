using MauiExamApp.Views;

namespace MauiExamApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        // Registrer ruter for navigation
        Routing.RegisterRoute(nameof(ExamView), typeof(ExamView));
        Routing.RegisterRoute(nameof(HistoryView), typeof(HistoryView));
	}
}