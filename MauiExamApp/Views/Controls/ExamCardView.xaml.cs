using MauiExamApp.Models;
using System.Windows.Input;

namespace MauiExamApp.Views.Controls;

public partial class ExamCardView : ContentView
{
    public static readonly BindableProperty ExamDisplayProperty =
        BindableProperty.Create(nameof(ExamDisplay), typeof(ExamDisplay), typeof(ExamCardView), null);

    public static readonly BindableProperty IsHistoryCardProperty =
        BindableProperty.Create(nameof(IsHistoryCard), typeof(bool), typeof(ExamCardView), false);

    public static readonly BindableProperty GoToExamCommandProperty =
        BindableProperty.Create(nameof(GoToExamCommand), typeof(ICommand), typeof(ExamCardView), null);

    public static readonly BindableProperty ShowStudentPopupCommandProperty =
        BindableProperty.Create(nameof(ShowStudentPopupCommand), typeof(ICommand), typeof(ExamCardView), null);

    public static readonly BindableProperty EditExamCommandProperty =
        BindableProperty.Create(nameof(EditExamCommand), typeof(ICommand), typeof(ExamCardView), null);

    public static readonly BindableProperty DeleteExamCommandProperty =
        BindableProperty.Create(nameof(DeleteExamCommand), typeof(ICommand), typeof(ExamCardView), null);

    public static readonly BindableProperty ShowDetailsCommandProperty =
        BindableProperty.Create(nameof(ShowDetailsCommand), typeof(ICommand), typeof(ExamCardView), null);

    public ExamDisplay ExamDisplay
    {
        get => (ExamDisplay)GetValue(ExamDisplayProperty);
        set => SetValue(ExamDisplayProperty, value);
    }

    public bool IsHistoryCard
    {
        get => (bool)GetValue(IsHistoryCardProperty);
        set => SetValue(IsHistoryCardProperty, value);
    }

    public ICommand GoToExamCommand
    {
        get => (ICommand)GetValue(GoToExamCommandProperty);
        set => SetValue(GoToExamCommandProperty, value);
    }

    public ICommand ShowStudentPopupCommand
    {
        get => (ICommand)GetValue(ShowStudentPopupCommandProperty);
        set => SetValue(ShowStudentPopupCommandProperty, value);
    }

    public ICommand EditExamCommand
    {
        get => (ICommand)GetValue(EditExamCommandProperty);
        set => SetValue(EditExamCommandProperty, value);
    }

    public ICommand DeleteExamCommand
    {
        get => (ICommand)GetValue(DeleteExamCommandProperty);
        set => SetValue(DeleteExamCommandProperty, value);
    }

    public ICommand ShowDetailsCommand
    {
        get => (ICommand)GetValue(ShowDetailsCommandProperty);
        set => SetValue(ShowDetailsCommandProperty, value);
    }

	public ExamCardView()
	{
		InitializeComponent();
	}
}
