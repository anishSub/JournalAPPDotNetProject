using System;

namespace JournalApp.Models
{
    public class JournalEntry
    {
        [SQLite.PrimaryKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; } = string.Empty;
        public string Preview { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty; // Keeping as string for UI match, typically DateTime
        public string Mood { get; set; } = string.Empty; // Emoji
        public string MoodLabel { get; set; } = string.Empty;
        
        [SQLite.Ignore]
        public List<string> Tags { get; set; } = new();

        // Database helper for serializing Tags
        public string TagsString
        {
            get => string.Join(",", Tags);
            set => Tags = string.IsNullOrEmpty(value) ? new List<string>() : value.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public string Content { get; set; } = string.Empty;
    }
}
