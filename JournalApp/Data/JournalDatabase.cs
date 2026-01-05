using SQLite;
using JournalApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JournalApp.Data
{
    public class JournalDatabase
    {
        private SQLiteAsyncConnection _database;

        async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await _database.CreateTableAsync<JournalEntry>();
        }

        public async Task<List<JournalEntry>> GetEntriesAsync()
        {
            await Init();
            return await _database.Table<JournalEntry>().ToListAsync();
        }
        
        // Month is 1-12, Year is YYYY
        public async Task<List<JournalEntry>> GetEntriesForMonthAsync(int month, int year)
        {
            await Init();
            // Since Date is stored as string (e.g. "Fri, May 12, 2025"), we might need LINQ filter on client side
            // or better, parse it. For simplicity/robustness with current string format, we'll fetch all and filter in memory
            // optimizing this would require changing Date to DateTime in Model, but we want to minimize UI breakage right now.
            
            var all = await _database.Table<JournalEntry>().ToListAsync();
            return all.Where(e => {
                if(DateTime.TryParse(e.Date, out var dt))
                {
                    return dt.Month == month && dt.Year == year;
                }
                return false;
            }).ToList();
        }

        public async Task<JournalEntry> GetEntryAsync(string id)
        {
            await Init();
            return await _database.Table<JournalEntry>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveEntryAsync(JournalEntry item)
        {
            await Init();
            // Determine if update or insert based on if existing ID is found
            var existing = await GetEntryAsync(item.Id);
            if (existing != null)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteEntryAsync(JournalEntry item)
        {
            await Init();
            return await _database.DeleteAsync(item);
        }

        public async Task<List<JournalEntry>> SearchEntriesAsync(string query, string mood, string tag)
        {
             await Init();
             // Client-side filtering for simplicity with the complex string/list matching
             // SQLite's LIKE is limited for the TagsString csv
             
             var all = await _database.Table<JournalEntry>().ToListAsync();
             var filtered = all.AsEnumerable();

             if (!string.IsNullOrWhiteSpace(query))
             {
                 filtered = filtered.Where(e => 
                    (e.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) || 
                    (e.Content?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
             }

             if (!string.IsNullOrWhiteSpace(mood) && mood != "All")
             {
                 // Mood comparison - checking Label or Emoji if needed, assuming Label here based on usage
                 filtered = filtered.Where(e => e.MoodLabel == mood || e.Mood == mood); 
             }

             if (!string.IsNullOrWhiteSpace(tag) && tag != "All")
             {
                 filtered = filtered.Where(e => e.Tags.Contains(tag));
             }

             return filtered.ToList();
        }
    }
}
