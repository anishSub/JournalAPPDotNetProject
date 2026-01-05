using JournalApp.Models;
using JournalApp.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace JournalApp.Services
{
    public class JournalService : IJournalService
    {
        private readonly JournalDatabase _database;

        public JournalService(JournalDatabase database)
        {
            _database = database;
        }

        public async Task<bool> EntryExists(DateTime date)
        {
             var all = await _database.GetEntriesAsync();
             return all.Any(e => {
                 if(DateTime.TryParse(e.Date, out var dt))
                 {
                     return dt.Date == date.Date;
                 }
                 return false;
             });
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
             var all = await _database.GetEntriesAsync();
             return all.FirstOrDefault(e => {
                 if(DateTime.TryParse(e.Date, out var dt))
                 {
                     return dt.Date == date.Date;
                 }
                 return false;
             });
        }

        public async Task SaveEntryAsync(JournalEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Id)) entry.Id = Guid.NewGuid().ToString();
            await _database.SaveEntryAsync(entry);
        }

        public async Task UpdateEntryAsync(JournalEntry entry)
        {
            await _database.SaveEntryAsync(entry);
        }

        public async Task DeleteEntryAsync(string id)
        {
            var entry = await GetEntryByIdAsync(id);
            if (entry != null)
                await _database.DeleteEntryAsync(entry);
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            return await _database.GetEntriesAsync();
        }

        public async Task<List<JournalEntry>> GetEntriesForMonthAsync(int month, int year)
        {
            return await _database.GetEntriesForMonthAsync(month, year);
        }

        public async Task<List<JournalEntry>> SearchEntriesAsync(string query, string mood, string tag)
        {
            return await _database.SearchEntriesAsync(query, mood, tag);
        }

        public async Task<JournalEntry?> GetEntryByIdAsync(string id)
        {
            return await _database.GetEntryAsync(id);
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            var allEntries = await _database.GetEntriesAsync();
            var dates = allEntries
                .Select(e => DateTime.TryParse(e.Date, out var dt) ? dt.Date : (DateTime?)null)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
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
                    // Found a date older than expected, meaning a gap existed
                    break; 
                }
                // If date > currentCheck, that shouldn't happen due to logic/sort, but loop continues
            }
            
            return streak;
        }

        public async Task<int> GetLongestStreakAsync()
        {
            var allEntries = await _database.GetEntriesAsync();
            var dates = allEntries
                .Select(e => DateTime.TryParse(e.Date, out var dt) ? dt.Date : (DateTime?)null)
                .Where(d => d.HasValue)
                .Select(d => d.Value)
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
            var entries = await _database.GetEntriesAsync();
            return entries.Count;
        }

        public async Task<string> GetMostFrequentMoodAsync()
        {
             var entries = await _database.GetEntriesAsync();
             if (!entries.Any()) return "Neutral"; // Default

             var mostFreq = entries
                .Where(e => !string.IsNullOrEmpty(e.MoodLabel))
                .GroupBy(e => e.MoodLabel)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

             return mostFreq?.Key ?? "Neutral";
        }
    }
}
