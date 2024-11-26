using System;
using System.Windows;
using System.Diagnostics;
using WorkoutApp.Data;

namespace WorkoutApp
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                Debug.WriteLine("Application starting...");
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Debug.WriteLine("Exception handlers registered");
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
    }
}
