using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
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

    private void ConnectionPage_Click(object sender, RoutedEventArgs e)
        => _viewModel.PageIndex = 0;

    private void ExplorerPage_Click(object sender, RoutedEventArgs e)
        => _viewModel.PageIndex = 1;

    private async void CustomersPage_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.PageIndex = 2;
        await _viewModel.LoadCustomersAsync(SqlPassword.Password);
    }

    private async void LoadCustomers_Click(object sender, RoutedEventArgs e)
        => await _viewModel.LoadCustomersAsync(SqlPassword.Password);

    private async void PreviousCustomers_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Customers.HasPrevious)
            await _viewModel.LoadCustomersAsync(SqlPassword.Password, _viewModel.Customers.Page - 1);
    }

    private async void NextCustomers_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Customers.HasNext)
            await _viewModel.LoadCustomersAsync(SqlPassword.Password, _viewModel.Customers.Page + 1);
    }

    private void Theme_Click(object sender, RoutedEventArgs e)
        => _viewModel.ToggleTheme();

    private async void ImportUdl_Click(object sender, RoutedEventArgs e)
        => await ImportUdlAsync();

    private async Task ImportUdlAsync()
    {
        if (_viewModel.IsBusy)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Επιλογή SQL Server UDL",
            Filter = "Universal Data Link (*.udl)|*.udl",
            CheckFileExists = true,
            Multiselect = false,
            RestoreDirectory = true
        };

        if (dialog.ShowDialog(this) != true)
            return;

        var password = await _viewModel.ImportUdlAsync(dialog.FileName);
        if (password is not null)
            SqlPassword.Password = password;
    }

    private async void TestConnection_Click(object sender, RoutedEventArgs e)
        => await _viewModel.TestConnectionAsync(SqlPassword.Password);

    private async void InspectSchema_Click(object sender, RoutedEventArgs e)
        => await _viewModel.InspectSchemaAsync(SqlPassword.Password);

    private void Cancel_Click(object sender, RoutedEventArgs e)
        => _viewModel.Cancel();

    private void CertificateTrust_Click(object sender, RoutedEventArgs e)
        => ToggleCertificateTrust();

    private void ToggleCertificateTrust()
    {
        if (_viewModel.IsBusy)
            return;

        if (_viewModel.TrustServerCertificate)
        {
            _viewModel.DisableCertificateOverride();
            return;
        }

        var choice = MessageBox.Show(
            this,
            "Η επιλογή αυτή παρακάμπτει την επαλήθευση του πιστοποιητικού SQL Server.\n\n" +
            "Η μεταφορά εξακολουθεί να κρυπτογραφείται, αλλά δεν επαληθεύεται η ταυτότητα του server. " +
            "Αυτό αυξάνει τον κίνδυνο υποκλοπής από ενδιάμεσο επιτιθέμενο.\n\n" +
            "Να ενεργοποιηθεί ΜΟΝΟ για γνωστή, ελεγχόμενη δοκιμαστική εγκατάσταση; " +
            "Η επιλογή δεν αποθηκεύεται και απενεργοποιείται στην επόμενη εκκίνηση.",
            "Προσωρινή εξαίρεση επαλήθευσης TLS",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (choice == MessageBoxResult.Yes)
            _viewModel.EnableCertificateOverride();
    }

    private void CopyDiagnostics_Click(object sender, RoutedEventArgs e)
    {
        if (!_viewModel.HasSqlFailure)
            return;

        try
        {
            Clipboard.SetText(_viewModel.CopyableDiagnostic);
            _viewModel.SetStatusMessage("Η ασφαλής διάγνωση αντιγράφηκε στο πρόχειρο.");
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            _viewModel.SetStatusMessage("Το πρόχειρο των Windows δεν είναι διαθέσιμο. Δοκίμασε ξανά.");
        }
    }

    private async void HandlePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.Cancel();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
        {
            e.Handled = true;
            await ImportUdlAsync();
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.L)
        {
            e.Handled = true;
            _viewModel.ToggleTheme();
        }
        else if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.C)
        {
            e.Handled = true;
            ToggleCertificateTrust();
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D1)
        {
            e.Handled = true;
            _viewModel.PageIndex = 0;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D2)
        {
            e.Handled = true;
            _viewModel.PageIndex = 1;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.D3)
        {
            e.Handled = true;
            _viewModel.PageIndex = 2;
            await _viewModel.LoadCustomersAsync(SqlPassword.Password);
        }
        else if (e.Key == Key.Enter && ReferenceEquals(e.OriginalSource, CustomerSearchBox))
        {
            e.Handled = true;
            await _viewModel.LoadCustomersAsync(SqlPassword.Password);
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
            if (_viewModel.IsExplorerPage)
            {
                SchemaSearchBox.Focus();
                SchemaSearchBox.SelectAll();
            }
            else if (_viewModel.IsCustomersPage)
            {
                CustomerSearchBox.Focus();
                CustomerSearchBox.SelectAll();
            }
            else
            {
                ServerInput.Focus();
                ServerInput.SelectAll();
            }
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
