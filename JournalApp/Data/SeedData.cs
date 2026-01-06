using JournalApp.Models;

namespace JournalApp.Data
{
    /// <summary>
    /// Helper class to seed sample journal entries for testing and demonstration
    /// </summary>
    public static class SeedData
    {
        public static async Task SeedJournalEntries(JournalDatabase database)
        {
            // Get default user
            const string userId = "default-user";
            
            // 1. Get/Seed Categories
            var categories = await database.GetCategoriesAsync(userId);
            if (!categories.Any())
            {
                await database.SaveCategoryAsync(new Category { Name = "Personal", Color = "#3b82f6", UserId = userId });
                await database.SaveCategoryAsync(new Category { Name = "Work", Color = "#ef4444", UserId = userId });
                await database.SaveCategoryAsync(new Category { Name = "Wellness", Color = "#10b981", UserId = userId });
                categories = await database.GetCategoriesAsync(userId);
            }
            var personalCat = categories.FirstOrDefault(c => c.Name == "Personal");
            var workCat = categories.FirstOrDefault(c => c.Name == "Work");
            var wellnessCat = categories.FirstOrDefault(c => c.Name == "Wellness");

            // 2. Get/Seed Tags
            var tags = await database.GetTagsAsync(userId);
            if (!tags.Any())
            {
                await database.GetOrCreateTagAsync("Nature", userId);
                await database.GetOrCreateTagAsync("Food", userId);
                await database.GetOrCreateTagAsync("Travel", userId);
                await database.GetOrCreateTagAsync("Family", userId);
                await database.GetOrCreateTagAsync("Mindfulness", userId);
                await database.GetOrCreateTagAsync("Career", userId);
                await database.GetOrCreateTagAsync("Hobby", userId);
                tags = await database.GetTagsAsync(userId);
            }
            
            // 3. Get Moods
            var moods = await database.GetMoodsAsync();
            var happyMood = moods.FirstOrDefault(m => m.Name == "Happy");
            var calmMood = moods.FirstOrDefault(m => m.Name == "Calm");
            var excitedMood = moods.FirstOrDefault(m => m.Name == "Excited");
            var gratefulMood = moods.FirstOrDefault(m => m.Name == "Grateful");
            
            // Sample journal entries with emojis
            var sampleEntries = new List<JournalEntry>
            {
                new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Amazing Day at the Beach 🏖️",
                    Content = "Today was incredible! The weather was perfect ☀️, and I spent the whole day at the beach. The waves were amazing 🌊, and I even saw dolphins 🐬! Feeling so grateful for days like this.",
                    Preview = "Today was incredible! The weather was perfect, and I spent the whole day at the beach...",
                    Date = DateTime.Now.AddDays(-1),
                    MoodId = happyMood?.Id,
                    SecondaryMood = "Relaxed",
                    CategoryId = personalCat?.Id,
                    CreatedDate = DateTime.Now.AddDays(-1),
                    ModifiedDate = DateTime.Now.AddDays(-1),
                    Tags = tags.Where(t => t.Name == "Nature" || t.Name == "Travel").ToList()
                },
                new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Morning Meditation 🧘‍♀️",
                    Content = "Started my day with 20 minutes of meditation. Feeling centered and peaceful 🕊️. The sunrise was beautiful this morning 🌅. Ready to tackle the day with a clear mind.",
                    Preview = "Started my day with 20 minutes of meditation. Feeling centered and peaceful...",
                    Date = DateTime.Now.AddDays(-2),
                    MoodId = calmMood?.Id,
                    SecondaryMood = "Focus",
                    CategoryId = wellnessCat?.Id,
                    CreatedDate = DateTime.Now.AddDays(-2),
                    ModifiedDate = DateTime.Now.AddDays(-2),
                    Tags = tags.Where(t => t.Name == "Mindfulness").ToList()
                },
                new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "New Job Opportunity! 🎉",
                    Content = "Got a call today about an exciting job opportunity! 📞 The interview went great and I'm so excited about the possibilities 🚀. Can't wait to see what happens next!",
                    Preview = "Got a call today about an exciting job opportunity! The interview went great...",
                    Date = DateTime.Now.AddDays(-3),
                    MoodId = excitedMood?.Id,
                    SecondaryMood = "Hopeful",
                    CategoryId = workCat?.Id,
                    CreatedDate = DateTime.Now.AddDays(-3),
                    ModifiedDate = DateTime.Now.AddDays(-3),
                    Tags = tags.Where(t => t.Name == "Career").ToList()
                },
                new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Family Dinner 👨‍👩‍👧‍👦",
                    Content = "Had dinner with the whole family tonight 🍽️. Mom made my favorite pasta 🍝! Spent hours talking and laughing. These moments are precious 💝. So grateful for family.",
                    Preview = "Had dinner with the whole family tonight. Mom made my favorite pasta!...",
                    Date = DateTime.Now.AddDays(-4),
                    MoodId = gratefulMood?.Id,
                    SecondaryMood = "Loved",
                    CategoryId = personalCat?.Id,
                    CreatedDate = DateTime.Now.AddDays(-4),
                    ModifiedDate = DateTime.Now.AddDays(-4),
                    Tags = tags.Where(t => t.Name == "Family" || t.Name == "Food").ToList()
                },
                new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Weekend Hiking Adventure ⛰️",
                    Content = "Went hiking in the mountains today 🥾. The view from the top was breathtaking! 🏔️ Saw so many beautiful wildflowers 🌸 along the trail. Nature therapy at its best 🌿.",
                    Preview = "Went hiking in the mountains today. The view from the top was breathtaking!...",
                    Date = DateTime.Now.AddDays(-5),
                    MoodId = happyMood?.Id,
                    SecondaryMood = "Energetic",
                    CategoryId = wellnessCat?.Id,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    ModifiedDate = DateTime.Now.AddDays(-5),
                    Tags = tags.Where(t => t.Name == "Nature").ToList()
                },
                new JournalEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Title = "Reading by the Fireplace 📚",
                    Content = "Cozy evening reading my new book by the fireplace 🔥. Perfect way to unwind after a busy week. The story is captivating! Already halfway through 📖. Peaceful and content.",
                    Preview = "Cozy evening reading my new book by the fireplace. Perfect way to unwind...",
                    Date = DateTime.Now.AddDays(-6),
                    MoodId = calmMood?.Id,
                    SecondaryMood = "Cozy",
                    CategoryId = personalCat?.Id,
                    CreatedDate = DateTime.Now.AddDays(-6),
                    ModifiedDate = DateTime.Now.AddDays(-6),
                    Tags = tags.Where(t => t.Name == "Hobby").ToList()
                }
            };
            
            // Save all entries
            foreach (var entry in sampleEntries)
            {
                await database.SaveEntryAsync(entry);
            }
        }
    }
}
