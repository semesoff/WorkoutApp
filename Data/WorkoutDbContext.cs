using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WorkoutApp.Models;
using System.Diagnostics;

namespace WorkoutApp.Data
{
    public class WorkoutDbContext : DbContext
    {
        private static string DbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WorkoutApp",
            "workouts.db");

        public DbSet<Workout> Workouts { get; set; }

        public WorkoutDbContext()
        {
            try
            {
                var folder = Path.GetDirectoryName(DbPath);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in WorkoutDbContext constructor: {ex}");
                throw;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");
                Debug.WriteLine($"Using database at: {DbPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnConfiguring: {ex}");
                throw;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            try
            {
                base.OnModelCreating(modelBuilder);

                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

                modelBuilder.Entity<Workout>().HasData(
                    new Workout
                    {
                        Id = 1,
                        Name = "Отжимания",
                        MuscleGroup = "Грудь, Трицепс",
                        Sets = 3,
                        Repetitions = 15,
                        Duration = 60,
                        Description = "Классические отжимания от пола",
                        IsCustom = false,
                        ScheduledDate = startOfWeek,
                        StartTimeMinutes = 10 * 60,
                        EndTimeMinutes = 11 * 60
                    },
                    new Workout
                    {
                        Id = 2,
                        Name = "Приседания",
                        MuscleGroup = "Ноги",
                        Sets = 3,
                        Repetitions = 20,
                        Duration = 60,
                        Description = "Классические приседания",
                        IsCustom = false,
                        ScheduledDate = startOfWeek.AddDays(2),
                        StartTimeMinutes = 15 * 60,
                        EndTimeMinutes = 16 * 60
                    },
                    new Workout
                    {
                        Id = 3,
                        Name = "Планка",
                        MuscleGroup = "Пресс, Спина",
                        Sets = 3,
                        Repetitions = 1,
                        Duration = 60,
                        Description = "Планка",
                        IsCustom = false,
                        ScheduledDate = startOfWeek.AddDays(4),
                        StartTimeMinutes = 18 * 60,
                        EndTimeMinutes = 19 * 60
                    }
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in OnModelCreating: {ex}");
                throw;
            }
        }

        public void EnsureDatabaseCreated()
        {
            try
            {
                if (!File.Exists(DbPath))
                {
                    Debug.WriteLine("Database file does not exist, creating new database...");
                    Database.EnsureCreated();
                    Debug.WriteLine("Database created successfully");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ensuring database is created: {ex}");
                throw;
            }
        }
    }
}
