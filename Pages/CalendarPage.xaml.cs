using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WorkoutApp.Models;
using WorkoutApp.ViewModels;
using WorkoutApp.Data;
using WorkoutApp.Windows;

namespace WorkoutApp.Pages
{
    public partial class CalendarPage : Page
    {
        private readonly WorkoutDbContext _context;
        private DateTime _currentDate;
        private bool _isWeeklyView;

        public CalendarPage(WorkoutDbContext context)
        {
            InitializeComponent();
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _currentDate = DateTime.Now;
            _isWeeklyView = true;
            InitializeWeeklyView();
            UpdateView();
        }

        private void ToggleView_Click(object sender, RoutedEventArgs e)
        {
            _isWeeklyView = !_isWeeklyView;
            WeeklyView.Visibility = _isWeeklyView ? Visibility.Visible : Visibility.Collapsed;
            MonthView.Visibility = !_isWeeklyView ? Visibility.Visible : Visibility.Collapsed;
            UpdateView();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = DateTime.Now;
            UpdateView();
        }

        private void UpdateView()
        {
            if (_isWeeklyView)
            {
                UpdateWeeklyView();
            }
            else
            {
                UpdateMonthView();
            }
        }

        private void InitializeWeeklyView()
        {
            try
            {
                // Populate time labels (24-hour format)
                var timeLabels = new List<string>();
                for (int hour = 0; hour < 24; hour++)
                {
                    timeLabels.Add($"{hour:D2}:00");
                }
                TimeLabels.ItemsSource = timeLabels;

                // Initialize day columns with empty collections
                MondayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();
                TuesdayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();
                WednesdayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();
                ThursdayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();
                FridayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();
                SaturdayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();
                SundayColumn.ItemsSource = new ObservableCollection<WorkoutViewModel>();

                UpdateWeeklyView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in InitializeWeeklyView: {ex}");
                MessageBox.Show($"Error initializing weekly view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateWeeklyView()
        {
            try
            {
                // Get start of week (Monday)
                var weekStart = _currentDate.AddDays(-(int)_currentDate.DayOfWeek + (int)DayOfWeek.Monday);
                var weekEnd = weekStart.AddDays(7);
                
                // Update period text
                PeriodText.Text = $"{weekStart:dd.MM.yyyy} - {weekStart.AddDays(6):dd.MM.yyyy}";

                // Get workouts for the week
                var workouts = _context.Workouts
                    .Where(w => w.ScheduledDate >= weekStart && w.ScheduledDate < weekEnd)
                    .OrderBy(w => w.StartTimeMinutes)
                    .ToList();

                Debug.WriteLine($"Found {workouts.Count} workouts for week {weekStart:dd.MM.yyyy} - {weekEnd:dd.MM.yyyy}");

                // Clear existing workouts
                var columns = new[] { MondayColumn, TuesdayColumn, WednesdayColumn, ThursdayColumn, FridayColumn, SaturdayColumn, SundayColumn };
                foreach (var column in columns)
                {
                    ((ObservableCollection<WorkoutViewModel>)column.ItemsSource).Clear();
                }

                // Add workouts to appropriate columns
                foreach (var workout in workouts)
                {
                    if (workout.ScheduledDate.HasValue)
                    {
                        var dayOfWeek = workout.ScheduledDate.Value.DayOfWeek;
                        var column = GetColumnForDay(dayOfWeek);
                        if (column != null)
                        {
                            var workoutVm = new WorkoutViewModel(workout);
                            ((ObservableCollection<WorkoutViewModel>)column.ItemsSource).Add(workoutVm);
                            Debug.WriteLine($"Added workout {workout.Name} to {dayOfWeek} column");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateWeeklyView: {ex}");
                MessageBox.Show($"Error updating weekly view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMonthView()
        {
            try
            {
                MonthCalendarGrid.Children.Clear();

                // Get the first day of the month
                var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                
                // Update period text
                PeriodText.Text = firstDayOfMonth.ToString("MMMM yyyy");

                // Calculate the first day to display (Monday before the first day of month if necessary)
                var firstDayToShow = firstDayOfMonth;
                while (firstDayToShow.DayOfWeek != DayOfWeek.Monday)
                {
                    firstDayToShow = firstDayToShow.AddDays(-1);
                }

                // Get workouts for the visible period
                var lastDayToShow = firstDayToShow.AddDays(42); // 6 weeks
                var workouts = _context.Workouts
                    .Where(w => w.ScheduledDate >= firstDayToShow && w.ScheduledDate < lastDayToShow)
                    .OrderBy(w => w.StartTimeMinutes)
                    .ToList()
                    .GroupBy(w => w.ScheduledDate?.Date)
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Create calendar cells
                var currentDate = firstDayToShow;
                for (int week = 0; week < 6; week++)
                {
                    for (int day = 0; day < 7; day++)
                    {
                        var isCurrentMonth = currentDate.Month == _currentDate.Month;
                        var dayWorkouts = workouts.ContainsKey(currentDate.Date) ? workouts[currentDate.Date] : new List<Workout>();
                        var cell = CreateDayCell(currentDate, dayWorkouts, !isCurrentMonth);
                        
                        Grid.SetRow(cell, week);
                        Grid.SetColumn(cell, day);
                        MonthCalendarGrid.Children.Add(cell);

                        currentDate = currentDate.AddDays(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateMonthView: {ex}");
                MessageBox.Show($"Error updating month view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Border CreateDayCell(DateTime date, List<Workout> workouts, bool isOtherMonth)
        {
            var border = new Border
            {
                BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"],
                BorderThickness = new Thickness(1),
                Margin = new Thickness(2),
                Background = isOtherMonth 
                    ? (SolidColorBrush)Application.Current.Resources["SecondaryBackground"]
                    : (SolidColorBrush)Application.Current.Resources["SurfaceBackground"],
                Padding = new Thickness(5)
            };

            var stackPanel = new StackPanel();
            
            // Add date header
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = date.Day.ToString(),
                FontWeight = date.Date == DateTime.Today.Date ? FontWeights.Bold : FontWeights.Normal,
                Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryForeground"]
            });

            // Add workouts
            foreach (var workout in workouts.Take(3))
            {
                var workoutBlock = new TextBlock
                {
                    Text = $"• {workout.Name}",
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryForeground"],
                    Opacity = 0.7
                };
                stackPanel.Children.Add(workoutBlock);
            }

            if (workouts.Count > 3)
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"... {workouts.Count - 3} more",
                    Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryForeground"]
                });
            }

            border.Child = stackPanel;
            border.MouseLeftButtonDown += (s, e) => DayCell_Click(date);
            
            return border;
        }

        private ItemsControl? GetColumnForDay(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => MondayColumn,
                DayOfWeek.Tuesday => TuesdayColumn,
                DayOfWeek.Wednesday => WednesdayColumn,
                DayOfWeek.Thursday => ThursdayColumn,
                DayOfWeek.Friday => FridayColumn,
                DayOfWeek.Saturday => SaturdayColumn,
                DayOfWeek.Sunday => SundayColumn,
                _ => null
            };
        }

        private void DayCell_Click(DateTime date)
        {
            try
            {
                _currentDate = date;
                if (!_isWeeklyView)
                {
                    _isWeeklyView = true;
                    WeeklyView.Visibility = Visibility.Visible;
                    MonthView.Visibility = Visibility.Collapsed;
                }
                UpdateView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DayCell_Click: {ex}");
                MessageBox.Show($"Error handling day click: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviousPeriod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isWeeklyView)
                {
                    _currentDate = _currentDate.AddDays(-7);
                }
                else
                {
                    _currentDate = _currentDate.AddMonths(-1);
                }
                UpdateView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PreviousPeriod_Click: {ex}");
                MessageBox.Show($"Error navigating to previous period: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextPeriod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isWeeklyView)
                {
                    _currentDate = _currentDate.AddDays(7);
                }
                else
                {
                    _currentDate = _currentDate.AddMonths(1);
                }
                UpdateView();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NextPeriod_Click: {ex}");
                MessageBox.Show($"Error navigating to next period: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new EditWorkoutWindow(_context, new Workout());
                if (window.ShowDialog() == true)
                {
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddWorkoutButton_Click: {ex}");
                MessageBox.Show($"Error adding workout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditWorkout(Workout workout)
        {
            try
            {
                var window = new EditWorkoutWindow(_context, workout);
                if (window.ShowDialog() == true)
                {
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in EditWorkout: {ex}");
                MessageBox.Show($"Error editing workout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
