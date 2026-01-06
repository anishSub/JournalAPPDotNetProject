using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a mood option that can be associated with journal entries.
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
