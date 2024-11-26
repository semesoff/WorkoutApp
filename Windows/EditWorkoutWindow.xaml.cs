using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WorkoutApp.Models;
using WorkoutApp.Data;
using System.Diagnostics;

namespace WorkoutApp.Windows
{
    public partial class EditWorkoutWindow : Window
    {
        private readonly WorkoutDbContext _context;
        private readonly Workout _workout;
        private bool _isNewWorkout;

        public EditWorkoutWindow(WorkoutDbContext context, Workout workout = null)
        {
            try
            {
                Debug.WriteLine("Initializing EditWorkoutWindow...");
                
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context), "Database context cannot be null");
                }

                InitializeComponent();
                _context = context;
                _isNewWorkout = workout == null;

                if (_isNewWorkout)
                {
                    _workout = new Workout
                    {
                        Name = "Новая тренировка",
                        ScheduledDate = DateTime.Today,
                        StartTimeMinutes = 9 * 60,
                        EndTimeMinutes = 10 * 60,
                        MuscleGroup = "Общая",
                        Sets = 3,
                        Repetitions = 12,
                        Duration = 60
                    };
                }
                else
                {
                    _workout = workout;
                }

                DataContext = _workout;
                InitializeTimeComboBoxes();
                Debug.WriteLine($"EditWorkoutWindow initialized successfully for workout: {_workout.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing EditWorkoutWindow: {ex}");
                MessageBox.Show($"Ошибка при инициализации окна: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void InitializeTimeComboBoxes()
        {
            try
            {
                // Заполняем комбобоксы для выбора времени
                for (int hour = 0; hour < 24; hour++)
                {
                    string timeStr = $"{hour:D2}:00";
                    StartTimeComboBox.Items.Add(timeStr);
                    EndTimeComboBox.Items.Add(timeStr);
                }

                // Устанавливаем выбранное время
                if (_workout.StartTimeMinutes.HasValue)
                {
                    var startHour = _workout.StartTimeMinutes.Value / 60;
                    StartTimeComboBox.SelectedItem = $"{startHour:D2}:00";
                    Debug.WriteLine($"Setting start time to {startHour:D2}:00");
                }
                if (_workout.EndTimeMinutes.HasValue)
                {
                    var endHour = _workout.EndTimeMinutes.Value / 60;
                    EndTimeComboBox.SelectedItem = $"{endHour:D2}:00";
                    Debug.WriteLine($"Setting end time to {endHour:D2}:00");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing time combo boxes: {ex}");
                MessageBox.Show($"Ошибка при инициализации выбора времени: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateWorkout())
                {
                    return;
                }

                UpdateWorkoutTimes();

                if (_isNewWorkout)
                {
                    _context.Workouts.Add(_workout);
                }

                _context.SaveChanges();
                DialogResult = true;
                Close();
                
                Debug.WriteLine($"Workout saved successfully: {_workout.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving workout: {ex}");
                MessageBox.Show($"Ошибка при сохранении тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateWorkout()
        {
            if (string.IsNullOrWhiteSpace(_workout.Name))
            {
                MessageBox.Show("Пожалуйста, введите название тренировки",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_workout.MuscleGroup))
            {
                MessageBox.Show("Пожалуйста, укажите группу мышц",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void UpdateWorkoutTimes()
        {
            if (StartTimeComboBox.SelectedItem is string startTimeStr)
            {
                var hours = int.Parse(startTimeStr.Split(':')[0]);
                _workout.StartTimeMinutes = hours * 60;
                Debug.WriteLine($"Setting start time minutes to {_workout.StartTimeMinutes}");
            }
            else
            {
                _workout.StartTimeMinutes = null;
            }

            if (EndTimeComboBox.SelectedItem is string endTimeStr)
            {
                var hours = int.Parse(endTimeStr.Split(':')[0]);
                _workout.EndTimeMinutes = hours * 60;
                Debug.WriteLine($"Setting end time minutes to {_workout.EndTimeMinutes}");
            }
            else
            {
                _workout.EndTimeMinutes = null;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
