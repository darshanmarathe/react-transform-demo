using System.Windows;
using VibeTasks.Wpf.Models;
using VibeTasks.Wpf.Services;

namespace VibeTasks.Wpf.Dialogs;

public partial class TaskDialog : Window
{
    private readonly TaskItem _task;

    public TaskDialog(TaskItem? task = null)
    {
        InitializeComponent();
        _task = task ?? new TaskItem();
        Title = _task.Id == 0 ? "New Task" : "Edit Task";
        Loaded += TaskDialog_Loaded;
    }

    private async void TaskDialog_Loaded(object sender, RoutedEventArgs e)
    {
        StatusBox.ItemsSource = Enum.GetValues<TaskItemStatus>();
        PriorityBox.ItemsSource = Enum.GetValues<TaskPriority>();
        RecurrenceBox.ItemsSource = Enum.GetValues<RecurrenceInterval>();

        var users = await ApiConfig.Users.GetAllAsync();
        users.Insert(0, new User { Id = 0, Name = "-- Unassigned --" });
        UserBox.ItemsSource = users;

        TitleBox.Text = _task.Title;
        DescriptionBox.Text = _task.Description;
        StatusBox.SelectedItem = _task.Status;
        PriorityBox.SelectedItem = _task.Priority;
        UserBox.SelectedValue = _task.AssignedUserId ?? 0;
        HasDueDateBox.IsChecked = _task.DueDate.HasValue;
        DueDatePicker.SelectedDate = _task.DueDate;
        RecurringBox.IsChecked = _task.IsRecurring;
        RecurrenceBox.SelectedItem = _task.RecurrenceInterval ?? RecurrenceInterval.Daily;
        RecurrenceCountBox.Text = (_task.RecurrenceCount ?? 1).ToString();
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TitleBox.Text))
        {
            MessageBox.Show(this, "Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (RecurringBox.IsChecked == true && !int.TryParse(RecurrenceCountBox.Text, out _))
        {
            MessageBox.Show(this, "Recurring count must be a number.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _task.Title = TitleBox.Text.Trim();
        _task.Description = DescriptionBox.Text.Trim();
        _task.Status = (TaskItemStatus)StatusBox.SelectedItem;
        _task.Priority = (TaskPriority)PriorityBox.SelectedItem;
        _task.DueDate = HasDueDateBox.IsChecked == true ? DueDatePicker.SelectedDate : null;
        _task.AssignedUserId = Convert.ToInt32(UserBox.SelectedValue) == 0 ? null : Convert.ToInt32(UserBox.SelectedValue);
        _task.IsRecurring = RecurringBox.IsChecked == true;
        _task.RecurrenceInterval = _task.IsRecurring ? (RecurrenceInterval)RecurrenceBox.SelectedItem : null;
        _task.RecurrenceCount = _task.IsRecurring ? int.Parse(RecurrenceCountBox.Text) : null;

        try
        {
            if (_task.Id == 0)
                await ApiConfig.Tasks.CreateAsync(_task);
            else
                await ApiConfig.Tasks.UpdateAsync(_task);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Could not save task: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
