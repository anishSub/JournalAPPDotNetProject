using SQLite;
using JournalApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace JournalApp.Data
{
    /// <summary>
    /// Manages SQLite database interactions for the relational schema.
    /// Handles CRUD operations for all entities: User, Mood, Category, Tag, JournalEntry, EntryTag.
    /// </summary>
    public class JournalDatabase
    {
        private SQLiteAsyncConnection _database;
        private static bool _initialized = false;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        async Task Init()
        {
            if (_initialized)
                return;

            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_initialized)
                    return;

                _database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
                
                // Create all tables
                await _database.CreateTableAsync<User>().ConfigureAwait(false);
                await _database.CreateTableAsync<Mood>().ConfigureAwait(false);
                await _database.CreateTableAsync<Category>().ConfigureAwait(false);
                await _database.CreateTableAsync<Tag>().ConfigureAwait(false);
                await _database.CreateTableAsync<JournalEntry>().ConfigureAwait(false);
                await _database.CreateTableAsync<JournalEntry>().ConfigureAwait(false);
                await _database.CreateTableAsync<EntryTag>().ConfigureAwait(false);
                await _database.CreateTableAsync<SecondaryMood>().ConfigureAwait(false);
                await _database.CreateTableAsync<EntrySecondaryMood>().ConfigureAwait(false);

                // Seed default data
                await SeedDefaultData().ConfigureAwait(false);
                
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        private async Task SeedDefaultData()
        {
            // Create default user if none exists
            var users = await _database.Table<User>().ToListAsync().ConfigureAwait(false);
            if (!users.Any())
            {
                await _database.InsertAsync(new User
                {
                    Id = "default-user",
                    Username = "Me",
                    CreatedDate = DateTime.Now
                }).ConfigureAwait(false);
            }

            // Seed moods if none exist
            // Seed moods
            var existingMoods = await _database.Table<Mood>().ToListAsync().ConfigureAwait(false);
            
            var requiredMoods = new List<Mood>
            {
                new Mood { Name = "Positive", Emoji = "😊", Color = "#10b981" },
                new Mood { Name = "Neutral", Emoji = "😐", Color = "#94a3b8" },
                new Mood { Name = "Negative", Emoji = "😞", Color = "#ef4444" }
            };

            foreach (var required in requiredMoods)
            {
                // Check if mood exists by name
                if (!existingMoods.Any(m => m.Name == required.Name))
                {
                    await _database.InsertAsync(required).ConfigureAwait(false);
                }
            }

            // Seed Secondary Moods
            await SeedSecondaryMoods(requiredMoods).ConfigureAwait(false);
        }

        private async Task SeedSecondaryMoods(List<Mood> parentMoods)
        {
            var existing = await _database.Table<SecondaryMood>().ToListAsync().ConfigureAwait(false);
            if (existing.Any()) return;

            // We need parent IDs. Reload moods to be sure we have IDs
            var dbMoods = await _database.Table<Mood>().ToListAsync().ConfigureAwait(false);
            var positive = dbMoods.FirstOrDefault(m => m.Name == "Positive");
            var neutral = dbMoods.FirstOrDefault(m => m.Name == "Neutral");
            var negative = dbMoods.FirstOrDefault(m => m.Name == "Negative");

            var defaults = new List<SecondaryMood>();

            if (positive != null)
            {
                defaults.Add(new SecondaryMood { Name = "Happy", Emoji = "😊", ParentMoodId = positive.Id });
                defaults.Add(new SecondaryMood { Name = "Grateful", Emoji = "🙏", ParentMoodId = positive.Id });
                defaults.Add(new SecondaryMood { Name = "Excited", Emoji = "😄", ParentMoodId = positive.Id });
                defaults.Add(new SecondaryMood { Name = "Blessed", Emoji = "✨", ParentMoodId = positive.Id });
            }

            if (neutral != null)
            {
                defaults.Add(new SecondaryMood { Name = "Calm", Emoji = "😐", ParentMoodId = neutral.Id });
                defaults.Add(new SecondaryMood { Name = "Thinking", Emoji = "🤔", ParentMoodId = neutral.Id });
                defaults.Add(new SecondaryMood { Name = "Surprised", Emoji = "😮", ParentMoodId = neutral.Id });
                defaults.Add(new SecondaryMood { Name = "Tired", Emoji = "😴", ParentMoodId = neutral.Id });
            }

            if (negative != null)
            {
                defaults.Add(new SecondaryMood { Name = "Sad", Emoji = "😢", ParentMoodId = negative.Id });
                defaults.Add(new SecondaryMood { Name = "Anxious", Emoji = "😰", ParentMoodId = negative.Id });
                defaults.Add(new SecondaryMood { Name = "Crying", Emoji = "😭", ParentMoodId = negative.Id });
                defaults.Add(new SecondaryMood { Name = "Worried", Emoji = "😟", ParentMoodId = negative.Id });
            }

            foreach (var item in defaults)
            {
                await _database.InsertAsync(item).ConfigureAwait(false);
            }
        }

        #region JournalEntry CRUD

        public async Task<List<JournalEntry>> GetEntriesAsync()
        {
            await Init().ConfigureAwait(false);
            var entries = await _database.Table<JournalEntry>().ToListAsync().ConfigureAwait(false);
            
            // OPTIMIZATION: Batch load all relations at once instead of one-by-one
            await LoadBatchEntryRelations(entries).ConfigureAwait(false);
            
            return entries;
        }
        
        public async Task<List<JournalEntry>> GetEntriesForMonthAsync(int month, int year)
        {
            await Init().ConfigureAwait(false);
            
            // Calculate range to avoid using .Month/.Year in SQL expression (not supported)
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1);

            var entries = await _database.Table<JournalEntry>()
                .Where(e => e.Date >= start && e.Date < end)
                .ToListAsync().ConfigureAwait(false);
            
            await LoadBatchEntryRelations(entries).ConfigureAwait(false);
            
            return entries;
        }

        public async Task<JournalEntry?> GetEntryAsync(string id)
        {
            await Init().ConfigureAwait(false);
            var entry = await _database.Table<JournalEntry>().Where(i => i.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
            
            if (entry != null)
            {
                await LoadEntryRelations(entry).ConfigureAwait(false);
            }
            
            return entry;
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            await Init().ConfigureAwait(false);
            
            // Calculate range to avoid using .Date in SQL expression (not supported)
            var start = date.Date;
            var end = start.AddDays(1);

            var entry = await _database.Table<JournalEntry>()
                .Where(i => i.Date >= start && i.Date < end)
                .FirstOrDefaultAsync().ConfigureAwait(false);
            
            if (entry != null)
            {
                await LoadEntryRelations(entry).ConfigureAwait(false);
            }
            
            return entry;
        }

        public async Task<int> SaveEntryAsync(JournalEntry item)
        {
            await Init().ConfigureAwait(false);
            
            var existing = await GetEntryAsync(item.Id).ConfigureAwait(false);
            int result;
            
            if (existing != null)
            {
                item.ModifiedDate = DateTime.Now;
                result = await _database.UpdateAsync(item).ConfigureAwait(false);
            }
            else
            {
                item.CreatedDate = DateTime.Now;
                item.ModifiedDate = DateTime.Now;
                if (string.IsNullOrEmpty(item.UserId))
                    item.UserId = "default-user"; // Set default user
                result = await _database.InsertAsync(item).ConfigureAwait(false);
            }

            // Save tags (many-to-many relationship)
            await SaveEntryTags(item).ConfigureAwait(false);
            
            // Save secondary moods (many-to-many relationship)
            await SaveEntrySecondaryMoods(item).ConfigureAwait(false);

            return result;
        }

        public async Task<int> DeleteEntryAsync(JournalEntry item)
        {
            await Init().ConfigureAwait(false);
            
            // Delete associated entry-tags first
            // Delete associated entry-tags first (Direct SQL because EntryTag has no PK)
            await _database.ExecuteAsync("DELETE FROM EntryTag WHERE EntryId = ?", item.Id).ConfigureAwait(false);
            await _database.ExecuteAsync("DELETE FROM EntrySecondaryMood WHERE EntryId = ?", item.Id).ConfigureAwait(false);
            
            return await _database.DeleteAsync(item).ConfigureAwait(false);
        }

        #endregion



        #region Helper Methods

        private async Task LoadEntryRelations(JournalEntry entry)
        {
            // Load Mood
            if (entry.MoodId.HasValue)
            {
                entry.Mood = await _database.Table<Mood>()
                    .Where(m => m.Id == entry.MoodId.Value)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
            }

            // Load Category
            if (entry.CategoryId.HasValue)
            {
                entry.Category = await _database.Table<Category>()
                    .Where(c => c.Id == entry.CategoryId.Value)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
            }

            // Load Tags
            var entryTags = await _database.Table<EntryTag>()
                .Where(et => et.EntryId == entry.Id)
                .ToListAsync().ConfigureAwait(false);

            entry.Tags = new List<Tag>();
            foreach (var et in entryTags)
            {
                var tag = await _database.Table<Tag>()
                    .Where(t => t.Id == et.TagId)
                    .FirstOrDefaultAsync().ConfigureAwait(false);
                
                if (tag != null)
                    entry.Tags.Add(tag);
            }

            // Load Secondary Moods
            var entryMoods = await _database.Table<EntrySecondaryMood>()
                .Where(e => e.EntryId == entry.Id)
                .ToListAsync().ConfigureAwait(false);
            
            entry.SecondaryMoodsList = new List<SecondaryMood>();
            foreach(var em in entryMoods)
            {
                var sm = await _database.Table<SecondaryMood>().Where(s => s.Id == em.SecondaryMoodId).FirstOrDefaultAsync().ConfigureAwait(false);
                if(sm != null) entry.SecondaryMoodsList.Add(sm);
            }
            // Sync legacy string for UI back-compat if needed
            entry.SecondaryMood = string.Join(", ", entry.SecondaryMoodsList.Select(s => s.Name));
        }

        // OPTIMIZED: Batch load relations for multiple entries (fixes N+1 query problem)
        private async Task LoadBatchEntryRelations(List<JournalEntry> entries)
        {
            if (!entries.Any()) return;

            // Batch load ALL moods, categories, tags at once
            var allMoods = await _database.Table<Mood>().ToListAsync().ConfigureAwait(false);
            var allCategories = await _database.Table<Category>().ToListAsync().ConfigureAwait(false);
            var allTags = await _database.Table<Tag>().ToListAsync().ConfigureAwait(false);
            var allEntryTags = await _database.Table<EntryTag>().ToListAsync().ConfigureAwait(false);
            
            // Load all Secondary Moods data
            var allSecMoods = await _database.Table<SecondaryMood>().ToListAsync().ConfigureAwait(false);
            var allEntrySecMoods = await _database.Table<EntrySecondaryMood>().ToListAsync().ConfigureAwait(false);

            // Create lookup dictionaries
            var moodDict = allMoods.ToDictionary(m => m.Id);
            var categoryDict = allCategories.ToDictionary(c => c.Id);
            var tagDict = allTags.ToDictionary(t => t.Id);
            var secMoodDict = allSecMoods.ToDictionary(s => s.Id);
            
            var entryIds = entries.Select(e => e.Id).ToList();
            var entryTagsDict = allEntryTags
                .Where(et => entryIds.Contains(et.EntryId))
                .GroupBy(et => et.EntryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var entrySecMoodsDict = allEntrySecMoods
                .Where(e => entryIds.Contains(e.EntryId))
                .GroupBy(e => e.EntryId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Assign relations to each entry using lookups (O(1) instead of O(n))
            foreach (var entry in entries)
            {
                // Assign mood
                if (entry.MoodId.HasValue && moodDict.ContainsKey(entry.MoodId.Value))
                {
                    entry.Mood = moodDict[entry.MoodId.Value];
                }

                // Assign category
                if (entry.CategoryId.HasValue && categoryDict.ContainsKey(entry.CategoryId.Value))
                {
                    entry.Category = categoryDict[entry.CategoryId.Value];
                }

                // Assign tags
                entry.Tags = new List<Tag>();
                if (entryTagsDict.ContainsKey(entry.Id))
                {
                    foreach (var et in entryTagsDict[entry.Id])
                    {
                        if (tagDict.ContainsKey(et.TagId))
                        {
                            entry.Tags.Add(tagDict[et.TagId]);
                        }
                    }
                }

                // Assign secondary moods
                entry.SecondaryMoodsList = new List<SecondaryMood>();
                if (entrySecMoodsDict.ContainsKey(entry.Id))
                {
                    foreach (var em in entrySecMoodsDict[entry.Id])
                    {
                        if (secMoodDict.ContainsKey(em.SecondaryMoodId))
                        {
                            entry.SecondaryMoodsList.Add(secMoodDict[em.SecondaryMoodId]);
                        }
                    }
                }
                // Sync legacy string
                entry.SecondaryMood = string.Join(", ", entry.SecondaryMoodsList.Select(s => s.Name));
            }
        }

        private async Task SaveEntryTags(JournalEntry entry)
        {
            // Remove existing entry-tag associations using direct SQL (safer for tables without PK)
            await _database.ExecuteAsync("DELETE FROM EntryTag WHERE EntryId = ?", entry.Id).ConfigureAwait(false);

            // Add new associations
            foreach (var tag in entry.Tags)
            {
                await _database.InsertAsync(new EntryTag
                {
                    EntryId = entry.Id,
                    TagId = tag.Id
                }).ConfigureAwait(false);
            }
        }

        private async Task SaveEntrySecondaryMoods(JournalEntry entry)
        {
             // Remove existing
            await _database.ExecuteAsync("DELETE FROM EntrySecondaryMood WHERE EntryId = ?", entry.Id).ConfigureAwait(false);

            // Add new
            if (entry.SecondaryMoodsList != null)
            {
                foreach (var sm in entry.SecondaryMoodsList)
                {
                    await _database.InsertAsync(new EntrySecondaryMood
                    {
                        EntryId = entry.Id,
                        SecondaryMoodId = sm.Id
                    }).ConfigureAwait(false);
                }
            }
        }

        #endregion






        #region Mood CRUD

        public async Task<List<Mood>> GetMoodsAsync()
        {
            await Init().ConfigureAwait(false);
            return await _database.Table<Mood>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<Mood?> GetMoodByIdAsync(int id)
        {
            await Init().ConfigureAwait(false);
            return await _database.Table<Mood>().Where(m => m.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<List<SecondaryMood>> GetSecondaryMoodsAsync()
        {
            await Init().ConfigureAwait(false);
            return await _database.Table<SecondaryMood>().ToListAsync().ConfigureAwait(false);
        }

        public async Task<int> SaveSecondaryMoodAsync(SecondaryMood item)
        {
             await Init().ConfigureAwait(false);
             if (item.Id != 0)
                 return await _database.UpdateAsync(item).ConfigureAwait(false);
             else
                 return await _database.InsertAsync(item).ConfigureAwait(false);
        }

        public async Task<int> DeleteSecondaryMoodAsync(int id)
        {
            await Init().ConfigureAwait(false);
            // Also need to cleanup EntrySecondaryMoods? Or let them point to nothing? Better to cleanup.
            await _database.ExecuteAsync("DELETE FROM EntrySecondaryMood WHERE SecondaryMoodId = ?", id).ConfigureAwait(false);
            return await _database.DeleteAsync<SecondaryMood>(id).ConfigureAwait(false);
        }

        #endregion

        #region User CRUD
        
        public async Task<User?> GetUserAsync(string id)
        {
            await Init().ConfigureAwait(false);
            return await _database.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        #endregion





        #region Tag CRUD

        public async Task<List<Tag>> GetTagsAsync(string userId)
        {
            await Init().ConfigureAwait(false);
            return await _database.Table<Tag>().Where(t => t.UserId == userId).ToListAsync().ConfigureAwait(false);
        }

        public async Task<Tag?> GetOrCreateTagAsync(string tagName, string userId)
        {
            await Init().ConfigureAwait(false);
            
            var existing = await _database.Table<Tag>()
                .Where(t => t.Name == tagName && t.UserId == userId)
                .FirstOrDefaultAsync().ConfigureAwait(false);

            if (existing != null)
                return existing;

            var newTag = new Tag { Name = tagName, UserId = userId };
            await _database.InsertAsync(newTag).ConfigureAwait(false);
            
            // Fetch with generated Id
            return await _database.Table<Tag>()
                .Where(t => t.Name == tagName && t.UserId == userId)
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public async Task<int> DeleteTagAsync(int id)
        {
            await Init().ConfigureAwait(false);
            await _database.ExecuteAsync("DELETE FROM EntryTag WHERE TagId = ?", id).ConfigureAwait(false);
            return await _database.DeleteAsync<Tag>(id).ConfigureAwait(false);
        }

        #endregion





        #region Category CRUD

        public async Task<List<Category>> GetCategoriesAsync(string userId)
        {
            await Init().ConfigureAwait(false);
            return await _database.Table<Category>().Where(c => c.UserId == userId).ToListAsync().ConfigureAwait(false);
        }

        public async Task<int> SaveCategoryAsync(Category category)
        {
            await Init().ConfigureAwait(false);
            if (category.Id != 0)
            {
                return await _database.UpdateAsync(category).ConfigureAwait(false);
            }
            else
            {
                return await _database.InsertAsync(category).ConfigureAwait(false);
            }
        }

        #endregion





        #region Search

        public async Task<List<JournalEntry>> SearchEntriesAsync(string query, string mood, string tag)
        {
            await Init().ConfigureAwait(false);
            var allEntries = await GetEntriesAsync().ConfigureAwait(false); // This now uses batch loading!
            var filtered = allEntries.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                filtered = filtered.Where(e => 
                    (e.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) || 
                    (e.Content?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(mood) && mood != "All")
            {
                if (mood == "Positive")
                    filtered = filtered.Where(e => e.MoodLabel == "Positive" || e.MoodLabel == "Happy" || e.MoodLabel == "Excited" || e.MoodLabel == "Peaceful");
                else if (mood == "Neutral")
                     filtered = filtered.Where(e => e.MoodLabel == "Neutral" || e.MoodLabel == "Calm" || e.MoodLabel == "Grateful");
                else if (mood == "Negative")
                     filtered = filtered.Where(e => e.MoodLabel == "Negative" || e.MoodLabel == "Sad" || e.MoodLabel == "Angry");
                else
                    filtered = filtered.Where(e => e.MoodLabel == mood);
            }

            if (!string.IsNullOrWhiteSpace(tag) && tag != "All")
            {
                filtered = filtered.Where(e => e.Tags.Any(t => t.Name == tag));
            }

            return filtered.ToList();
        }

        #endregion
    }
}
