using MauiExamApp.ViewModels;

namespace MauiExamApp.Views;

public partial class DashboardView : ContentPage
{
	private readonly DashboardViewModel _viewModel;
	public DashboardView(DashboardViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.OnAppearing(); // Kald ViewModel'ens metode
	}
}