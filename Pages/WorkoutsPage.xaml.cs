using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using WorkoutApp.Data;
using WorkoutApp.Models;
using WorkoutApp.Windows;
using System.Diagnostics;

namespace WorkoutApp.Pages
{
    public partial class WorkoutsPage : Page
    {
        private readonly WorkoutDbContext _context;

        public WorkoutsPage(WorkoutDbContext context)
        {
            try
            {
                Debug.WriteLine("Initializing WorkoutsPage...");
                
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context), "Database context cannot be null");
                }

                InitializeComponent();
                _context = context;
                LoadWorkouts();
                
                Debug.WriteLine("WorkoutsPage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing WorkoutsPage: {ex}");
                MessageBox.Show($"Ошибка при инициализации страницы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void LoadWorkouts()
        {
            try
            {
                var workouts = _context.Workouts
                    .OrderBy(w => w.Name)
                    .ToList();
                WorkoutsList.ItemsSource = workouts;
                Debug.WriteLine($"Loaded {workouts.Count} workouts");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading workouts: {ex}");
                MessageBox.Show($"Ошибка при загрузке тренировок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddWorkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new EditWorkoutWindow(_context);
                if (dialog.ShowDialog() == true)
                {
                    Debug.WriteLine("New workout created successfully");
                    LoadWorkouts();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating workout: {ex}");
                MessageBox.Show($"Ошибка при создании тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditWorkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is Workout workout)
                {
                    Debug.WriteLine($"Editing workout: {workout.Name}");
                    
                    var dialog = new EditWorkoutWindow(_context, workout);
                    if (dialog.ShowDialog() == true)
                    {
                        Debug.WriteLine($"Workout edited successfully: {workout.Name}");
                        LoadWorkouts();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error editing workout: {ex}");
                MessageBox.Show($"Ошибка при редактировании тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteWorkout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is Workout workout)
                {
                    var result = MessageBox.Show(
                        $"Вы уверены, что хотите удалить тренировку '{workout.Name}'?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        _context.Workouts.Remove(workout);
                        _context.SaveChanges();
                        Debug.WriteLine($"Workout deleted: {workout.Name}");
                        LoadWorkouts();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting workout: {ex}");
                MessageBox.Show($"Ошибка при удалении тренировки: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
