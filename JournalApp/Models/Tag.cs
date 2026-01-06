using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a tag that can be applied to journal entries.
    /// </summary>
    public class Tag
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // e.g., "Travel", "Family"

        [Indexed]
        public string UserId { get; set; } = string.Empty; // Foreign Key to User
    }
}
