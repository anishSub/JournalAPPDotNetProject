using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Junction table for the many-to-many relationship between JournalEntry and Tag.
    /// Relationships:
    /// - Foreign Key to JournalEntry
    /// - Foreign Key to Tag
    /// </summary>
    public class EntryTag
    {
        [Indexed]
        public string EntryId { get; set; } = string.Empty; // Foreign Key to JournalEntry

        [Indexed]
        public int TagId { get; set; } // Foreign Key to Tag
    }
}
