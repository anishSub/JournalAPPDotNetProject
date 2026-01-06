using JournalApp.Models;
using JournalApp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace JournalApp.Services
{
    /// <summary>
    /// Acts as the bridge (Business Logic Layer) between the UI and the Database.
    /// Updated to work with relational database schema.
    /// </summary>
    public class JournalService : IJournalService
    {
        private readonly JournalDatabase _database;

        public JournalService(JournalDatabase database)
        {
            _database = database;
        }

        public async Task<bool> EntryExists(DateTime date)
        {
            var entry = await _database.GetEntryByDateAsync(date).ConfigureAwait(false);
            return entry != null;
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            return await _database.GetEntryByDateAsync(date).ConfigureAwait(false);
        }

        public async Task SaveEntryAsync(JournalEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Id)) 
                entry.Id = Guid.NewGuid().ToString();
            
            if (string.IsNullOrEmpty(entry.UserId))
                entry.UserId = "default-user";

            await _database.SaveEntryAsync(entry).ConfigureAwait(false);
        }

        public async Task UpdateEntryAsync(JournalEntry entry)
        {
            await _database.SaveEntryAsync(entry).ConfigureAwait(false);
        }

        public async Task DeleteEntryAsync(string id)
        {
            var entry = await GetEntryByIdAsync(id).ConfigureAwait(false);
            if (entry != null)
                await _database.DeleteEntryAsync(entry).ConfigureAwait(false);
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return await _database.GetEntriesAsync().ConfigureAwait(false);
        }

        public async Task<List<JournalEntry>> GetEntriesForMonthAsync(int month, int year)
        {
            return await _database.GetEntriesForMonthAsync(month, year).ConfigureAwait(false);
        }

        public async Task<List<JournalEntry>> SearchEntriesAsync(string query, string mood, string tag)
        {
            return await _database.SearchEntriesAsync(query, mood, tag).ConfigureAwait(false);
        }

        public async Task<JournalEntry?> GetEntryByIdAsync(string id)
        {
            return await _database.GetEntryAsync(id).ConfigureAwait(false);
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            var allEntries = await _database.GetEntriesAsync().ConfigureAwait(false);
            var dates = allEntries
                .Select(e => e.Date.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (!dates.Any()) return 0;

            var today = DateTime.Today;
            var currentCheck = today;

            // Determine start point
            if (dates.First() == today)
            {
                currentCheck = today;
            }
            else if (dates.First() == today.AddDays(-1))
            {
                currentCheck = today.AddDays(-1);
            }
            else
            {
                return 0; // Streak broken
            }

            var streak = 0;
            foreach (var date in dates)
            {
                if (date == currentCheck)
                {
                    streak++;
                    currentCheck = currentCheck.AddDays(-1);
                }
                else if (date < currentCheck)
                {
                    break;
                }
            }
            
            return streak;
        }

        public async Task<int> GetLongestStreakAsync()
        {
            var allEntries = await _database.GetEntriesAsync().ConfigureAwait(false);
            var dates = allEntries
                .Select(e => e.Date.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            if (!dates.Any()) return 0;

            int maxStreak = 0;
            int currentStreak = 0;
            DateTime? lastDate = null;

            foreach (var date in dates)
            {
                if (lastDate == null || date == lastDate.Value.AddDays(1))
                {
                    currentStreak++;
                }
                else
                {
                    currentStreak = 1;
                }
                
                if (currentStreak > maxStreak)
                    maxStreak = currentStreak;
                    
                lastDate = date;
            }

            return maxStreak;
        }

        public async Task<int> GetTotalEntriesAsync()
        {
            var entries = await _database.GetEntriesAsync().ConfigureAwait(false);
            return entries.Count;
        }

        public async Task<string> GetMostFrequentMoodAsync()
        {
            var entries = await _database.GetEntriesAsync().ConfigureAwait(false);
            if (!entries.Any()) return "Neutral";

            var mostFreq = entries
                .Where(e => !string.IsNullOrEmpty(e.MoodLabel))
                .GroupBy(e => e.MoodLabel)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            return mostFreq?.Key ?? "Neutral";
        }

        // New methods for relational database
        public async Task<List<Mood>> GetMoodsAsync()
        {
            return await _database.GetMoodsAsync().ConfigureAwait(false);
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _database.GetTagsAsync("default-user").ConfigureAwait(false);
        }

        public async Task<Tag?> GetOrCreateTagAsync(string tagName)
        {
            return await _database.GetOrCreateTagAsync(tagName, "default-user").ConfigureAwait(false);
        }
    }
}
