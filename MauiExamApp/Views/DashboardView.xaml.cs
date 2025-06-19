using MauiExamApp.ViewModels;

namespace MauiExamApp.Views;

public partial class DashboardView : ContentPage
{
	public DashboardView(DashboardViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}