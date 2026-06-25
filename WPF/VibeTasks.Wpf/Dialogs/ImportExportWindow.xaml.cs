using System.Windows;
using Microsoft.Win32;
using VibeTasks.Wpf.Services;

namespace VibeTasks.Wpf.Dialogs;

public partial class ImportExportWindow : Window
{
    public ImportExportWindow()
    {
        InitializeComponent();
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        var isCsv = CsvRadio.IsChecked == true;
        var dialog = new SaveFileDialog
        {
            Filter = isCsv ? "CSV Files|*.csv" : "JSON Files|*.json",
            FileName = isCsv ? "vibetasks_export.csv" : "vibetasks_export.json"
        };

        if (dialog.ShowDialog(this) != true)
            return;

        if (isCsv)
            await ApiConfig.ExportImport.ExportToCsvAsync(dialog.FileName, IncludeArchivedBox.IsChecked == true);
        else
            await ApiConfig.ExportImport.ExportToJsonAsync(dialog.FileName, IncludeArchivedBox.IsChecked == true);

        MessageBox.Show(this, "Export completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        var isCsv = CsvRadio.IsChecked == true;
        var dialog = new OpenFileDialog
        {
            Filter = isCsv ? "CSV Files|*.csv" : "JSON Files|*.json"
        };

        if (dialog.ShowDialog(this) != true)
            return;

        var count = isCsv
            ? await ApiConfig.ExportImport.ImportFromCsvAsync(dialog.FileName)
            : await ApiConfig.ExportImport.ImportFromJsonAsync(dialog.FileName);

        MessageBox.Show(this, $"Imported {count} task(s).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
