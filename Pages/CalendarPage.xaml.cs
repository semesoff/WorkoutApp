using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using WorkoutApp.Data;
using WorkoutApp.Models;
using WorkoutApp.Windows;

namespace WorkoutApp.Pages
{
    public partial class CalendarPage : Page
    {
        private readonly WorkoutDbContext _context;
        private DateTime _currentDate;
        private bool _isMonthView = true;

        public CalendarPage(WorkoutDbContext context)
        {
            try
            {
                Debug.WriteLine("Initializing CalendarPage...");
                
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context), "WorkoutDbContext cannot be null");
                }

                InitializeComponent();
                _context = context;
                _currentDate = DateTime.Today;

                // Инициализируем элементы управления
                MonthViewRadio.IsChecked = true;
                WeekViewRadio.IsChecked = false;
                MonthView.Visibility = Visibility.Visible;
                WeekView.Visibility = Visibility.Collapsed;

                UpdateCalendarDisplay();
                Debug.WriteLine("CalendarPage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing CalendarPage: {ex}");
                MessageBox.Show($"Ошибка при инициализации календаря: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Перебрасываем исключение, чтобы MainWindow мог его обработать
            }
        }

        private void UpdateCalendarDisplay()
        {
            try
            {
                if (_context == null)
                {
                    throw new InvalidOperationException("Database context is not initialized");
                }

                if (_isMonthView)
                {
                    UpdateMonthView();
                }
                else
                {
                    UpdateWeekView();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating calendar display: {ex}");
                MessageBox.Show($"Ошибка при обновлении календаря: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMonthView()
        {
            try
            {
                // Очищаем старые элементы
                MonthCalendarGrid.Children.Clear();

                // Получаем первый день месяца
                var firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
                
                // Определяем смещение для первого дня (0 = понедельник, 6 = воскресенье)
                int offset = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;
                
                // Получаем количество дней в месяце
                int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
                
                // Обновляем заголовок
                PeriodText.Text = $"{_currentDate:MMMM yyyy}";

                // Получаем тренировки на месяц
                var startDate = firstDayOfMonth.AddDays(-offset);
                var endDate = firstDayOfMonth.AddMonths(1);
                
                List<Workout> workouts;
                try
                {
                    workouts = _context.Workouts
                        .Where(w => w.ScheduledDate >= startDate && w.ScheduledDate < endDate)
                        .OrderBy(w => w.StartTimeMinutes)
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading workouts: {ex}");
                    workouts = new List<Workout>();
                }

                // Создаем ячейки календаря
                for (int i = 0; i < 42; i++) // 6 недель * 7 дней
                {
                    var currentDate = firstDayOfMonth.AddDays(i - offset);
                    var dayWorkouts = workouts.Where(w => w.ScheduledDate?.Date == currentDate.Date).ToList();
                    
                    var dayCell = CreateDayCell(currentDate, dayWorkouts, currentDate.Month != _currentDate.Month);
                    Grid.SetRow(dayCell, i / 7);
                    Grid.SetColumn(dayCell, i % 7);
                    MonthCalendarGrid.Children.Add(dayCell);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateMonthView: {ex}");
                throw;
            }
        }

        private void UpdateWeekView()
        {
            try
            {
                // Очищаем старые элементы
                WeekCalendarGrid.Children.Clear();
                TimeGrid.RowDefinitions.Clear();

                // Получаем начало недели (понедельник)
                var weekStart = _currentDate.AddDays(-(int)_currentDate.DayOfWeek + (int)DayOfWeek.Monday);
                
                // Обновляем заголовок
                PeriodText.Text = $"{weekStart:dd.MM.yyyy} - {weekStart.AddDays(6):dd.MM.yyyy}";

                // Обновляем заголовки дней
                for (int i = 0; i < 7; i++)
                {
                    var date = weekStart.AddDays(i);
                    var header = (TextBlock)this.FindName($"Day{i + 1}Header");
                    if (header != null)
                    {
                        header.Text = $"{date:ddd, dd.MM}";
                    }
                }

                // Создаем строки для каждого часа
                for (int hour = 0; hour < 24; hour++)
                {
                    TimeGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) });
                    
                    // Добавляем метку времени
                    var timeLabel = new TextBlock
                    {
                        Text = $"{hour:00}:00",
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5, 0, 5, 0)
                    };
                    Grid.SetRow(timeLabel, hour);
                    TimeGrid.Children.Add(timeLabel);
                }

                // Получаем тренировки на неделю
                var weekEnd = weekStart.AddDays(7);
                List<Workout> workouts;
                try
                {
                    workouts = _context.Workouts
                        .Where(w => w.ScheduledDate >= weekStart && w.ScheduledDate < weekEnd)
                        .OrderBy(w => w.StartTimeMinutes)
                        .ToList();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error loading workouts: {ex}");
                    workouts = new List<Workout>();
                }

                // Добавляем тренировки
                foreach (var workout in workouts)
                {
                    if (workout.ScheduledDate.HasValue && workout.StartTimeMinutes.HasValue && workout.EndTimeMinutes.HasValue)
                    {
                        var workoutControl = CreateWorkoutControl(workout);
                        var dayIndex = ((int)workout.ScheduledDate.Value.DayOfWeek + 6) % 7 + 1;
                        
                        // Вычисляем начальный и конечный час
                        var startHour = workout.StartTimeMinutes.Value / 60;
                        var endHour = workout.EndTimeMinutes.Value / 60;
                        
                        // Если конечное время равно 0:00, считаем это как 24:00
                        if (endHour == 0)
                        {
                            endHour = 24;
                        }
                        
                        // Устанавливаем позицию и размер
                        Grid.SetColumn(workoutControl, dayIndex);
                        Grid.SetRow(workoutControl, startHour);
                        
                        // Вычисляем количество часовых блоков для тренировки
                        var rowSpan = endHour - startHour;
                        if (rowSpan <= 0) // Защита от некорректных данных
                        {
                            rowSpan = 1;
                        }
                        Grid.SetRowSpan(workoutControl, rowSpan);
                        
                        WeekCalendarGrid.Children.Add(workoutControl);
                        Debug.WriteLine($"Added workout {workout.Name} at day {dayIndex}, " +
                            $"from hour {startHour} to {endHour}, rowSpan: {rowSpan}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdateWeekView: {ex}");
                throw;
            }
        }

        private Border CreateDayCell(DateTime date, List<Workout> workouts, bool isOtherMonth)
        {
            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0.5),
                Margin = new Thickness(2),
                Background = isOtherMonth ? Brushes.LightGray : Brushes.White
            };

            var stackPanel = new StackPanel { Margin = new Thickness(5) };
            
            // Добавляем номер дня
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = date.Day.ToString(),
                FontWeight = date.Date == DateTime.Today.Date ? FontWeights.Bold : FontWeights.Normal,
                Foreground = date.Date == DateTime.Today.Date ? Brushes.Blue : Brushes.Black
            });

            // Добавляем тренировки
            foreach (var workout in workouts.Take(3)) // Показываем только первые 3 тренировки
            {
                var workoutBlock = new TextBlock
                {
                    Text = $"• {workout.Name}",
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    Foreground = Brushes.DarkBlue
                };
                stackPanel.Children.Add(workoutBlock);
            }

            if (workouts.Count > 3)
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = $"... ещё {workouts.Count - 3}", 
                    Foreground = Brushes.Gray 
                });
            }

            border.Child = stackPanel;
            border.MouseLeftButtonDown += (s, e) => DayCell_Click(date);
            
            return border;
        }

        private Border CreateWorkoutControl(Workout workout)
        {
            var startTime = workout.StartTimeMinutes.HasValue ? 
                TimeSpan.FromMinutes(workout.StartTimeMinutes.Value) : TimeSpan.Zero;
            var endTime = workout.EndTimeMinutes.HasValue ? 
                TimeSpan.FromMinutes(workout.EndTimeMinutes.Value) : startTime.Add(TimeSpan.FromHours(1));

            var border = new Border
            {
                BorderBrush = Brushes.DarkBlue,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(2),
                Padding = new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(200, 173, 216, 230)) // Полупрозрачный LightBlue
            };

            var stackPanel = new StackPanel();
            
            // Добавляем заголовок и кнопки управления
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock
            {
                Text = $"{startTime:hh\\:mm} - {endTime:hh\\:mm}",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 10, 0)
            });

            // Кнопки редактирования и удаления
            var buttonsPanel = new StackPanel 
            { 
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var editButton = new Button
            {
                Content = "✎",
                Padding = new Thickness(5, 0, 5, 0),
                Margin = new Thickness(0, 0, 5, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            editButton.Click += (s, e) => 
            {
                e.Handled = true;
                EditWorkout(workout);
            };

            var deleteButton = new Button
            {
                Content = "✖",
                Padding = new Thickness(5, 0, 5, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = Brushes.Red
            };
            deleteButton.Click += (s, e) => 
            {
                e.Handled = true;
                DeleteWorkout(workout);
            };

            buttonsPanel.Children.Add(editButton);
            buttonsPanel.Children.Add(deleteButton);

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(headerPanel, 0);
            Grid.SetColumn(buttonsPanel, 2);

            headerGrid.Children.Add(headerPanel);
            headerGrid.Children.Add(buttonsPanel);

            stackPanel.Children.Add(headerGrid);
            stackPanel.Children.Add(new TextBlock 
            { 
                Text = workout.Name,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 5, 0, 0)
            });
            
            if (!string.IsNullOrEmpty(workout.Description))
            {
                stackPanel.Children.Add(new TextBlock 
                { 
                    Text = workout.Description,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 0)
                });
            }

            border.Child = stackPanel;
            return border;
        }

        private void DeleteWorkout(Workout workout)
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить эту тренировку?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Workouts.Remove(workout);
                    _context.SaveChanges();
                    UpdateCalendarDisplay();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DeleteWorkout: {ex}");
                MessageBox.Show($"Ошибка при удалении тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DayCell_Click(DateTime date)
        {
            try
            {
                _currentDate = date;
                MonthViewRadio.IsChecked = false;
                WeekViewRadio.IsChecked = true;
                UpdateCalendarDisplay();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in DayCell_Click: {ex}");
                MessageBox.Show($"Ошибка при выборе дня: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewType_Changed(object sender, RoutedEventArgs e)
        {
            try
            {
                _isMonthView = MonthViewRadio.IsChecked ?? true;
                MonthView.Visibility = _isMonthView ? Visibility.Visible : Visibility.Collapsed;
                WeekView.Visibility = _isMonthView ? Visibility.Collapsed : Visibility.Visible;
                UpdateCalendarDisplay();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ViewType_Changed: {ex}");
                MessageBox.Show($"Ошибка при переключении вида: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviousPeriod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isMonthView)
                {
                    _currentDate = _currentDate.AddMonths(-1);
                }
                else
                {
                    _currentDate = _currentDate.AddDays(-7);
                }
                UpdateCalendarDisplay();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PreviousPeriod_Click: {ex}");
                MessageBox.Show($"Ошибка при переходе к предыдущему периоду: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NextPeriod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isMonthView)
                {
                    _currentDate = _currentDate.AddMonths(1);
                }
                else
                {
                    _currentDate = _currentDate.AddDays(7);
                }
                UpdateCalendarDisplay();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NextPeriod_Click: {ex}");
                MessageBox.Show($"Ошибка при переходе к следующему периоду: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddWorkoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new EditWorkoutWindow(_context, new Workout());
                if (window.ShowDialog() == true)
                {
                    UpdateCalendarDisplay();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AddWorkoutButton_Click: {ex}");
                MessageBox.Show($"Ошибка при добавлении тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditWorkout(Workout workout)
        {
            try
            {
                var window = new EditWorkoutWindow(_context, workout);
                if (window.ShowDialog() == true)
                {
                    UpdateCalendarDisplay();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in EditWorkout: {ex}");
                MessageBox.Show($"Ошибка при редактировании тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
