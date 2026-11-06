using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using MoonHard.AmvrosiaTaxSync.Desktop.ViewModels;

namespace MoonHard.AmvrosiaTaxSync.Desktop;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
        => await _viewModel.TestConnectionAsync(SqlPassword.Password);

    private async void InspectSchema_Click(object sender, RoutedEventArgs e)
        => await _viewModel.InspectSchemaAsync(SqlPassword.Password);

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => _viewModel.Cancel();

    private async void HandlePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.Cancel();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.T)
        {
            e.Handled = true;
            await _viewModel.TestConnectionAsync(SqlPassword.Password);
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.R)
        {
            e.Handled = true;
            await _viewModel.InspectSchemaAsync(SqlPassword.Password);
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
        {
            e.Handled = true;
            ServerInput.Focus();
            ServerInput.SelectAll();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.Cancel();
        _viewModel.SavePreferences();
        SqlPassword.Clear();
        base.OnClosing(e);
    }
}
