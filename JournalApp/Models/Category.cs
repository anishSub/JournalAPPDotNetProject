using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a category for organizing journal entries.
    /// </summary>
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty; // e.g., "Personal", "Work"

        public string? Color { get; set; } // Optional color for UI

        [Indexed]
        public string UserId { get; set; } = string.Empty; // Foreign Key to User
    }
}
