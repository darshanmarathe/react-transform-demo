using VibeTasks.Models;
using VibeTasks.Services;

namespace VibeTasks.Forms;

public partial class UserForm : Form
{
    private readonly ApiUserService _userService = ApiConfig.Users;
    private DataGridView dgvUsers = null!;
    private TextBox txtName = null!;
    private TextBox txtEmail = null!;
    private Button btnAdd = null!;
    private Button btnUpdate = null!;
    private Button btnDelete = null!;
    private int? _selectedUserId;

    public UserForm()
    {
        InitializeComponent();
        LoadUsersAsync();
    }

    private void InitializeComponent()
    {
        Text = "Manage Users";
        StartPosition = FormStartPosition.CenterParent;
        Size = new Size(600, 450);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var lblName = new Label { Text = "Name:", Location = new Point(12, 15), Size = new Size(60, 23) };
        txtName = new TextBox { Location = new Point(80, 12), Size = new Size(200, 23) };

        var lblEmail = new Label { Text = "Email:", Location = new Point(12, 45), Size = new Size(60, 23) };
        txtEmail = new TextBox { Location = new Point(80, 42), Size = new Size(200, 23) };

        btnAdd = new Button { Text = "Add", Location = new Point(300, 10), Size = new Size(80, 28) };
        btnAdd.Click += BtnAdd_Click!;

        btnUpdate = new Button { Text = "Update", Location = new Point(390, 10), Size = new Size(80, 28), Enabled = false };
        btnUpdate.Click += BtnUpdate_Click!;

        btnDelete = new Button { Text = "Delete", Location = new Point(480, 10), Size = new Size(80, 28), Enabled = false };
        btnDelete.Click += BtnDelete_Click!;

        dgvUsers = new DataGridView
        {
            Location = new Point(12, 80),
            Size = new Size(560, 310),
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        dgvUsers.SelectionChanged += DgvUsers_SelectionChanged!;

        Controls.AddRange(new Control[] { lblName, txtName, lblEmail, txtEmail, btnAdd, btnUpdate, btnDelete, dgvUsers });
    }

    private async void LoadUsersAsync()
    {
        var users = await _userService.GetAllAsync();
        dgvUsers.DataSource = users;
        if (dgvUsers.Columns["Tasks"] != null)
            dgvUsers.Columns["Tasks"].Visible = false;
    }

    private void DgvUsers_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvUsers.SelectedRows.Count > 0 && dgvUsers.SelectedRows[0].DataBoundItem is User user)
        {
            _selectedUserId = user.Id;
            txtName.Text = user.Name;
            txtEmail.Text = user.Email;
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
        }
        else
        {
            _selectedUserId = null;
            btnUpdate.Enabled = false;
            btnDelete.Enabled = false;
        }
    }

    private async void BtnAdd_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        await _userService.CreateAsync(txtName.Text.Trim(), txtEmail.Text.Trim());
        txtName.Clear();
        txtEmail.Clear();
        LoadUsersAsync();
    }

    private async void BtnUpdate_Click(object? sender, EventArgs e)
    {
        if (!_selectedUserId.HasValue || string.IsNullOrWhiteSpace(txtName.Text)) return;
        var user = await _userService.GetByIdAsync(_selectedUserId.Value);
        if (user != null)
        {
            user.Name = txtName.Text.Trim();
            user.Email = txtEmail.Text.Trim();
            await _userService.UpdateAsync(user);
            LoadUsersAsync();
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (!_selectedUserId.HasValue) return;
        if (MessageBox.Show("Delete this user?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            await _userService.DeleteAsync(_selectedUserId.Value);
            txtName.Clear();
            txtEmail.Clear();
            LoadUsersAsync();
        }
    }
}
