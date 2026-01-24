using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a mood option that can be associated with journal entries.
    /// Relationships:
    /// - One-to-Many with JournalEntry: A mood can be used by multiple entries.
    /// - One-to-Many with SecondaryMood: A primary mood is a parent to multiple secondary moods.
    /// </summary>
    public class Mood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // e.g., "Happy", "Sad"

        public string Emoji { get; set; } = string.Empty; // e.g., "😊", "😔"

        public string? Color { get; set; } // Optional color for UI visualization
    }
}
