using System;

namespace WorkoutApp.Models
{
    public class Workout
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MuscleGroup { get; set; } = string.Empty;
        public int Sets { get; set; }
        public int Repetitions { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCustom { get; set; }
        public DateTime? ScheduledDate { get; set; }
        
        // Храним время в минутах от начала дня (0-1439)
        public int? StartTimeMinutes { get; set; }
        public int? EndTimeMinutes { get; set; }

        // Вспомогательные свойства для работы с TimeSpan
        public TimeSpan? StartTime
        {
            get => StartTimeMinutes.HasValue ? TimeSpan.FromMinutes(StartTimeMinutes.Value) : null;
            set => StartTimeMinutes = value?.TotalMinutes is double minutes ? (int)minutes : null;
        }

        public TimeSpan? EndTime
        {
            get => EndTimeMinutes.HasValue ? TimeSpan.FromMinutes(EndTimeMinutes.Value) : null;
            set => EndTimeMinutes = value?.TotalMinutes is double minutes ? (int)minutes : null;
        }

        public Workout()
        {
            Name = string.Empty;
            MuscleGroup = string.Empty;
            Description = string.Empty;
            Sets = 0;
            Repetitions = 0;
            Duration = 0;
            IsCustom = false;
        }

        public Workout(string name, string muscleGroup, string description, int sets, int repetitions, int duration, bool isCustom = false)
        {
            Name = name;
            MuscleGroup = muscleGroup;
            Description = description;
            Sets = sets;
            Repetitions = repetitions;
            Duration = duration;
            IsCustom = isCustom;
        }
    }
}
