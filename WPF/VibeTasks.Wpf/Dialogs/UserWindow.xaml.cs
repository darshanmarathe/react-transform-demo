using System.Windows;
using System.Windows.Controls;
using VibeTasks.Wpf.Models;
using VibeTasks.Wpf.Services;

namespace VibeTasks.Wpf.Dialogs;

public partial class UserWindow : Window
{
    private User? _selectedUser;

    public UserWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        UsersGrid.ItemsSource = await ApiConfig.Users.GetAllAsync();
    }

    private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedUser = UsersGrid.SelectedItem as User;
        NameBox.Text = _selectedUser?.Name ?? string.Empty;
        EmailBox.Text = _selectedUser?.Email ?? string.Empty;
    }

    private async void Add_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show(this, "Name is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        await ApiConfig.Users.CreateAsync(NameBox.Text.Trim(), EmailBox.Text.Trim());
        NameBox.Clear();
        EmailBox.Clear();
        await LoadUsersAsync();
    }

    private async void Update_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedUser == null || string.IsNullOrWhiteSpace(NameBox.Text))
            return;

        _selectedUser.Name = NameBox.Text.Trim();
        _selectedUser.Email = EmailBox.Text.Trim();
        await ApiConfig.Users.UpdateAsync(_selectedUser);
        await LoadUsersAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedUser == null)
            return;

        if (MessageBox.Show(this, "Delete this user?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        await ApiConfig.Users.DeleteAsync(_selectedUser.Id);
        NameBox.Clear();
        EmailBox.Clear();
        await LoadUsersAsync();
    }
}
