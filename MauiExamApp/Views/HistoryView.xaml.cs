using MauiExamApp.ViewModels;

namespace MauiExamApp.Views;

public partial class HistoryView : ContentPage
{
	public HistoryView(HistoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}