using VibeTasks.Models;
using VibeTasks.Services;

namespace VibeTasks.Forms;

public partial class TaskForm : Form
{
    private readonly ApiUserService _userService = ApiConfig.Users;
    private TaskItem? _task;
    private ComboBox cmbStatus = null!;
    private ComboBox cmbPriority = null!;
    private ComboBox cmbAssignedUser = null!;
    private ComboBox cmbRecurrence = null!;
    private NumericUpDown numRecurrenceCount = null!;
    private CheckBox chkRecurring = null!;
    private DateTimePicker dtpDueDate = null!;
    private CheckBox chkHasDueDate = null!;
    private TextBox txtTitle = null!;
    private TextBox txtDescription = null!;

    public TaskItem? TaskItem => _task;

    private TaskForm()
    {
        InitializeComponent();
        LoadUsers();
    }

    public TaskForm(TaskItem task) : this()
    {
        _task = task;
        Text = "Edit Task";
        LoadTask();
    }

    public TaskForm(int? preSelectedUserId = null) : this()
    {
        if (preSelectedUserId.HasValue)
            cmbAssignedUser.SelectedValue = preSelectedUserId.Value;
    }

    private void InitializeComponent()
    {
        Text = "New Task";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Size = new Size(500, 500);

        var lblTitle = new Label { Text = "Title:", Location = new Point(12, 15), Size = new Size(80, 23) };
        txtTitle = new TextBox { Location = new Point(100, 12), Size = new Size(370, 23) };

        var lblDescription = new Label { Text = "Description:", Location = new Point(12, 45), Size = new Size(80, 23) };
        txtDescription = new TextBox
        {
            Location = new Point(100, 42),
            Size = new Size(370, 100),
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            AcceptsReturn = true,
            WordWrap = true
        };

        var lblStatus = new Label { Text = "Status:", Location = new Point(12, 155), Size = new Size(80, 23) };
        cmbStatus = new ComboBox
        {
            Location = new Point(100, 152),
            Size = new Size(150, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbStatus.Items.AddRange(Enum.GetNames<TaskItemStatus>());
        cmbStatus.SelectedIndex = 0;

        var lblPriority = new Label { Text = "Priority:", Location = new Point(270, 155), Size = new Size(80, 23) };
        cmbPriority = new ComboBox
        {
            Location = new Point(340, 152),
            Size = new Size(130, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbPriority.Items.AddRange(Enum.GetNames<TaskPriority>());
        cmbPriority.SelectedIndex = 1;

        chkHasDueDate = new CheckBox { Text = "Due Date:", Location = new Point(12, 190), Size = new Size(80, 23) };
        chkHasDueDate.CheckedChanged += (_, _) => dtpDueDate.Enabled = chkHasDueDate.Checked;
        dtpDueDate = new DateTimePicker
        {
            Location = new Point(100, 188),
            Size = new Size(150, 23),
            Enabled = false,
            Format = DateTimePickerFormat.Short
        };

        var lblAssignedUser = new Label { Text = "Assign To:", Location = new Point(12, 225), Size = new Size(80, 23) };
        cmbAssignedUser = new ComboBox
        {
            Location = new Point(100, 222),
            Size = new Size(200, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        chkRecurring = new CheckBox { Text = "Recurring", Location = new Point(12, 260), Size = new Size(100, 23) };
        chkRecurring.CheckedChanged += (_, _) =>
        {
            cmbRecurrence.Enabled = chkRecurring.Checked;
            numRecurrenceCount.Enabled = chkRecurring.Checked;
        };

        var lblRecurrence = new Label { Text = "Interval:", Location = new Point(120, 260), Size = new Size(60, 23) };
        cmbRecurrence = new ComboBox
        {
            Location = new Point(180, 258),
            Size = new Size(100, 23),
            DropDownStyle = ComboBoxStyle.DropDownList,
            Enabled = false
        };
        cmbRecurrence.Items.AddRange(Enum.GetNames<RecurrenceInterval>());
        cmbRecurrence.SelectedIndex = 0;

        var lblRecurCount = new Label { Text = "Count:", Location = new Point(290, 260), Size = new Size(50, 23) };
        numRecurrenceCount = new NumericUpDown
        {
            Location = new Point(340, 258),
            Size = new Size(50, 23),
            Minimum = 1,
            Maximum = 999,
            Enabled = false
        };

        var btnSave = new Button { Text = "Save", Location = new Point(100, 310), Size = new Size(100, 30) };
        btnSave.Click += BtnSave_Click!;

        var btnCancel = new Button { Text = "Cancel", Location = new Point(220, 310), Size = new Size(100, 30) };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        Controls.AddRange(new Control[]
        {
            lblTitle, txtTitle,
            lblDescription, txtDescription,
            lblStatus, cmbStatus,
            lblPriority, cmbPriority,
            chkHasDueDate, dtpDueDate,
            lblAssignedUser, cmbAssignedUser,
            chkRecurring,
            lblRecurrence, cmbRecurrence,
            lblRecurCount, numRecurrenceCount,
            btnSave, btnCancel
        });
    }

    private async void LoadUsers()
    {
        var users = await _userService.GetAllAsync();
        users.Insert(0, new User { Id = 0, Name = "-- Unassigned --" });
        cmbAssignedUser.DataSource = users;
        cmbAssignedUser.DisplayMember = "Name";
        cmbAssignedUser.ValueMember = "Id";
    }

    private void LoadTask()
    {
        if (_task == null) return;
        txtTitle.Text = _task.Title;
        txtDescription.Text = _task.Description;
        cmbStatus.SelectedItem = _task.Status.ToString();
        cmbPriority.SelectedItem = _task.Priority.ToString();
        cmbAssignedUser.SelectedValue = _task.AssignedUserId ?? 0;
        chkHasDueDate.Checked = _task.DueDate.HasValue;
        if (_task.DueDate.HasValue)
            dtpDueDate.Value = _task.DueDate.Value;
        chkRecurring.Checked = _task.IsRecurring;
        if (_task.RecurrenceInterval.HasValue)
            cmbRecurrence.SelectedItem = _task.RecurrenceInterval.Value.ToString();
        if (_task.RecurrenceCount.HasValue)
            numRecurrenceCount.Value = _task.RecurrenceCount.Value;
    }

    private async void BtnSave_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtTitle.Text))
        {
            MessageBox.Show("Title is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _task ??= new TaskItem();
        _task.Title = txtTitle.Text.Trim();
        _task.Description = txtDescription.Text.Trim();
        _task.Status = Enum.Parse<TaskItemStatus>(cmbStatus.SelectedItem!.ToString()!);
        _task.Priority = Enum.Parse<TaskPriority>(cmbPriority.SelectedItem!.ToString()!);
        _task.DueDate = chkHasDueDate.Checked ? dtpDueDate.Value : null;
        _task.AssignedUserId = (int)cmbAssignedUser.SelectedValue! == 0 ? null : (int)cmbAssignedUser.SelectedValue!;
        _task.IsRecurring = chkRecurring.Checked;
        _task.RecurrenceInterval = chkRecurring.Checked
            ? Enum.Parse<RecurrenceInterval>(cmbRecurrence.SelectedItem!.ToString()!)
            : null;
        _task.RecurrenceCount = chkRecurring.Checked ? (int)numRecurrenceCount.Value : null;

        var service = ApiConfig.Tasks;
        if (_task.Id == 0)
            _task = await service.CreateAsync(_task);
        else
            await service.UpdateAsync(_task);

        DialogResult = DialogResult.OK;
    }
}
