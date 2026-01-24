using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a granular specific mood derived from a parent mood.
    /// Relationships:
    /// - Many-to-One with Mood: Belongs to a parent mood (e.g. "Excited" belongs to "Positive").
    /// - Many-to-Many with JournalEntry (via EntrySecondaryMood): Used in multiple entries.
    /// </summary>
    public class SecondaryMood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Emoji { get; set; } = string.Empty;

        // Foreign key to the parent Mood (Positive, Neutral, Negative)
        [Indexed]
        public int ParentMoodId { get; set; }
    }
}
