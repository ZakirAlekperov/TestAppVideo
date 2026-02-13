using Avalonia.Controls;
using TestAppVideo.Presentation.Avalonia.ViewModels;

namespace TestAppVideo.Presentation.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.SetWindow(this);
        }
    }
}
