using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a user of the journal application.
    /// Relationships:
    /// - One-to-Many with JournalEntry: A user can have multiple entries.
    /// - One-to-Many with Tag: A user can create custom tags.
    /// - One-to-Many with Category: A user can define custom categories.
    /// </summary>
    public class User
    {
        [PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Username { get; set; } = "Default User";

        public string? Email { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
