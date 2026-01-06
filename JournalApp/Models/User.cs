using SQLite;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a user of the journal application.
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
