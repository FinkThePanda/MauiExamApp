using MauiExamApp.ViewModels;

namespace MauiExamApp.Views;

public partial class ExamView : ContentPage
{
	public ExamView(ExamViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}