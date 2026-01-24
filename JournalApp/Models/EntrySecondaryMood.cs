using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Junction table for the many-to-many relationship between JournalEntry and SecondaryMood.
    /// Relationships:
    /// - Foreign Key to JournalEntry
    /// - Foreign Key to SecondaryMood
    /// </summary>
    public class EntrySecondaryMood
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public string EntryId { get; set; } = string.Empty;

        [Indexed]
        public int SecondaryMoodId { get; set; }
    }
}
