using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media;
using WorkoutApp.Data;

namespace WorkoutApp
{
    public partial class App : Application
    {
        private static bool isDarkTheme = false;

        public App()
        {
            try
            {
                InitializeComponent();
                Debug.WriteLine("Application starting...");
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Debug.WriteLine("Exception handlers registered");
                ApplyTheme(); // Apply default theme
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in App constructor: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Критическая ошибка при запуске приложения: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Unhandled exception in UI thread: {e.Exception.Message}");
            Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
            MessageBox.Show($"Необработанная ошибка: {e.Exception.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Debug.WriteLine($"Unhandled exception in AppDomain: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Критическая ошибка: {ex.Message}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                Debug.WriteLine("OnStartup called");
                base.OnStartup(e);

                using (var dbContext = new WorkoutDbContext())
                {
                    dbContext.EnsureDatabaseCreated();
                }
                Debug.WriteLine("Base OnStartup completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnStartup: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        public static void ToggleTheme()
        {
            try
            {
                isDarkTheme = !isDarkTheme;
                ApplyTheme();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling theme: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ApplyTheme()
        {
            try
            {
                var app = Current;
                var themeDictionary = isDarkTheme ? "DarkTheme" : "LightTheme";
                var theme = app.Resources[themeDictionary] as ResourceDictionary;

                if (theme != null)
                {
                    foreach (var key in theme.Keys)
                    {
                        if (app.Resources.Contains(key))
                        {
                            app.Resources[key] = theme[key];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying theme: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
