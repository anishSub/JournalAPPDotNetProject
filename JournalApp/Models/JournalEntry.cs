using System;

namespace JournalApp.Models
{
    /// <summary>
    /// Represents a single journal entry made by the user.
    /// Updated to use proper relational foreign keys.
    /// Relationships:
    /// - Many-to-One with User: Belongs to a specific user.
    /// - Many-to-One with Mood: Associated with one primary mood.
    /// - Many-to-One with Category: Can be assigned to one category.
    /// - Many-to-Many with Tag (via EntryTag): Can have multiple tags.
    /// - Many-to-Many with SecondaryMood (via EntrySecondaryMood): Can have multiple secondary moods.
    /// </summary>
    public class JournalEntry
    {
        [SQLite.PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // The main title of the journal entry.
        public string Title { get; set; } = string.Empty;

        // A short preview of the content for display in lists.
        public string Preview { get; set; } = string.Empty;

        // The date of the entry (changed to DateTime for proper querying)
        public DateTime Date { get; set; } = DateTime.Today;

        public string SecondaryMood { get; set; } = string.Empty; // e.g. "Anxious", "Tired", "Blessed"

        // Foreign Keys
        [SQLite.Indexed]
        public string UserId { get; set; } = string.Empty; // Foreign Key to User

        [SQLite.Indexed]
        public int? MoodId { get; set; } // Foreign Key to Mood (nullable)

        [SQLite.Indexed]
        public int? CategoryId { get; set; } // Foreign Key to Category (nullable)

        // The full content/body of the journal entry (can include Markdown).
        public string Content { get; set; } = string.Empty;

        // Timestamps
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties (NOT stored in database, used for joining)
        [SQLite.Ignore]
        public Mood? Mood { get; set; }

        [SQLite.Ignore]
        public Category? Category { get; set; }

        [SQLite.Ignore]
        public List<Tag> Tags { get; set; } = new();

        [SQLite.Ignore]
        public List<SecondaryMood> SecondaryMoodsList { get; set; } = new();

        // BACKWARD COMPATIBILITY: Keep these for existing UI code
        // These will be populated from the Mood object
        [SQLite.Ignore]
        public string MoodLabel => Mood?.Name ?? string.Empty;

        [SQLite.Ignore]
        public string MoodEmoji 
        {
            get
            {
                var name = Mood?.Name ?? string.Empty;
                return name switch
                {
                    "Positive" or "Happy" or "Excited" or "Peaceful" => "🙂",
                    "Neutral" or "Calm" or "Grateful" => "😐",
                    "Negative" or "Sad" or "Angry" => "☹️",
                    _ => Mood?.Emoji ?? "🫥"
                };
            }
        }
    }
}
