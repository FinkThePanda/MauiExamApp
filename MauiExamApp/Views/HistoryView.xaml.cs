using MauiExamApp.ViewModels;

namespace MauiExamApp.Views;

public partial class HistoryView : ContentPage
{
	private readonly HistoryViewModel _viewModel;
	public HistoryView(HistoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		_viewModel = viewModel;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.OnAppearing();
	}
}

