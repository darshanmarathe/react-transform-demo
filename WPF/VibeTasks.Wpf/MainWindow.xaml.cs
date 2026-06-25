using System.Windows;
using System.Windows.Controls;
using VibeTasks.Wpf.Dialogs;
using VibeTasks.Wpf.Models;
using VibeTasks.Wpf.Services;

namespace VibeTasks.Wpf;

public partial class MainWindow : Window
{
    private List<TaskItem> _tasks = new();

    public MainWindow()
    {
        InitializeComponent();
        StatusFilter.ItemsSource = new[] { "All", "Todo", "InProgress", "Done" };
        PriorityFilter.ItemsSource = new[] { "All", "Low", "Medium", "High", "Critical" };
        StatusFilter.SelectedIndex = 0;
        PriorityFilter.SelectedIndex = 0;
        Loaded += async (_, _) => await LoadTasksAsync();
    }

    private async Task LoadTasksAsync()
    {
        try
        {
            StatusText.Text = "Loading tasks...";
            _tasks = await ApiConfig.Tasks.GetAllAsync(ArchivedFilter.IsChecked == true);
            ApplyFilters();
        }
        catch (Exception ex)
        {
            StatusText.Text = "API connection failed";
            MessageBox.Show(this,
                $"Could not load tasks from the API. Make sure the server is running at http://localhost:5000.\n\n{ex.Message}",
                "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<TaskItem> filtered = _tasks;

        if (StatusFilter.SelectedItem is string statusText && statusText != "All")
        {
            var status = Enum.Parse<TaskItemStatus>(statusText);
            filtered = filtered.Where(t => t.Status == status);
        }

        if (PriorityFilter.SelectedItem is string priorityText && priorityText != "All")
        {
            var priority = Enum.Parse<TaskPriority>(priorityText);
            filtered = filtered.Where(t => t.Priority == priority);
        }

        var visibleTasks = filtered.ToList();
        TasksGrid.ItemsSource = visibleTasks;
        StatusText.Text = $"{visibleTasks.Count} task(s)";
    }

    private async void Filter_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        if (sender == ArchivedFilter)
            await LoadTasksAsync();
        else
            ApplyFilters();
    }

    private async void NewTask_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new TaskDialog { Owner = this };
        if (dialog.ShowDialog() == true)
            await LoadTasksAsync();
    }

    private async void EditTask_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTask() is not { } task)
            return;

        var dialog = new TaskDialog(task) { Owner = this };
        if (dialog.ShowDialog() == true)
            await LoadTasksAsync();
    }

    private async void Complete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTask() is not { } task)
            return;

        await ApiConfig.Tasks.CompleteAsync(task.Id);
        await LoadTasksAsync();
    }

    private async void Archive_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTask() is not { } task)
            return;

        await ApiConfig.Tasks.ArchiveAsync(task.Id);
        await LoadTasksAsync();
    }

    private async void Restore_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTask() is not { } task)
            return;

        await ApiConfig.Tasks.RestoreAsync(task.Id);
        await LoadTasksAsync();
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTask() is not { } task)
            return;

        if (MessageBox.Show(this, $"Delete '{task.Title}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        await ApiConfig.Tasks.DeleteAsync(task.Id);
        await LoadTasksAsync();
    }

    private void Users_Click(object sender, RoutedEventArgs e)
    {
        new UserWindow { Owner = this }.ShowDialog();
    }

    private async void ImportExport_Click(object sender, RoutedEventArgs e)
    {
        new ImportExportWindow { Owner = this }.ShowDialog();
        await LoadTasksAsync();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TasksGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        EditTask_Click(sender, e);
    }

    private TaskItem? SelectedTask()
    {
        if (TasksGrid.SelectedItem is TaskItem task)
            return task;

        MessageBox.Show(this, "Select a task first.", "No Task Selected", MessageBoxButton.OK, MessageBoxImage.Information);
        return null;
    }
}
