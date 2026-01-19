using SQLite;

namespace JournalApp.Models
{
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
