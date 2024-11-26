using System;
using System.Windows;
using System.Windows.Controls;
using WorkoutApp.Data;
using System.Diagnostics;

namespace WorkoutApp.Pages
{
    public partial class HomePage : Page
    {
        private readonly WorkoutDbContext _context;

        public HomePage(WorkoutDbContext context)
        {
            try
            {
                Debug.WriteLine("Initializing HomePage...");
                
                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context), "Database context cannot be null");
                }

                InitializeComponent();
                _context = context;
                
                Debug.WriteLine("HomePage initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing HomePage: {ex}");
                MessageBox.Show($"Ошибка при инициализации страницы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        private void NavigateToWorkouts(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Navigating to WorkoutsPage...");
                NavigationService?.Navigate(new WorkoutsPage(_context));
                Debug.WriteLine("Navigated to WorkoutsPage successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to WorkoutsPage: {ex}");
                MessageBox.Show($"Ошибка при переходе к списку тренировок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToCalendar(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Navigating to CalendarPage...");
                NavigationService?.Navigate(new CalendarPage(_context));
                Debug.WriteLine("Navigated to CalendarPage successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to CalendarPage: {ex}");
                MessageBox.Show($"Ошибка при переходе к календарю: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
