using VibeTasks.Services;

namespace VibeTasks.Forms;

public partial class ImportExportForm : Form
{
    private readonly ApiExportImportService _exportImportService = ApiConfig.ExportImport;
    private CheckBox chkIncludeArchived = null!;
    private RadioButton rbCsv = null!;
    private RadioButton rbJson = null!;

    public ImportExportForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Import / Export";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(400, 250);

        var grpFormat = new GroupBox { Text = "Format", Location = new Point(12, 12), Size = new Size(360, 70) };
        rbCsv = new RadioButton { Text = "CSV", Location = new Point(20, 25), Size = new Size(100, 23), Checked = true };
        rbJson = new RadioButton { Text = "JSON", Location = new Point(130, 25), Size = new Size(100, 23) };
        grpFormat.Controls.AddRange(new Control[] { rbCsv, rbJson });

        chkIncludeArchived = new CheckBox
        {
            Text = "Include archived tasks",
            Location = new Point(20, 50),
            Size = new Size(200, 23)
        };
        grpFormat.Controls.Add(chkIncludeArchived);

        var btnExport = new Button { Text = "Export...", Location = new Point(12, 100), Size = new Size(170, 35) };
        btnExport.Click += BtnExport_Click!;

        var btnImport = new Button { Text = "Import...", Location = new Point(200, 100), Size = new Size(170, 35) };
        btnImport.Click += BtnImport_Click!;

        var btnClose = new Button { Text = "Close", Location = new Point(150, 150), Size = new Size(100, 30) };
        btnClose.Click += (_, _) => Close();

        Controls.AddRange(new Control[] { grpFormat, btnExport, btnImport, btnClose });
    }

    private async void BtnExport_Click(object? sender, EventArgs e)
    {
        var filter = rbCsv.Checked ? "CSV Files|*.csv" : "JSON Files|*.json";
        var ext = rbCsv.Checked ? "csv" : "json";
        using var dialog = new SaveFileDialog { Filter = filter, DefaultExt = ext, FileName = $"vibetasks_export.{ext}" };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            if (rbCsv.Checked)
                await _exportImportService.ExportToCsvAsync(dialog.FileName, chkIncludeArchived.Checked);
            else
                await _exportImportService.ExportToJsonAsync(dialog.FileName, chkIncludeArchived.Checked);
            MessageBox.Show("Export completed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async void BtnImport_Click(object? sender, EventArgs e)
    {
        var filter = rbCsv.Checked ? "CSV Files|*.csv" : "JSON Files|*.json";
        using var dialog = new OpenFileDialog { Filter = filter };
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var count = rbCsv.Checked
                    ? await _exportImportService.ImportFromCsvAsync(dialog.FileName)
                    : await _exportImportService.ImportFromJsonAsync(dialog.FileName);
                MessageBox.Show($"Imported {count} task(s) successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Import failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
