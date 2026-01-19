using SQLite;

namespace JournalApp.Models
{
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
