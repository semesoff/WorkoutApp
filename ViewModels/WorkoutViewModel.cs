using System;
using WorkoutApp.Models;

namespace WorkoutApp.ViewModels
{
    public class WorkoutViewModel
    {
        private const int PIXELS_PER_HOUR = 60;
        public Workout Workout { get; }

        public WorkoutViewModel(Workout workout)
        {
            Workout = workout ?? throw new ArgumentNullException(nameof(workout));
        }

        public string DisplayTime
        {
            get
            {
                if (!Workout.StartTimeMinutes.HasValue || !Workout.EndTimeMinutes.HasValue)
                    return string.Empty;

                var startTime = TimeSpan.FromMinutes(Workout.StartTimeMinutes.Value);
                var endTime = TimeSpan.FromMinutes(Workout.EndTimeMinutes.Value);
                return $"{startTime:hh\\:mm} - {endTime:hh\\:mm}";
            }
        }

        public string DisplayName => Workout.Name ?? string.Empty;
        public string DisplayDescription => Workout.Description ?? string.Empty;

        public double TopPosition
        {
            get
            {
                if (!Workout.StartTimeMinutes.HasValue) return 0;
                return (Workout.StartTimeMinutes.Value / 60.0) * PIXELS_PER_HOUR;
            }
        }

        public double Height
        {
            get
            {
                if (!Workout.StartTimeMinutes.HasValue || !Workout.EndTimeMinutes.HasValue)
                    return PIXELS_PER_HOUR; // Default height of 1 hour

                var duration = Workout.EndTimeMinutes.Value - Workout.StartTimeMinutes.Value;
                return Math.Max(duration / 60.0 * PIXELS_PER_HOUR, 30); // Minimum height of 30 pixels
            }
        }
    }
}
