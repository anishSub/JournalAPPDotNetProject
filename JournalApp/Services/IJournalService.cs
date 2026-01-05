using JournalApp.Models;

namespace JournalApp.Services
{
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
    }
}
