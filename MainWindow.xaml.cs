using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using WorkoutApp.Pages;
using WorkoutApp.Data;

namespace WorkoutApp
{
    public partial class MainWindow : Window
    {
        private bool isDarkTheme = false;
        public WorkoutDbContext Context { get; private set; }

        public MainWindow()
        {
            try
            {
                Debug.WriteLine("Initializing MainWindow");
                InitializeComponent();
                Debug.WriteLine("InitializeComponent completed");
                
                // Инициализируем базу данных
                Context = new WorkoutDbContext();
                Context.EnsureDatabaseCreated();
                Debug.WriteLine("Database initialization completed");

                Debug.WriteLine("Navigating to HomePage...");
                NavigateToHome();
                Debug.WriteLine("Navigation to HomePage completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in MainWindow constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToHome()
        {
            try
            {
                Debug.WriteLine("Navigating to HomePage...");
                EnsureDatabaseContext();
                MainFrame.Navigate(new HomePage(Context));
                Debug.WriteLine("Navigated to HomePage successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to HomePage: {ex}");
                MessageBox.Show($"Ошибка при переходе на главную страницу: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToHome(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void NavigateToCalendar(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Navigating to CalendarPage...");
                
                // Проверяем, что контекст базы данных существует
                if (Context == null)
                {
                    Debug.WriteLine("Database context is null, creating new one");
                    Context = new WorkoutDbContext();
                    Context.EnsureDatabaseCreated();
                }
                if (Context == null)
                {
                    Debug.WriteLine("Database context is null, creating new one before navigating to CalendarPage");
                    Context = new WorkoutDbContext();
                    Context.EnsureDatabaseCreated();
                }

                var calendarPage = new CalendarPage(Context);
                MainFrame.Navigate(calendarPage);
                Debug.WriteLine("Navigation to CalendarPage completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in NavigateToCalendar: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // MessageBox.Show($"Ошибка при переходе на страницу календаря: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToWorkouts(object sender, RoutedEventArgs e)
        {
            NavigateToWorkouts();
        }

        private void NavigateToWorkouts()
        {
            try
            {
                Debug.WriteLine("Navigating to WorkoutsPage...");
                EnsureDatabaseContext();
                MainFrame.Navigate(new WorkoutsPage(Context));
                Debug.WriteLine("Navigated to WorkoutsPage successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error navigating to WorkoutsPage: {ex}");
                MessageBox.Show($"Ошибка при переходе к списку тренировок: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnsureDatabaseContext()
        {
            if (Context == null)
            {
                Context = new WorkoutDbContext();
                Context.EnsureDatabaseCreated();
            }
        }
    }
}
