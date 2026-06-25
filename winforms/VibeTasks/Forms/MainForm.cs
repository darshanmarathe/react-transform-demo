using VibeTasks.Models;
using VibeTasks.Services;

namespace VibeTasks.Forms;

public partial class MainForm : Form
{
    private readonly ApiTaskService _taskService = ApiConfig.Tasks;
    private DataGridView dgvTasks = null!;
    private ComboBox cmbFilterStatus = null!;
    private ComboBox cmbFilterPriority = null!;
    private CheckBox chkShowArchived = null!;
    private Button btnNew = null!;
    private Button btnEdit = null!;
    private Button btnComplete = null!;
    private Button btnArchive = null!;
    private Button btnRestore = null!;
    private Button btnDelete = null!;
    private Button btnUsers = null!;
    private Button btnImportExport = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel lblStatus = null!;

    public MainForm()
    {
        InitializeComponent();
        _ = LoadTasksAsync();
    }

    private void InitializeComponent()
    {
        Text = "Vibe Tasks";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1100, 650);

        var menuStrip = new MenuStrip();
        var fileMenu = new ToolStripMenuItem("&File");
        var usersItem = new ToolStripMenuItem("&Manage Users...");
        usersItem.Click += (_, _) => OpenUserForm();
        var importExportItem = new ToolStripMenuItem("&Import / Export...");
        importExportItem.Click += (_, _) => OpenImportExportForm();
        var exitItem = new ToolStripMenuItem("E&xit");
        exitItem.Click += (_, _) => Close();
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { usersItem, importExportItem, new ToolStripSeparator(), exitItem });
        menuStrip.Items.Add(fileMenu);
        Controls.Add(menuStrip);

        var toolbar = new Panel { Location = new Point(0, 27), Size = new Size(1100, 40), BackColor = SystemColors.ControlLight };

        btnNew = new Button { Text = "New Task", Location = new Point(10, 8), Size = new Size(90, 25) };
        btnNew.Click += BtnNew_Click!;

        btnEdit = new Button { Text = "Edit", Location = new Point(105, 8), Size = new Size(70, 25) };
        btnEdit.Click += BtnEdit_Click!;

        btnComplete = new Button { Text = "Complete", Location = new Point(180, 8), Size = new Size(80, 25) };
        btnComplete.Click += BtnComplete_Click!;

        btnArchive = new Button { Text = "Archive", Location = new Point(265, 8), Size = new Size(70, 25) };
        btnArchive.Click += BtnArchive_Click!;

        btnRestore = new Button { Text = "Restore", Location = new Point(340, 8), Size = new Size(70, 25), Visible = false };
        btnRestore.Click += BtnRestore_Click!;

        btnDelete = new Button { Text = "Delete", Location = new Point(415, 8), Size = new Size(70, 25) };
        btnDelete.Click += BtnDelete_Click!;

        btnUsers = new Button { Text = "Users", Location = new Point(500, 8), Size = new Size(70, 25) };
        btnUsers.Click += (_, _) => OpenUserForm();

        btnImportExport = new Button { Text = "Import/Export", Location = new Point(575, 8), Size = new Size(100, 25) };
        btnImportExport.Click += (_, _) => OpenImportExportForm();

        toolbar.Controls.AddRange(new Control[] { btnNew, btnEdit, btnComplete, btnArchive, btnRestore, btnDelete, btnUsers, btnImportExport });
        Controls.Add(toolbar);

        var filterPanel = new Panel { Location = new Point(0, 67), Size = new Size(1100, 35) };

        var lblFilterStatus = new Label { Text = "Status:", Location = new Point(10, 8), Size = new Size(45, 23) };
        cmbFilterStatus = new ComboBox
        {
            Location = new Point(55, 6),
            Size = new Size(100, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbFilterStatus.Items.AddRange(new[] { "All", "Todo", "InProgress", "Done" });
        cmbFilterStatus.SelectedIndex = 0;
        cmbFilterStatus.SelectedIndexChanged += (_, _) => _ = LoadTasksAsync();

        var lblFilterPriority = new Label { Text = "Priority:", Location = new Point(170, 8), Size = new Size(50, 23) };
        cmbFilterPriority = new ComboBox
        {
            Location = new Point(220, 6),
            Size = new Size(100, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbFilterPriority.Items.AddRange(new[] { "All", "Low", "Medium", "High", "Critical" });
        cmbFilterPriority.SelectedIndex = 0;
        cmbFilterPriority.SelectedIndexChanged += (_, _) => _ = LoadTasksAsync();

        chkShowArchived = new CheckBox { Text = "Show Archived", Location = new Point(340, 6), Size = new Size(110, 23) };
        chkShowArchived.CheckedChanged += (_, _) => _ = LoadTasksAsync();

        filterPanel.Controls.AddRange(new Control[] { lblFilterStatus, cmbFilterStatus, lblFilterPriority, cmbFilterPriority, chkShowArchived });
        Controls.Add(filterPanel);

        dgvTasks = new DataGridView
        {
            Location = new Point(0, 102),
            Size = new Size(1085, 490),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };
        dgvTasks.CellDoubleClick += (_, _) => BtnEdit_Click(null!, EventArgs.Empty);
        Controls.Add(dgvTasks);

        statusStrip = new StatusStrip();
        lblStatus = new ToolStripStatusLabel("Ready");
        statusStrip.Items.Add(lblStatus);
        Controls.Add(statusStrip);

        MainMenuStrip = menuStrip;
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            var tasks = await _taskService.GetAllAsync(chkShowArchived.Checked);

            var statusFilter = cmbFilterStatus.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                var status = Enum.Parse<TaskItemStatus>(statusFilter);
                tasks = tasks.Where(t => t.Status == status).ToList();
            }

            var priorityFilter = cmbFilterPriority.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(priorityFilter) && priorityFilter != "All")
            {
                var priority = Enum.Parse<TaskPriority>(priorityFilter);
                tasks = tasks.Where(t => t.Priority == priority).ToList();
            }

            dgvTasks.DataSource = null;
            dgvTasks.DataSource = tasks;

            foreach (DataGridViewColumn col in dgvTasks.Columns)
            {
                if (col.Name is "AssignedUser" or "AssignedUserId" or "Id")
                    continue;
                if (col.Name == "RecurrenceInterval")
                {
                    col.Visible = true;
                    col.HeaderText = "Recurrence";
                    continue;
                }
                if (col.Name == "RecurrenceCount")
                {
                    col.HeaderText = "Repeat";
                    continue;
                }
                col.Visible = true;
            }

            if (dgvTasks.Columns["AssignedUser"] is DataGridViewTextBoxColumn userCol)
            {
                userCol.Visible = true;
                userCol.HeaderText = "Assigned To";
            }

            bool hasArchived = tasks.Any(t => t.IsArchived);
            btnRestore.Visible = hasArchived && chkShowArchived.Checked;

            lblStatus.Text = $"{tasks.Count} task(s)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        using var form = new TaskForm();
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = LoadTasksAsync();
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        if (dgvTasks.SelectedRows[0].DataBoundItem is not TaskItem task) return;

        using var form = new TaskForm(task);
        if (form.ShowDialog(this) == DialogResult.OK)
            _ = LoadTasksAsync();
    }

    private async void BtnComplete_Click(object? sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        if (dgvTasks.SelectedRows[0].DataBoundItem is not TaskItem task) return;

        await _taskService.CompleteAsync(task.Id);
        await LoadTasksAsync();
    }

    private async void BtnArchive_Click(object? sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        if (dgvTasks.SelectedRows[0].DataBoundItem is not TaskItem task) return;

        await _taskService.ArchiveAsync(task.Id);
        await LoadTasksAsync();
    }

    private async void BtnRestore_Click(object? sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        if (dgvTasks.SelectedRows[0].DataBoundItem is not TaskItem task) return;

        await _taskService.RestoreAsync(task.Id);
        await LoadTasksAsync();
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (dgvTasks.SelectedRows.Count == 0) return;
        if (dgvTasks.SelectedRows[0].DataBoundItem is not TaskItem task) return;

        if (MessageBox.Show($"Delete '{task.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            await _taskService.DeleteAsync(task.Id);
            await LoadTasksAsync();
        }
    }

    private void OpenUserForm()
    {
        using var form = new UserForm();
        form.ShowDialog(this);
    }

    private void OpenImportExportForm()
    {
        using var form = new ImportExportForm();
        form.ShowDialog(this);
    }
}
