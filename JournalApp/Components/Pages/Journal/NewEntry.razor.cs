using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using JournalApp.Services;
using Microsoft.JSInterop;

namespace JournalApp.Components.Pages.Journal
{
    /// <summary>
    /// Code-behind for the NewEntry page.
    /// Handles the creation, editing, and saving of journal entries.
    /// Manages UI state for mood selection, tagging, and form validation.
    /// </summary>
    public partial class NewEntry : ComponentBase
    {
        [Inject]
        public IJournalService JournalService { get; set; } = default!;

        [Inject]
        public NavigationManager Navigation { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Now;
        public string CreatedAt { get; set; } = DateTime.Now.ToString("g");
        public string UpdatedAt { get; set; } = DateTime.Now.ToString("g");
        public string Category { get; set; } = "";
        public string PrimaryMood { get; set; } = "";
        public List<string> SecondaryMoods { get; set; } = new();
        public List<string> Tags { get; set; } = new();
        public string TagInput { get; set; } = "";
        protected bool _showAllTags = false;
        protected bool _entryExists = false;
        public string LastError { get; set; } = ""; // Debugging helper
        [SupplyParameterFromQuery]
        public string? DateParam { get; set; }

        [Parameter]
        public string? EntryId { get; set; }
        
        private string _existingId = "";

        protected override async Task OnParametersSetAsync()
        {
            try 
            {
                // Priority 1: Load by ID (from Edit Route)
                if (!string.IsNullOrEmpty(EntryId))
                {
                    await LoadEntryById(EntryId);
                    StateHasChanged();
                    return;
                }

                // Priority 2: Load by Query Date (Attempt manual parse if DateParam is null)
                DateTime? targetDate = null;

                if (!string.IsNullOrEmpty(DateParam) && 
                    DateTime.TryParseExact(DateParam, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out var parsed))
                {
                    targetDate = parsed;
                }
                else
                {
                    // Fallback: Manual URI parsing (Dependency-free manner)
                    var query = Navigation.ToAbsoluteUri(Navigation.Uri).Query;
                    if (!string.IsNullOrEmpty(query))
                    {
                        var queryParams = System.Web.HttpUtility.ParseQueryString(query);
                        var dateVal = queryParams["date"];
                        
                        if (!string.IsNullOrEmpty(dateVal) && DateTime.TryParseExact(dateVal, "yyyy-MM-dd", 
                            System.Globalization.CultureInfo.InvariantCulture, 
                            System.Globalization.DateTimeStyles.None, out var manualParsed))
                        {
                            targetDate = manualParsed;
                        }
                    }
                }

                if (targetDate.HasValue)
                {
                    Date = targetDate.Value;
                    await CheckEntryExists();
                }
                else
                {
                     // Priority 3: Default to Today
                     // But! If we are reusing the component, Date might be stale.
                     // On fresh load, Date is DateTime.Now.
                     // Ensure we check existance for WHATEVER date is current.
                    await CheckEntryExists();
                }

                StateHasChanged(); 
            }
            catch (Exception ex)
            {
                LastError = $"Startup Error: {ex.Message}";
            }
        }

        protected async Task LoadEntryById(string id)
        {
            var existingEntry = await JournalService.GetEntryByIdAsync(id);
            if (existingEntry != null)
            {
                // Populate fields
                _existingId = existingEntry.Id;
                Title = existingEntry.Title;
                Content = existingEntry.Content;
                Date = existingEntry.Date;
                
                // Load timestamps from existing entry
                CreatedAt = existingEntry.CreatedDate.ToString("g");
                UpdatedAt = existingEntry.ModifiedDate.ToString("g");
                
                // Mood defaults
                PrimaryMood = _moodOptions.FirstOrDefault(m => m.Label == existingEntry.MoodLabel)?.Value ?? "";
                
                // Load Secondary Moods
                if (!string.IsNullOrEmpty(existingEntry.SecondaryMood))
                {
                    SecondaryMoods = existingEntry.SecondaryMood
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();
                }
                else
                {
                    SecondaryMoods = new List<string>();
                }
                
                Tags = existingEntry.Tags.Select(t => t.Name).ToList() ?? new List<string>();
                
                _entryExists = true; // It exists
            }
        }

        protected async Task CheckEntryExists()
        {
            var existingEntry = await JournalService.GetEntryByDateAsync(Date);
            if (existingEntry != null)
            {
                // Load existing data for editing
                _existingId = existingEntry.Id;
                Title = existingEntry.Title;
                Content = existingEntry.Content;
                
                // Load timestamps from existing entry
                CreatedAt = existingEntry.CreatedDate.ToString("g");
                UpdatedAt = existingEntry.ModifiedDate.ToString("g");
                
                // Mood defaults
                PrimaryMood = _moodOptions.FirstOrDefault(m => m.Label == existingEntry.MoodLabel)?.Value ?? "";
                
                // Load Secondary Moods (Deserialization)
                if (!string.IsNullOrEmpty(existingEntry.SecondaryMood))
                {
                    SecondaryMoods = existingEntry.SecondaryMood
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToList();
                }
                else
                {
                    SecondaryMoods = new List<string>();
                }
                
                Tags = existingEntry.Tags.Select(t => t.Name).ToList() ?? new List<string>();
                
                // We don't show the blocking warning anymore, we just let them edit.
                _entryExists = false; 
            }
            else
            {
                // No entry exists for this date -> New Entry
                _existingId = "";
                _entryExists = false; 
                
                // CRITICAL: Clear strict form state to avoid showing "Today's" data on "Yesterday's" new page
                Title = "";
                Content = "";
                PrimaryMood = "";
                SecondaryMoods.Clear();
                Tags.Clear();
                TagInput = "";
                LastError = "";
                
                // Reset timestamps for new entry
                CreatedAt = DateTime.Now.ToString("g");
                UpdatedAt = DateTime.Now.ToString("g");
            }
            StateHasChanged();
        }

        // ... (Middle code unchanged) ...

        protected async Task SaveEntry()
        {
            try
            {
                LastError = "Saving...";
                await InvokeAsync(StateHasChanged);

                // For updates, fetch the existing entry to preserve CreatedDate
                JournalApp.Models.JournalEntry? existingEntry = null;
                if (!string.IsNullOrEmpty(_existingId))
                {
                    existingEntry = await JournalService.GetEntryByIdAsync(_existingId);
                }

                // Build the entry object
                var entry = new JournalApp.Models.JournalEntry
                {
                    Id = _existingId,
                    Title = Title,
                    Content = Content,
                    Date = Date,
                    Preview = Content.Length > 100 ? Content.Substring(0, 100) + "..." : Content,
                    UserId = "default-user",
                    SecondaryMood = string.Join(",", SecondaryMoods),
                    // Preserve CreatedDate if updating, otherwise let database set it
                    CreatedDate = existingEntry?.CreatedDate ?? DateTime.Now
                };

                // Get or create Mood
                var selectedMoodOption = _moodOptions.FirstOrDefault(m => m.Value == PrimaryMood);
                if (selectedMoodOption != null)
                {
                    var moods = await JournalService.GetMoodsAsync();
                    var mood = moods.FirstOrDefault(m => m.Name == selectedMoodOption.Label);
                    if (mood != null)
                    {
                        entry.MoodId = mood.Id;
                    }
                }

                // Convert string tags to Tag objects
                entry.Tags = new List<JournalApp.Models.Tag>();
                foreach (var tagName in Tags)
                {
                    var tag = await JournalService.GetOrCreateTagAsync(tagName);
                    if (tag != null)
                    {
                        entry.Tags.Add(tag);
                    }
                }

                await JournalService.SaveEntryAsync(entry);
                Navigation.NavigateTo("/");
            }
            catch (Exception ex)
            {
                LastError = $"Save Error: {ex.Message} -> {ex.InnerException?.Message}";
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task DeleteEntry()
        {
            try
            {
                if (string.IsNullOrEmpty(_existingId)) return;

                bool confirm = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this entry?");
                if (!confirm) return;

                LastError = "Deleting...";
                await InvokeAsync(StateHasChanged);

                await JournalService.DeleteEntryAsync(_existingId);
                
                Navigation.NavigateTo("/");
            }
            catch (Exception ex)
            {
                LastError = $"Delete Error: {ex.Message}";
                await InvokeAsync(StateHasChanged);
            }
        }

        // --- Options Data ---
        protected List<string> _categoryOptions = new() { "Personal", "Work", "Health", "Travel", "Reflection", "Goals", "Gratitude", "Dreams" };
        
        protected class MoodOption { public string Value { get; set; } = ""; public string Label { get; set; } = ""; public string Emoji { get; set; } = ""; }
        protected List<MoodOption> _moodOptions = new()
        {
            new MoodOption { Value = "happy", Label = "Happy", Emoji = "😊" },
            new MoodOption { Value = "excited", Label = "Excited", Emoji = "🎉" },
            new MoodOption { Value = "calm", Label = "Calm", Emoji = "😌" },
            new MoodOption { Value = "neutral", Label = "Neutral", Emoji = "😐" },
            new MoodOption { Value = "sad", Label = "Sad", Emoji = "😢" },
            new MoodOption { Value = "anxious", Label = "Anxious", Emoji = "😰" },
            new MoodOption { Value = "angry", Label = "Angry", Emoji = "😠" },
            new MoodOption { Value = "grateful", Label = "Grateful", Emoji = "🙏" }
        };

        protected class SecondaryMood { public string Value { get; set; } = ""; public string Emoji { get; set; } = ""; public string Type { get; set; } = ""; }
        protected List<SecondaryMood> _secondaryMoods = new()
        {
            new SecondaryMood { Value = "happy", Emoji = "😊", Type = "positive" },
            new SecondaryMood { Value = "grateful", Emoji = "🙏", Type = "positive" },
            new SecondaryMood { Value = "excited", Emoji = "😄", Type = "positive" },
            new SecondaryMood { Value = "blessed", Emoji = "✨", Type = "positive" },
            new SecondaryMood { Value = "calm", Emoji = "😐", Type = "neutral" },
            new SecondaryMood { Value = "thinking", Emoji = "🤔", Type = "neutral" },
            new SecondaryMood { Value = "surprised", Emoji = "😮", Type = "neutral" },
            new SecondaryMood { Value = "tired", Emoji = "😴", Type = "neutral" },
            new SecondaryMood { Value = "sad", Emoji = "😢", Type = "negative" },
            new SecondaryMood { Value = "anxious", Emoji = "😰", Type = "negative" },
            new SecondaryMood { Value = "crying", Emoji = "😭", Type = "negative" },
            new SecondaryMood { Value = "worried", Emoji = "😟", Type = "negative" }
        };

        protected List<string> _suggestedTags = new() { "Work", "Career", "Studies", "Family", "Friends", "Relationships", "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies", "Travel" };


        //     --- Logic ---

        protected int GetWordCount()
        {
            if (string.IsNullOrWhiteSpace(Content)) return 0;
            return Content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        protected async Task<bool> EntryExists(DateTime date)
        {
            return await JournalService.EntryExists(date);
        }

        protected async Task ToggleSecondaryMood(string moodValue)
        {
            try
            {
                LastError = ""; // Clear errors
                if (SecondaryMoods.Contains(moodValue))
                {
                    SecondaryMoods.Remove(moodValue);
                }
                else if (SecondaryMoods.Count < 2)
                {
                    SecondaryMoods.Add(moodValue);
                }
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                LastError = $"Mood Error: {ex.Message}";
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task AddTag()
        {
            try
            {
                LastError = "";
                if (!string.IsNullOrWhiteSpace(TagInput) && !Tags.Contains(TagInput.Trim()))
                {
                    Tags.Add(TagInput.Trim());
                    TagInput = "";
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (Exception ex)
            {
                LastError = $"Add Tag Error: {ex.Message}";
                await InvokeAsync(StateHasChanged);
            }
        }

        protected void AddSuggestedTag(string tag)
        {
            Console.WriteLine($"[NewEntry] AddSuggestedTag called: {tag}");
            try
            {
                if (!Tags.Contains(tag))
                {
                    Tags.Add(tag);
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NewEntry] Error in AddSuggestedTag: {ex.Message}");
            }
        }

        protected void HandleTagKeyUp(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                AddTag();
            }
        }

        protected void ShowAllTags()
        {
            _showAllTags = true;
            InvokeAsync(StateHasChanged);
        }

        protected async Task InsertMarkdown(string prefix, string suffix)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("window.editor.insertText", "journal-editor", prefix, suffix);
            }
            catch
            {
                 // Ignore JS errors if editor not initialized
            }
        }

        protected void RemoveTag(string tag)
        {
            Tags.Remove(tag);
            StateHasChanged();
        }

        protected void GoBack()
        {
            Navigation.NavigateTo("/");
        }


    }
}
