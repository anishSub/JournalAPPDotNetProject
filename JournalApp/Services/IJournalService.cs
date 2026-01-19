using JournalApp.Models;

namespace JournalApp.Services
{
    /// <summary>
    /// Defines the contract for journal data operations.
    /// Decouples the UI from specific data implementations for easier testing and DI.
    /// </summary>
    public interface IJournalService
    {
        Task<bool> EntryExists(DateTime date);

        Task<JournalEntry?> GetEntryByDateAsync(DateTime date);

        Task SaveEntryAsync(JournalEntry entry);

        Task UpdateEntryAsync(JournalEntry entry);

        Task DeleteEntryAsync(string id);

        Task<List<JournalEntry>> GetAllEntriesAsync();

        Task<List<JournalEntry>> GetEntriesForMonthAsync(int month, int year);

        Task<List<JournalEntry>> SearchEntriesAsync(string query, string mood, string tag);

        Task<JournalEntry?> GetEntryByIdAsync(string id);

        Task<int> GetCurrentStreakAsync();

        Task<int> GetLongestStreakAsync();

        Task<int> GetTotalEntriesAsync();

        Task<string> GetMostFrequentMoodAsync();

        // New methods for relational database
        Task<List<Mood>> GetMoodsAsync();
        Task<List<Tag>> GetTagsAsync();
        Task<Tag?> GetOrCreateTagAsync(string tagName);
        Task<int> DeleteTagAsync(string tagName); 

        // Secondary Moods
        Task<List<SecondaryMood>> GetSecondaryMoodsAsync();
        Task<int> SaveSecondaryMoodAsync(SecondaryMood mood);
        Task<int> DeleteSecondaryMoodAsync(int id);
    }
}
